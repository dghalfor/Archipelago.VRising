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
        if (!ProgressionHandler.IsResearching)
        {
            var message = (FixedString512Bytes)"<color=red>You are not in research mode, enter '.startResearch' into chat</color>";
            var userData = entityManager.GetComponentData<ProjectM.Network.User>(user);
            ServerChatUtils.SendSystemMessageToClient(entityManager, userData, ref message);
        }

        Plugin.BepinLogger.LogInfo($"Research UnlockProgression: {DebugTool.GetPrefabName(researchGuid)}");
        Plugin.APClient.SendLocationCheck(DebugTool.GetPrefabName(researchGuid));
        ResearchDeskHandler.DisplayUnlockInResearchStation(user, researchGuid, entityManager);
        return false;
    }
}