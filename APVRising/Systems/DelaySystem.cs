using System;
using APVRising;
using APVRising.Archipelago;
using APVRising.Hooks;
using APVRising.Utils;
using APVRising.Systems;
using ProjectM;
using ProjectM.Gameplay.Scripting;
using ProjectM.Network;
using Stunlock.Core;
using Stunlock.Core.Animation;
using Unity.Collections;
using Unity.Entities;


namespace VRisingArchipelago;

public static class DelaySystem
{
    /// <summary>
    /// How often the server re-runs Resync() (grant/revoke pass + re-seed and re-broadcast the
    /// full UnlockStateSync state to every connected player) purely on a timer, independent of
    /// any player action. Keeps everyone's client in line with the authoritative AP state even if
    /// a batch was somehow missed, without needing the player to run .sync themselves.
    /// </summary>
    public static float PeriodicResyncIntervalSeconds = 300f;

    private static bool _periodicResyncStarted = false;

    /// <summary>
    /// Starts the recurring resync timer. Safe to call more than once (only the first call does
    /// anything) and safe to call before AP has ever connected -- each tick just checks
    /// ArchipelagoClient.Authenticated and no-ops if it isn't, then reschedules itself regardless,
    /// so it keeps ticking through disconnects/reconnects without needing to be restarted.
    /// </summary>
    public static void StartPeriodicResync()
    {
        if (_periodicResyncStarted) return;
        _periodicResyncStarted = true;
        SchedulePeriodicResyncTick();
    }

    private static void SchedulePeriodicResyncTick()
    {
        DeferredActionSystem.Schedule(
            action: () =>
            {
                if (ArchipelagoClient.Authenticated)
                {
                    Plugin.BepinLogger.LogInfo("[AP] Periodic resync tick");
                    // Goes through the same choke point as a fresh connect / manual .sync:
                    // ArchipelagoItemSystem.OnUpdate calls Resync() then re-seeds and re-sends the
                    // full UnlockStateSync state to every connected player, on the main thread.
                    ArchipelagoItemSystem.PendingResync = true;
                }

                // Reschedule unconditionally -- a skipped tick (not yet connected) just waits for
                // the next one rather than requiring something else to restart the timer.
                SchedulePeriodicResyncTick();
            },
            delaySeconds: PeriodicResyncIntervalSeconds,
            maxRetries: 0 // a recurring timer reschedules itself either way; retrying THIS tick doesn't apply
        );
    }

    public static void StopResearchDeferred()
    {
        Plugin.BepinLogger.LogInfo("StopResearchDeferred");
        DeferredActionSystem.Schedule(
            action: () =>
            {
                ProgressionHandler.setResearch(false);
                // Drain right now rather than waiting for the next ArchipelagoItemSystem.OnUpdate
                // tick, which can lose the race against the very next achievement/kill's Prefix
                // (no delay of its own) flipping IsResearching back to true first.
                ArchipelagoItemSystem.Instance?.DrainPendingItems();
            },
            delaySeconds: 2.5f,
            maxRetries: 3
        );
        DeferredActionSystem.Schedule(
            action: () => ChatMessage.NotifyClientResearch(false),
            delaySeconds: 2.5f,
            maxRetries: 3
        );
    }

    public static void ClientBaselineCapture()
    {
        Plugin.BepinLogger.LogInfo("ClientBaselineCapture");
        DeferredActionSystem.Schedule(
            action: () => ChatMessage.NotifyClientCaptureBaseline(),
            delaySeconds: 3,
            maxRetries: 3
        );
    }

    public static void NotifyClientConfiguredLocations()
    {
        Plugin.BepinLogger.LogInfo("ClientConfiguredLocations");
        DeferredActionSystem.Schedule(
            action: () => ChatMessage.NotifyClientConfiguredLocations(),
            delaySeconds: 4,
            maxRetries: 3
        );
    }

    public static void StopResearchDeferredSlow()
    {
        Plugin.BepinLogger.LogInfo("StopResearchDeferred");
        DeferredActionSystem.Schedule(
            action: () =>
            {
                ProgressionHandler.setResearch(false);
                ArchipelagoItemSystem.Instance?.DrainPendingItems();
            },
            delaySeconds: 8f,
            maxRetries: 3
        );
        DeferredActionSystem.Schedule(
            action: () => ChatMessage.NotifyClientResearch(false),
            delaySeconds: 8f,
            maxRetries: 3
        );
    }
    public static void ResyncDeferred()
    {
        Plugin.BepinLogger.LogInfo("ResyncDeferred");
        DeferredActionSystem.Schedule(
            action: () => Plugin.APClient.Resync(),
            delaySeconds: 10.0f,
            maxRetries: 3
        );
    }


