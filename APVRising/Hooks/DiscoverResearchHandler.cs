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
using Unity.Entities;
using static ProjectM.ProgressionUtility;

namespace APVRising.Hooks;

[HarmonyPatch]
internal class DiscoverResearchHandler
{

    // Discover fix
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
        Plugin.BepinLogger.LogInfo($"[AP] UnlockProgression: {DebugTool.GetPrefabName(randomResearch.ResearchGuid)}");

        _lastRolledResearchGuid = randomResearch.ResearchGuid;
        _lastRolledTargetStation = targetResearchStation;
        return true;
    }
    public static PrefabGUID _lastRolledResearchGuid = default;
    public static Entity _lastRolledTargetStation = default;

    // TODO This may not be necessary now since we're not messing with the progression entity
    [HarmonyPatch(typeof(DiscoverResearchSystem), nameof(DiscoverResearchSystem.UnlockProgression))]
    [HarmonyPostfix]
    public static void PostFix(
        ResearchBuffer randomResearch,
        FromCharacter fromCharacter,
        EntityCommandBuffer commandBuffer,
        PrefabLookupMap prefabLookupMap,
        Entity targetResearchStation,
        Entity progressionEntity)
    {
        {
            var em = Plugin.EntityManager;

            Plugin.BepinLogger.LogInfo($"Research intercepted: {DebugTool.GetPrefabName(randomResearch.ResearchGuid)}");

            // 1. Remove from station's ResearchBuffer
            if (em.HasBuffer<ResearchBuffer>(targetResearchStation))
            {
                var stationBuffer = em.GetBuffer<ResearchBuffer>(targetResearchStation);

                for (int i = stationBuffer.Length - 1; i >= 0; i--)
                {
                    if (stationBuffer[i].ResearchGuid == randomResearch.ResearchGuid)
                    {
                        //stationBuffer.RemoveAt(i);
                        Plugin.BepinLogger.LogInfo($"Removed {DebugTool.GetPrefabName(randomResearch.ResearchGuid)} from research pool");
                        break;
                    }
                }
            }
            return;
        }
    }
}