using System.Collections.Generic;
using System.Linq;
using APVRising;
using APVRising.Archipelago;
using APVRising.Data;
using APVRising.Hooks;
using APVRising.Utils;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace VRisingArchipelago
{
    /// <summary>
    /// Sends the server-wide set of AP-configured locations to a connecting player as
    /// ack-tracked chunked batches, replacing the old blind-timer "done configuring" signal.
    ///
    /// Plugin.APClient / configured locations are the same for the whole server session
    /// (not per-user), so the code list is computed once and reused for every connection.
    ///
    /// Wire format: ##CFGLOC#{version}#{count}#{3-digit code}{3-digit code}...##
    /// (no action char needed — "configured" is the only action here).
    /// </summary>
    public static class ConfiguredLocationsSync
    {
        private const int MaxCodesPerMessage = 100; // 100*3 = 300 chars body

        private static List<string> _cachedCodes;
        private static readonly object _cacheLock = new();

        private static List<string> GetOrBuildCodes()
        {
            lock (_cacheLock)
            {
                if (_cachedCodes != null) return _cachedCodes;

                var codes = new List<string>();
                foreach (var kvp in DataDicts.TechToPrefab)
                {
                    if (!DataDicts.EntityNameToAPLocation.TryGetValue(kvp.Key, out var locationName))
                        continue;
                    if (!Plugin.APClient.IsConfiguredLocation(locationName))
                        continue;

                    ArchipelagoData.ConfiguredLocations.Add(kvp.Value._Value);

                    if (!BatchCodeTable.PrefabToCode.TryGetValue(kvp.Value, out var code))
                    {
                        Plugin.BepinLogger.LogError($"[AP] No batch code for configured location {kvp.Key} ({kvp.Value._Value})");
                        continue;
                    }
                    codes.Add(code);
                }

                Plugin.BepinLogger.LogInfo($"[AP] Built configured-location code cache: {codes.Count} entries");
                _cachedCodes = codes;
                return _cachedCodes;
            }
        }

        /// <summary>
        /// Call this if the AP session reconnects to a different seed/room where
        /// IsConfiguredLocation could return different results, to force a rebuild.
        /// </summary>
        public static void InvalidateCache()
        {
            lock (_cacheLock) _cachedCodes = null;
        }

        public static void SendToUser(Entity userEntity)
        {
            var em = Plugin.EntityManager;
            var user = em.GetComponentData<User>(userEntity);
            ulong steamId = user.PlatformId;
            var codes = GetOrBuildCodes();

            if (codes.Count == 0)
            {
                ChatMessage.NotifyClientDoneConfiguring(user);
                return;
            }

            var chunks = codes.Chunk(MaxCodesPerMessage).ToList();
            int remaining = chunks.Count;
            var gate = new object();

            void MaybeComplete()
            {
                bool done;
                lock (gate) { remaining--; done = remaining <= 0; }
                if (done)
                {
                    Plugin.BepinLogger.LogInfo($"[AP] Config-location sync complete for {steamId}");
                    ChatMessage.NotifyClientDoneConfiguring(user);
                }
            }

            foreach (var chunk in chunks)
            {
                var version = BatchVersioning.NextVersion(steamId);
                var payload = $"##CFGLOC#{version}#{chunk.Length}#{string.Concat(chunk)}##";

                void Send()
                {
                    var fixedMsg = new FixedString512Bytes(payload);
                    ServerChatUtils.SendSystemMessageToClient(em, user, ref fixedMsg);
                }

                Plugin.BepinLogger.LogInfo($"[AP] Sending CFGLOC v{version} ({chunk.Length} codes) to {steamId}");
                Send();

                AckTracker.TrackBatch(
                    steamId, version, resendAction: Send,
                    onGiveUp: () =>
                    {
                        Plugin.BepinLogger.LogWarning($"[AP] CFGLOC v{version} never acked by {steamId}. Proceeding anyway.");
                        MaybeComplete();
                    },
                    onAck: MaybeComplete
                );
            }
        }
    }
}
