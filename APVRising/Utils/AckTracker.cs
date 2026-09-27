using APVRising;
using System;
using System.Collections.Concurrent;

namespace VRisingArchipelago
{
    /// <summary>
    /// Tracks outstanding server->client batches awaiting a client ##ACK#{version}## and
    /// drives retry via DeferredActionSystem. A batch that never acks is retried up to
    /// MaxRetries times, then treated as permanently failed (onGiveUp decides fallback,
    /// e.g. falling back to a full snapshot/resync, or just proceeding).
    /// </summary>
    public static class AckTracker
    {
        private class PendingBatch
        {
            public int Version;
            public Action ResendAction;
            public Action OnGiveUp;
            public Action OnAck;
            public int Attempts;
        }

        public const int MaxRetries = 5;
        public const float RetryDelaySeconds = 2.0f;

        // steamId -> version -> pending batch
        private static readonly ConcurrentDictionary<ulong, ConcurrentDictionary<int, PendingBatch>> _pending = new();

        /// <summary>
        /// Register a batch as sent and awaiting ack. Schedules the first retry.
        /// resendAction re-sends the same versioned payload to this player.
        /// onGiveUp is invoked once MaxRetries is exhausted with no ack.
        /// onAck is invoked once the client's ack for this exact version arrives.
        /// </summary>
        public static void TrackBatch(ulong steamId, int version, Action resendAction, Action onGiveUp, Action onAck = null)
        {
            var userBatches = _pending.GetOrAdd(steamId, _ => new ConcurrentDictionary<int, PendingBatch>());

            var batch = new PendingBatch
            {
                Version = version,
                ResendAction = resendAction,
                OnGiveUp = onGiveUp,
                OnAck = onAck,
                Attempts = 0
            };

            userBatches[version] = batch;

            ScheduleRetryCheck(steamId, version);

            Plugin.BepinLogger.LogInfo($"[AckTracker] Tracking batch v{version} for {steamId}");
        }

        /// <summary>
        /// Call when a client's ##ACK#{version}## arrives. Cancels any pending retry for that
        /// version, removes it from tracking, and fires onAck if one was registered.
        /// </summary>
        public static void Acknowledge(ulong steamId, int version)
        {
            if (_pending.TryGetValue(steamId, out var userBatches))
            {
                if (userBatches.TryRemove(version, out var batch))
                {
                    DeferredActionSystem.CancelGroup(GroupKey(steamId, version));
                    Plugin.BepinLogger.LogInfo($"[AckTracker] ACK'd batch v{version} for {steamId}");
                    batch.OnAck?.Invoke();
                }
                else
                {
                    // Ack for a version we're not tracking (already acked, already gave up,
                    // or stale/duplicate ack). Not an error, just log for visibility.
                    Plugin.BepinLogger.LogInfo($"[AckTracker] Received ACK for untracked v{version} from {steamId}");
                }
            }
        }

        /// <summary>
        /// Cancel all pending batches for a player, e.g. on disconnect.
        /// </summary>
        public static void ClearPlayer(ulong steamId)
        {
            if (_pending.TryRemove(steamId, out var userBatches))
            {
                foreach (var version in userBatches.Keys)
                    DeferredActionSystem.CancelGroup(GroupKey(steamId, version));

                Plugin.BepinLogger.LogInfo($"[AckTracker] Cleared all pending batches for {steamId}");
            }
        }

        public static int PendingCountFor(ulong steamId)
        {
            return _pending.TryGetValue(steamId, out var userBatches) ? userBatches.Count : 0;
        }

        private static void ScheduleRetryCheck(ulong steamId, int version)
        {
            DeferredActionSystem.Schedule(
                action: () => AttemptRetry(steamId, version),
                delaySeconds: RetryDelaySeconds,
                maxRetries: 0, // we drive our own retry loop below, not DeferredActionSystem's
                group: GroupKey(steamId, version)
            );
        }

        private static void AttemptRetry(ulong steamId, int version)
        {
            if (!_pending.TryGetValue(steamId, out var userBatches)) return;
            if (!userBatches.TryGetValue(version, out var batch)) return; // already acked

            batch.Attempts++;

            if (batch.Attempts > MaxRetries)
            {
                Plugin.BepinLogger.LogWarning(
                    $"[AckTracker] Batch v{version} for {steamId} never acked after {MaxRetries} retries. Giving up.");

                userBatches.TryRemove(version, out _);
                batch.OnGiveUp?.Invoke();
                return;
            }

            Plugin.BepinLogger.LogInfo(
                $"[AckTracker] Resending batch v{version} to {steamId} (attempt {batch.Attempts}/{MaxRetries})");

            batch.ResendAction();
            ScheduleRetryCheck(steamId, version);
        }

        private static string GroupKey(ulong steamId, int version) => $"ack:{steamId}:{version}";
    }
}