    public static void LockResearchDeferred(Entity userEntity, PrefabGUID prefabGUID)
    {
        Plugin.BepinLogger.LogInfo("LockTechDeferred");
        DeferredActionSystem.Schedule(
            action: () => ProgressionHandler.LockTechForPlayer(userEntity, prefabGUID),
            delaySeconds: 1.5f,
            maxRetries: 3
        );
    }
    public static void UnlockAchievementDeferred(Entity userEntity, PrefabGUID prefabGUID)
    {
        Plugin.BepinLogger.LogInfo("UnlockAchievementDeferred");
        DeferredActionSystem.Schedule(
            action: () => ProgressionHandler.UnlockAchievementForPlayer(userEntity, prefabGUID),
            delaySeconds: 1.5f,
            maxRetries: 3
        );
        DeferredActionSystem.Schedule(
            action: () => ChatMessage.NotifyClientUnlockAchievement(userEntity, prefabGUID.GuidHash),
            delaySeconds: 1.5f,
            maxRetries: 3
        );
    }

    public static void DisconnectReminderDeferred()
    {
        var fixedString = new FixedString512Bytes("<color=red>If this is not the correct server, please disconnect by opening the chat window and typing .disconnect, then .connect [Playername] [IP:port] [Password] to the correct server</color>");

        DeferredActionSystem.Schedule(
            action: () => ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref fixedString),
            delaySeconds: 5.0f,
            maxRetries: 3
        );

    }
    public static void StillInResearchReminderDeferred()
    {
        DeferredActionSystem.Schedule(
            action: () => StopResearchReminder(),
            delaySeconds: 30.0f,
            maxRetries: 3
        );
    }
    public static void DoneConsumingDeathLinksDefferred()
    {
        DeferredActionSystem.Schedule(
            action: () => DeathEventHandler.DoneConsumingDeathLinks(),
            delaySeconds: 15.0f,
            maxRetries: 3
        );
    }
    public static void StopResearchReminder()
    {
        if (!ProgressionHandler.IsResearching)
        {
            return;
        }
        var fixedString = new FixedString512Bytes("<color=red>You are still in research mode, if you are done researching please type '.stopResearch' into chat</color>");
        ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref fixedString);
    }

    public static void DelayDoneConfiguring(User user)
    {
        DeferredActionSystem.Schedule(
            action: () => ChatMessage.NotifyClientDoneConfiguring(user),
            delaySeconds: 10.0f,
            maxRetries: 3
        );
    }

    public static void DelayDoneReconcile(User user)
    {
        DeferredActionSystem.Schedule(
            action: () => ChatMessage.NotifyClientDoneReconcile(user),
            delaySeconds: 10.0f,
            maxRetries: 3
        );
    }

    public static void ReconcileWithAP(Entity progEntity)
    {
        Plugin.BepinLogger.LogInfo("Reconcile with AP");
        DeferredActionSystem.Schedule(
            action: () => ProgressionSnapshot.ReconcileWithAP(Plugin.EntityManager, progEntity),
            delaySeconds: 5.0f,
            maxRetries: 3
        );
        DeferredActionSystem.Schedule(
            action: () => ChatMessage.NotifyClientReconcile(),
            delaySeconds: 5.0f,
            maxRetries: 3
        );
    }
    public static void RestoreDeferred(EntityManager em, Entity progEntity)
    {
        Plugin.BepinLogger.LogInfo("RestoreDeferred");
        DeferredActionSystem.Schedule(
            action: () => ProgressionSnapshot.Restore(Plugin.EntityManager, progEntity),
            delaySeconds: 3.0f,
            maxRetries: 3
        );
        DeferredActionSystem.Schedule(
            action: () => ChatMessage.NotifyClientRestore(),
            delaySeconds: 3.0f,
            maxRetries: 3
        );
    }
    public static void SlowRestoreDeferred(Entity progEntity)
    {
        Plugin.BepinLogger.LogInfo("RestoreDeferred");
        DeferredActionSystem.Schedule(
            action: () => ProgressionSnapshot.Restore(Plugin.EntityManager, progEntity),
            delaySeconds: 7.0f,
            maxRetries: 3
        );
        DeferredActionSystem.Schedule(
            action: () => ChatMessage.NotifyClientRestore(),
            delaySeconds: 7.0f,
            maxRetries: 3
        );
    }
    public static void WaitForAuthenticationThenDeferred(Action onAuthenticated, Action onTimeout = null)
    {
        Plugin.BepinLogger.LogInfo("WaitForAuthenticationThenDeferred: polling for AP auth");
        DeferredActionSystem.Schedule(
            action: () =>
            {
                if (!ArchipelagoClient.Authenticated)
                {
                    // Throwing triggers the built-in retry/backoff in DeferredActionSystem.
                    throw new InvalidOperationException("AP not yet authenticated, retrying...");
                }
                onAuthenticated();
            },
            delaySeconds: 5f,
            maxRetries: 12 // ~1.5s initial + backoff (1,2,...,12s) ≈ up to ~80s worst case
        );

        if (onTimeout != null)
        {
            DeferredActionSystem.Schedule(
                action: () =>
                {
                    if (!ArchipelagoClient.Authenticated)
                        onTimeout();
                },
                delaySeconds: 85f,
                maxRetries: 0
            );
        }
    }
}