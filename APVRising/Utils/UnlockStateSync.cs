using System.Collections.Generic;
using System.Linq;
using APVRising;
using APVRising.Archipelago;
using APVRising.Data;
using APVRising.Hooks;
using APVRising.Utils;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace VRisingArchipelago
{
    /// <summary>
    /// Sends the server-wide RESOLVED unlock/lock state to a connecting player as
    /// ack-tracked chunked batches, replacing the timer-based Capture/Restore/Reconcile
    /// flow for initial sync.
    ///
    /// Unlike a history log, this tracks only the CURRENT state per item: a Dictionary
    /// keyed by 3-digit code, valued by the most recent action char applied to it ('U',
    /// 'L', 'S', 'K', 'A', 'P'). A prefab that's been unlocked then re-locked (e.g. a
    /// research re-roll) is sent once, in its current state — not as a two-step replay.
    ///
    /// Seeded once per server session from ArchipelagoData.ReceivedChecks (durable record
    /// of AP-delivered items -> treated as 'U'/'S'), then kept current via RecordState
    /// calls wired into every place the server actually applies an unlock OR a lock:
    ///   - ArchipelagoItemReceive.cs (unlocks, from AP items)
    ///   - DiscoverResearchHandler.cs / TechUnlockHandler.cs (locks, from research rolls)
    ///
    /// Wire format: same 4-char {action}{code} entries as the live BatchAccumulator
    /// stream, under the tag ##UNLOCKSYNC#{version}#{count}#{entry}{entry}...##
    /// </summary>
    public static class UnlockStateSync
    {
        private const int MaxEntriesPerMessage = 100; // 100*4 = 400 chars body

        // code -> most recent action char applied to that prefab
        private static readonly Dictionary<string, char> _state = new();
        private static readonly object _cacheLock = new();
        /// <summary>
        /// Call once per state-changing event as it's actually applied server-side —
        /// unlocks (ArchipelagoItemReceive.cs) AND locks (DiscoverResearchHandler.cs /
        /// TechUnlockHandler.cs) both call this. Overwrites any prior recorded state for
        /// that prefab, since only the current state matters for a fresh sync.
        ///
        /// Call once per distinct state-change EVENT, not once per connected player
        /// affected by it — if the apply site loops over all connected users, call this
        /// outside that inner loop.
        /// </summary>
        public static void RecordState(char actionChar, PrefabGUID prefab)
        {
            if (!BatchCodeTable.PrefabToCode.TryGetValue(prefab, out var code))
            {
                Plugin.BepinLogger.LogError($"[AP] No batch code for prefab {prefab._Value}, cannot record state.");
                return;
            }

            lock (_cacheLock)
                _state[code] = actionChar;
        }

        /// <summary>
        /// Merges ArchipelagoData.ReceivedChecks - the durable, server-wide record of every item
        /// guid AP has actually delivered this session - into the state map as 'U'/'S'.
        ///
        /// Unlike the original one-shot design, this re-merges every time it's called (cheap: a
        /// single pass over ReceivedChecks with a dictionary write per entry). It is purely
        /// additive/overwriting for the codes it touches, so it never erases a lock ('L'/'K'/'P')
        /// that RecordState captured live for a prefab that ISN'T in ReceivedChecks (a locked
        /// prefab is by definition not something the player currently owns, so it's never added
        /// to ReceivedChecks in the first place - see LockTechForPlayer). That makes it safe to
        /// call after every Resync(), which is what ArchipelagoItemSystem.OnUpdate does: Resync's
        /// revoke pass records any new locks live via RecordState, and this call then folds in
        /// anything Resync's "grant missing" pass only added to ReceivedChecks without also
        /// calling UnlockTechForPlayer (see the note on that pass in ArchipelagoClient.Resync).
        ///
        /// Achievement unlocks are NOT guaranteed to appear in ReceivedChecks if they're
        /// triggered by in-game events rather than direct AP item grants; UnlockAchievementForPlayer
        /// records 'A' live for those instead, independent of this seed.
        /// </summary>
        public static void SeedFromReceivedChecks()
        {
            lock (_cacheLock)
            {
                foreach (int guid in ArchipelagoData.ReceivedChecks)
                {
                    var prefab = new PrefabGUID(guid);

                    if (!DataDicts.PrefabToTech.TryGetValue(prefab, out var techKey))
                        continue; // not a tech/spell prefab, skip

                    char action = techKey.StartsWith("AB") ? 'S' : 'U';

                    if (!BatchCodeTable.PrefabToCode.TryGetValue(prefab, out var code))
                    {
                        Plugin.BepinLogger.LogError($"[AP] No batch code for prefab {prefab._Value}, cannot seed state.");
                        continue;
                    }
                    _state[code] = action;
                }

                Plugin.BepinLogger.LogInfo($"[AP] Merged ReceivedChecks into unlock-state cache: {_state.Count} entries total");
            }
        }

        public static void SendToUser(Entity userEntity)
        {
            var em = Plugin.EntityManager;
            var user = em.GetComponentData<User>(userEntity);
            var userData = Plugin.Server.EntityManager.GetComponentData<User>(userEntity);

            var steamId = userData.PlatformId;

            List<string> entries;
            lock (_cacheLock)
                entries = _state.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Value}{kvp.Key}").ToList();

            if (entries.Count == 0)
            {
                ChatMessage.NotifyClientDoneReconcile(user);
                return;
            }

            var chunks = entries.Chunk(MaxEntriesPerMessage).ToList();
            int remaining = chunks.Count;
            var gate = new object();

            void MaybeComplete()
            {
                bool done;
                lock (gate) { remaining--; done = remaining <= 0; }
                if (done)
                {
                    Plugin.BepinLogger.LogInfo($"[AP] Unlock-state sync complete for {steamId}");
                    ChatMessage.NotifyClientDoneReconcile(user);
                }
            }

            foreach (var chunk in chunks)
            {
                var version = BatchVersioning.NextVersion(steamId);
                var payload = $"##UNLOCKSYNC#{version}#{chunk.Length}#{string.Concat(chunk)}##";

                void Send()
                {
                    var fixedMsg = new FixedString512Bytes(payload);
                    ServerChatUtils.SendSystemMessageToClient(em, user, ref fixedMsg);
                }

                Plugin.BepinLogger.LogInfo($"[AP] Sending UNLOCKSYNC v{version} ({chunk.Length} entries) to {steamId}");
                Send();

                AckTracker.TrackBatch(
                    steamId, version, resendAction: Send,
                    onGiveUp: () =>
                    {
                        Plugin.BepinLogger.LogWarning($"[AP] UNLOCKSYNC v{version} never acked by {steamId}. Proceeding anyway.");
                        MaybeComplete();
                    },
                    onAck: MaybeComplete
                );
            }
        }
    }
}