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
        if(!ProgressionHandler.IsResearching)
            {
            var message = (FixedString512Bytes)"<color=red>You are not in research mode enter '.startResearch' into chat or else you may waste resources</color>";
            var userentity = entityManager.GetComponentData<ProjectM.Network.User>(user);
            ServerChatUtils.SendSystemMessageToClient(entityManager, userentity, ref message);
        }
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
    }
}