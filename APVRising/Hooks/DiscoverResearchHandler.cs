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
        ResearchBuffer randomResearch, 
        FromCharacter fromCharacter, 
        EntityCommandBuffer commandBuffer, 
        PrefabLookupMap prefabLookupMap, 
        Entity targetResearchStation, 
        Entity progressionEntity)
    {
        var name = DebugTool.GetPrefabName(randomResearch.ResearchGuid);
        Plugin.BepinLogger.LogInfo($"[APV] UnlockProgression: {name} ({randomResearch.ResearchGuid.GuidHash})");

        _lastRolledResearchGuid = randomResearch.ResearchGuid;
        _lastRolledTargetStation = targetResearchStation;
        RemoveFromResearchPool(GetProgressionEntity(fromCharacter.User), randomResearch.ResearchGuid);
        //RemoveFromResearchPool(GetProgressionEntity(fromCharacter.Character), randomResearch.ResearchGuid);
        return true;
    }
    public static PrefabGUID _lastRolledResearchGuid = default;
    public static Entity _lastRolledTargetStation = default;

    [HarmonyPatch(typeof(DiscoverResearchSystem), nameof(DiscoverResearchSystem.HandleEvent))]
    [HarmonyPostfix]
    public static void PostFix(
        DiscoverResearchEventV2 unlockResearchEvent, 
        FromCharacter fromCharacter, 
        NetworkIdLookupMap networkIdToEntityMap, 
        PrefabLookupMap prefabLookupMap, 
        MapZoneCollection mapZoneCollection, 
        EntityCommandBuffer commandBuffer)
    {
        {
            var em = Plugin.EntityManager;

            Plugin.BepinLogger.LogInfo($"[APV] Research intercepted: {DebugTool.GetPrefabName(_lastRolledResearchGuid)}");

            // 1. Remove from station's ResearchBuffer
            if (em.HasBuffer<ResearchBuffer>(_lastRolledTargetStation))
            {
                var stationBuffer = em.GetBuffer<ResearchBuffer>(_lastRolledTargetStation);

                for (int i = stationBuffer.Length - 1; i >= 0; i--)
                {
                    if (stationBuffer[i].ResearchGuid == _lastRolledResearchGuid)
                    {
                        stationBuffer.RemoveAt(i);
                        Plugin.BepinLogger.LogInfo($"Removed {DebugTool.GetPrefabName(_lastRolledResearchGuid)} from research pool");
                    }
                }
            }


            // 2. Write to player's UnlockedProgressionElement so they can't roll it again
            var userEntity = fromCharacter.User;
            Plugin.BepinLogger.LogInfo($"[debug] post attempting to remove from player unlockedProgression");

            RemoveFromResearchPool(GetProgressionEntity(fromCharacter.User), _lastRolledResearchGuid);
           // RemoveFromResearchPool(GetProgressionEntity(fromCharacter.Character), _lastRolledResearchGuid);

            // TODO: Send Archipelago location check
            // ArchipelagoClient.SendLocationCheck(researchGuid, fromCharacter.User);

            // Block vanilla — we've handled station memory manually
            return;
        }
    }

    public static Entity GetProgressionEntity(Entity userEntity)
    {
        var em = Plugin.EntityManager;

        if (!em.HasBuffer<AttachedBuffer>(userEntity)) return Entity.Null;

        var attached = em.GetBuffer<AttachedBuffer>(userEntity);
        foreach (var attachment in attached)
        {
            if (em.HasBuffer<UnlockedProgressionElement>(attachment.Entity))
                return attachment.Entity;
        }

        return Entity.Null;
    }
    /*
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

            Plugin.BepinLogger.LogInfo($"[APV] Research intercepted: {DebugTool.GetPrefabName(randomResearch.ResearchGuid)}");

            // 1. Remove from station's ResearchBuffer
            if (em.HasBuffer<ResearchBuffer>(targetResearchStation))
            {
                var stationBuffer = em.GetBuffer<ResearchBuffer>(targetResearchStation);

                for (int i = stationBuffer.Length - 1; i >= 0; i--)
                {
                    if (stationBuffer[i].ResearchGuid == randomResearch.ResearchGuid)
                    {
                        stationBuffer.RemoveAt(i);
                        Plugin.BepinLogger.LogInfo($"Removed {DebugTool.GetPrefabName(randomResearch.ResearchGuid)} from research pool");
                        return;
                    }
                }
            }


            // 2. Write to player's UnlockedProgressionElement so they can't roll it again
            var userEntity = fromCharacter.User;
            Plugin.BepinLogger.LogInfo($"[debug] post attempting to remove from player unlockedProgression");

            RemoveFromResearchPool(fromCharacter.Character, randomResearch.ResearchGuid);
            RemoveFromResearchPool(fromCharacter.User, randomResearch.ResearchGuid);
            RemoveFromResearchPool(progressionEntity, randomResearch.ResearchGuid);

            // TODO: Send Archipelago location check
            // ArchipelagoClient.SendLocationCheck(researchGuid, fromCharacter.User);

            // Block vanilla — we've handled station memory manually
            return;
        } 
    }*/
    public static void RemoveFromResearchPool(Entity progressionEntity, PrefabGUID techGuid)
    {
        var em = Plugin.Server.EntityManager;

        if (!em.HasBuffer<UnlockedProgressionElement>(progressionEntity))
        {
            Plugin.BepinLogger.LogInfo("No UnlockedProgressionElement buffer found");
            return;
        }

        var buffer = em.GetBuffer<UnlockedProgressionElement>(progressionEntity);

        for (int i = buffer.Length - 1; i >= 0; i--)
        {
            if (buffer[i].UnlockedPrefab._Value == techGuid._Value)
            {
                buffer.RemoveAt(i);
                Plugin.BepinLogger.LogInfo($"Removed {DebugTool.GetPrefabName(techGuid)} from research pool");
                return;
            }
        }

        Plugin.BepinLogger.LogInfo($"{DebugTool.GetPrefabName(techGuid)} not found in UnlockedProgressionElement");
    } 
}

