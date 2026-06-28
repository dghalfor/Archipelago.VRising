using APVRising.Utils;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using static ProjectM.ProgressionUtility;
using static VCF.Core.Basics.RoleCommands;

namespace APVRising.Hooks;

[HarmonyPatch]
internal class DiscoverResearchHandler
{
    [HarmonyPatch(typeof(DiscoverResearchSystem), nameof(DiscoverResearchSystem.UnlockProgression))]
    [HarmonyPrefix]
    public static bool Prefix(
        DiscoverResearchSystem __instance,
        ResearchBuffer randomResearch,
        FromCharacter fromCharacter,
        EntityCommandBuffer commandBuffer,
        ref PrefabLookupMap prefabLookupMap,
        Entity targetResearchStation,
        Entity progressionEntity)
    {
        if (!ProgressionHandler.IsResearching)
        {
            var message = (FixedString512Bytes)"<color=red>You are not in research mode enter '.startResearch' into chat or else you may waste resources</color>";
            var userEntity = Helper.GetEntityManager().GetComponentData<ProjectM.Network.User>(fromCharacter.User);
            ServerChatUtils.SendSystemMessageToClient(Helper.GetEntityManager(), userEntity, ref message);
        }

        Plugin.BepinLogger.LogInfo($"DiscoverResearch: {DebugTool.GetPrefabName(randomResearch.ResearchGuid)}");
        Plugin.APClient.SendLocationCheck(DebugTool.GetPrefabName(randomResearch.ResearchGuid));
        ResearchDeskHandler.DisplayUnlockInResearchStation(fromCharacter.User, randomResearch.ResearchGuid, Helper.GetEntityManager(), skipIngredientRemoval: true);
        return false;
    }
}