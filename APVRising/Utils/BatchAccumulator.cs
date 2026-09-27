using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Entities;
using APVRising.Archipelago;
using APVRising.Data;
using APVRising.Hooks;
using APVRising.Services;
using APVRising.Utils;
using VRisingArchipelago;
using APVRising;
using ProjectM;
using Unity.Collections;
using APVRising.Systems;
namespace ApVRising
{
    /// <summary>
    /// Accumulates outbound control-message entries per player and flushes them as a
    /// single versioned, ack-tracked batch instead of one message per action.
    ///
    /// Wire format per entry: {actionChar}{3-digit code} — 4 fixed-width chars, no
    /// delimiter needed between entries.
    ///   U = unlock tech      L = lock tech
    ///   S = unlock spell     K = lock spell
    ///   A = unlock achievement   P = lock prog
    ///
    /// Message: ##BATCH#{version}#{count}#{entry}{entry}{entry}...##
    /// </summary>
    public static class BatchAccumulator
    {
        private const float FlushDelaySeconds = 0.15f;
        private const int MaxEntriesPerMessage = 100; // 100*4 = 400 chars, comfortably under 512

        private static readonly ConcurrentDictionary<ulong, List<string>> _pending = new();
        private static readonly ConcurrentDictionary<ulong, bool> _flushScheduled = new();

        public static void Enqueue(Entity userEntity, char actionChar, int guid)
        {
            if (!BatchCodeTable.PrefabToCode.TryGetValue(new PrefabGUID(guid), out var code))
            {
                Plugin.BepinLogger.LogError($"[AP] No code registered for guid {guid}, cannot send.");
                return;
            }

            var entry = $"{actionChar}{code}"; // 4 chars, e.g. "U056"
            var userData = Plugin.Server.EntityManager.GetComponentData<User>(userEntity);

            var steamId = userData.PlatformId;

            var list = _pending.GetOrAdd(steamId, _ => new List<string>());

            lock (list) list.Add(entry);

            // Only schedule one flush per player per window, regardless of how many
            // Enqueue calls land in it.
            if (_flushScheduled.TryAdd(steamId, true))
            {
                DeferredActionSystem.Schedule(
                    action: () => Flush(steamId, userEntity),
                    delaySeconds: FlushDelaySeconds,
                    maxRetries: 0,
                    group: $"batchflush:{steamId}"
                );
            }
        }

        private static void Flush(ulong steamId, Entity userEntity)
        {
            _flushScheduled.TryRemove(steamId, out _);

            if (!_pending.TryRemove(steamId, out var entries) || entries.Count == 0)
                return;

            var em = Plugin.EntityManager;
            var user = em.GetComponentData<User>(userEntity);

            foreach (var chunk in entries.Chunk(MaxEntriesPerMessage))
            {
                var version = BatchVersioning.NextVersion(steamId);
                var payload = $"##BATCH#{version}#{chunk.Length}#{string.Concat(chunk)}##";

                void Send()
                {
                    var fixedMsg = new FixedString512Bytes(payload);
                    ServerChatUtils.SendSystemMessageToClient(em, user, ref fixedMsg);
                }

                Plugin.BepinLogger.LogInfo($"[AP] Sending batch v{version} ({chunk.Length} entries) to {steamId}");
                Send();

                AckTracker.TrackBatch(
                    steamId,
                    version,
                    resendAction: Send,
                    onGiveUp: () =>
                    {
                        Plugin.BepinLogger.LogWarning(
                            $"[AP] Batch v{version} never acked by {steamId}. Requesting resync.");
                        ArchipelagoItemSystem.PendingResync = true;
                    });
            }
        }
    }
}
