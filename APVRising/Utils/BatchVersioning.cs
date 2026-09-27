using System.Collections.Concurrent;

namespace VRisingArchipelago
{
    /// <summary>
    /// Assigns a monotonically increasing version number per player for outbound
    /// AP control messages. Used by AckTracker to key pending batches and let the
    /// client detect stale/out-of-order deliveries.
    /// </summary>
    public static class BatchVersioning
    {
        private static readonly ConcurrentDictionary<ulong, int> _counters = new();

        public static int NextVersion(ulong steamId) =>
            _counters.AddOrUpdate(steamId, 1, (_, current) => current + 1);

        public static void Reset(ulong steamId) => _counters.TryRemove(steamId, out _);
    }
}