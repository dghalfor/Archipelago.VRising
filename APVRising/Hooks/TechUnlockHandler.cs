using APVRising;
using APVRising.Archipelago;
using APVRising.Utils;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using static ProjectM.ProgressionUtility;
using static VCF.Core.Basics.RoleCommands;

namespace APVRising.Hooks;

[HarmonyPatch]
public static class UnlockResearch
{
    
    [HarmonyPatch(typeof(UnlockResearchSystem), nameof(UnlockResearchSystem.UnlockProgression))]
    [HarmonyPrefix]
    public static bool Prefix(
        EntityManager entityManager,
        UpdateUnlockedJobData progressionJobData,
        PrefabGUID researchGuid,
        Entity user,
        EntityCommandBuffer commandBuffer,
        PrefabLookupMap prefabMapping,
        Entity progressionEntity,
        bool logOnDuplicate = true)
    {
        Plugin.BepinLogger.LogInfo($"[AP] UnlockProgression: {DebugTool.GetPrefabName(researchGuid)}");
        Plugin.APClient.SendLocationCheck(DebugTool.GetPrefabName(researchGuid));
        return true;
    }

    [HarmonyPatch(typeof(UnlockResearchSystem), nameof(UnlockResearchSystem.UnlockProgression))]
    [HarmonyPostfix]
    public static void Postfix(
        EntityManager entityManager,
        UpdateUnlockedJobData progressionJobData,
        PrefabGUID researchGuid,
        Entity user,
        EntityCommandBuffer commandBuffer,
        PrefabLookupMap prefabMapping,
        Entity progressionEntity,
        bool logOnDuplicate = true)
    {
        ProgressionHandler.LockTechForPlayer(user, researchGuid);
        ChatMessage.NotifyClientLock(researchGuid.GuidHash);
    }
}