
using APVRising;
using APVRising.Hooks;
using APVRising.Utils;
using Stunlock.Core;
using Stunlock.Core.Animation;
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
        DeferredActionSystem.Schedule(
            action: () => ChatMessage.NotifyClientLock(prefabGUID.GuidHash),
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
    public static void RestoreDeferred(EntityManager em, Entity progEntity)
    {
        Plugin.BepinLogger.LogInfo("RestoreDeferred");
        DeferredActionSystem.Schedule(
            action: () => ProgressionSnapshot.Restore(Plugin.EntityManager, progEntity),
            delaySeconds: 1.0f,
            maxRetries: 3
        );
        DeferredActionSystem.Schedule(
            action: () => ChatMessage.NotifyClientRestore(),
            delaySeconds: 1.0f,
            maxRetries: 3
        );
    }
}
