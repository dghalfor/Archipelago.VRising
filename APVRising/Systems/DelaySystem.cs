
using APVRising;
using APVRising.Hooks;
using APVRising.Utils;
using ProjectM;
using Stunlock.Core;
using Stunlock.Core.Animation;
using Unity.Collections;
using Unity.Entities;


namespace VRisingArchipelago;

public static class DelaySystem
{
    public static void StopResearchDeferred()
    {
        Plugin.BepinLogger.LogInfo("StopResearchDeferred");
        DeferredActionSystem.Schedule(
            action: () => ProgressionHandler.setResearch(false),
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
            action: () => ProgressionHandler.setResearch(false),
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
            delaySeconds: 3.0f,
            maxRetries: 3
        );
        DeferredActionSystem.Schedule(
            action: () => ChatMessage.NotifyClientSync(),
            delaySeconds: 3.0f,
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
            action: () => ChatMessage.NotifyClientUnlockAchievement(prefabGUID.GuidHash),
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

    public static void StopResearchReminder()
    {
        if (!ProgressionHandler.IsResearching)
        {
            return;
        }
        var fixedString = new FixedString512Bytes("<color=red>You are still in research mode, if you are done researching please type '.stopResearch' into chat</color>");
        ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref fixedString);
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
}
