using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace VRisingArchipelago
{
    /// <summary>
    /// A deferred action scheduler that resolves V Rising ECS buffer race conditions.
    ///
    /// Problem: When locking/unlocking items in response to Archipelago messages, the game's
    /// ECS structural change buffers may be mid-flush. Writing to them immediately causes
    /// corruption, duplicate components, or silent no-ops depending on timing.
    ///
    /// Solution: Queue actions with an optional delay (default 1 second). The scheduler
    /// drains the queue during a safe Unity update phase, after ECS buffers have settled.
    ///
    /// Usage:
    ///   // Fire-and-forget with default delay
    ///   DeferredActionSystem.Schedule(() => UnlockRecipe(player, itemGuid));
    ///
    ///   // Custom delay
    ///   DeferredActionSystem.Schedule(() => LockItem(player, itemGuid), delaySeconds: 0.5f);
    ///
    ///   // With a retry policy (retries up to 3 times if the action throws)
    ///   DeferredActionSystem.Schedule(() => UnlockRecipe(player, itemGuid), maxRetries: 3);
    ///
    ///   // Cancel all pending actions for a player (e.g. on disconnect)
    ///   DeferredActionSystem.CancelGroup("player_steam_id");
    /// </summary>
    public class DeferredActionSystem : MonoBehaviour
    {
        // ── Configuration ────────────────────────────────────────────────────────────

        /// <summary>Default delay before executing a scheduled action (seconds).</summary>
        public static float DefaultDelay = 1.0f;

        /// <summary>Maximum number of actions drained per frame to avoid frame spikes.</summary>
        public static int MaxActionsPerFrame = 10;

        // ── Internals ────────────────────────────────────────────────────────────────

        private static DeferredActionSystem _instance;
        private static ManualLogSource _log;

        // Thread-safe pending list; actions are moved to _ready each frame.
        private readonly List<DeferredAction> _pending = new();
        private readonly List<DeferredAction> _ready   = new();

        // ── Bootstrap ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Call once from your plugin's Awake/Load to initialise the system.
        /// </summary>
        public static void Initialise(ManualLogSource log)
        {
            if (_instance != null) return;

            _log = log;

            var go = new GameObject("DeferredActionSystem");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent(Il2CppType.Of<DeferredActionSystem>()) as DeferredActionSystem;

            _log.LogInfo("[DeferredActionSystem] Initialised.");
        }

        // ── Public API ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Schedule an action to run after <paramref name="delaySeconds"/> seconds.
        /// </summary>
        /// <param name="action">The work to perform (ECS writes, component changes, etc.).</param>
        /// <param name="delaySeconds">Seconds to wait before execution. Defaults to <see cref="DefaultDelay"/>.</param>
        /// <param name="maxRetries">How many times to retry if the action throws. 0 = no retry.</param>
        /// <param name="group">Optional tag used with <see cref="CancelGroup"/> to bulk-cancel actions.</param>
        public static void Schedule(
            Action  action,
            float   delaySeconds = -1f,
            int     maxRetries   = 0,
            string  group        = null)
        {
            if (_instance == null)
                throw new InvalidOperationException(
                    "[DeferredActionSystem] Not initialised. Call DeferredActionSystem.Initialise() first.");

            if (delaySeconds < 0f) delaySeconds = DefaultDelay;

            var deferred = new DeferredAction
            {
                Action      = action,
                ExecuteAt   = Time.unscaledTime + delaySeconds,
                MaxRetries  = maxRetries,
                Retries     = 0,
                Group       = group,
                Cancelled   = false,
            };

            lock (_instance._pending)
                _instance._pending.Add(deferred);

            _log?.LogDebug(
                $"[DeferredActionSystem] Scheduled action " +
                $"(delay={delaySeconds:F2}s, group={group ?? "none"}, retries={maxRetries}).");
        }

        /// <summary>
        /// Schedules multiple actions with the same delay, fired sequentially.
        /// Useful when a single Archipelago item unlocks several recipes.
        /// </summary>
        public static void ScheduleBatch(
            IEnumerable<Action> actions,
            float  delaySeconds = -1f,
            string group        = null)
        {
            if (delaySeconds < 0f) delaySeconds = DefaultDelay;

            // Stagger each action slightly so they don't all slam ECS at once.
            float stagger = 0f;
            foreach (var action in actions)
            {
                Schedule(action, delaySeconds + stagger, group: group);
                stagger += 0.05f; // 50 ms between each
            }
        }

        /// <summary>
        /// Cancel all pending actions that belong to <paramref name="group"/>.
        /// Safe to call from any thread.
        /// </summary>
        public static void CancelGroup(string group)
        {
            if (group == null) return;

            lock (_instance._pending)
            {
                foreach (var a in _instance._pending)
                    if (a.Group == group)
                        a.Cancelled = true;
            }

            _log?.LogInfo($"[DeferredActionSystem] Cancelled all actions in group '{group}'.");
        }

        /// <summary>Returns the number of actions currently waiting to execute.</summary>
        public static int PendingCount
        {
            get
            {
                if (_instance == null) return 0;
                lock (_instance._pending) return _instance._pending.Count;
            }
        }

        // ── Unity Update Loop ────────────────────────────────────────────────────────

        private void Update()
        {
            float now = Time.unscaledTime;

            // Move ready actions out of the shared list under the lock, then execute
            // outside the lock so action code can itself call Schedule() without deadlock.
            lock (_pending)
            {
                for (int i = _pending.Count - 1; i >= 0; i--)
                {
                    var a = _pending[i];
                    if (a.Cancelled || now >= a.ExecuteAt)
                    {
                        _ready.Add(a);
                        _pending.RemoveAt(i);
                    }
                }
            }

            int executed = 0;
            for (int i = 0; i < _ready.Count && executed < MaxActionsPerFrame; i++, executed++)
            {
                var a = _ready[i];
                _ready.RemoveAt(i);
                i--;

                if (a.Cancelled) continue;

                ExecuteAction(a);
            }

            // If we hit the per-frame cap, push leftovers back with zero extra delay.
            if (_ready.Count > 0)
            {
                lock (_pending)
                    _pending.AddRange(_ready);
                _ready.Clear();
            }
        }

        private void ExecuteAction(DeferredAction a)
        {
            try
            {
                a.Action();
            }
            catch (Exception ex)
            {
                if (a.Retries < a.MaxRetries)
                {
                    a.Retries++;
                    float backoff = DefaultDelay * a.Retries; // linear back-off
                    a.ExecuteAt   = Time.unscaledTime + backoff;
                    a.Cancelled   = false;

                    _log?.LogWarning(
                        $"[DeferredActionSystem] Action failed (attempt {a.Retries}/{a.MaxRetries}). " +
                        $"Retrying in {backoff:F2}s. Error: {ex.Message}");

                    lock (_pending) _pending.Add(a);
                }
                else
                {
                    _log?.LogError(
                        $"[DeferredActionSystem] Action failed permanently after {a.Retries} retries.\n{ex}");
                }
            }
        }

        // ── Inner Types ──────────────────────────────────────────────────────────────

        private class DeferredAction
        {
            public Action Action;
            public float  ExecuteAt;
            public int    MaxRetries;
            public int    Retries;
            public string Group;
            public bool   Cancelled;
        }
    }
}
