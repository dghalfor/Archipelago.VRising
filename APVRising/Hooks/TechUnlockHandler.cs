using APVRising;
using APVRising.Archipelago;
using APVRising.Utils;
using BepInEx.Logging;
using HarmonyLib;
using ProjectM;
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
    // majority of this code adapted from VampireCommandFramework @ VCF.Core/Breadstone/ChatHook.cs
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
        var name = DebugTool.GetPrefabName(researchGuid);
        Plugin.BepinLogger.LogInfo($"[APV] UnlockProgression: {name} ({researchGuid.GuidHash})");
        //DebugTool.LogFullEntityDebugInfo(user);
        // Remove from the learn pool so it can't be rolled again
        //RemoveFromResearchPool(user, researchGuid);
        return true;
    }
    
    [HarmonyPatch(typeof(UnlockResearchSystem), nameof(UnlockResearchSystem.HandleEvent))]
    [HarmonyPostfix]
    public static void Postfix(
   UnlockResearchSystem __instance,
   UnlockResearchEvent unlockResearchEvent,
   FromCharacter fromCharacter,
   ref NetworkIdLookupMap networkIdToEntityMap,
   ref PrefabLookupMap prefabLookupMap,
   ref MapZoneCollection mapZoneCollection,
   EntityCommandBuffer commandBuffer)
    {
        var researchGuid = unlockResearchEvent.ResearchGUID;
        var em = Plugin.EntityManager;

        Plugin.BepinLogger.LogInfo($"[APV] Research intercepted: {DebugTool.GetPrefabName(researchGuid)}");
        //DebugTool.LogFullEntityDebugInfo(fromCharacter.User);

        // 1. Remove from station's ResearchBuffer
        if (networkIdToEntityMap.TryGetValue(unlockResearchEvent.Researchstation, out var stationEntity))
        {
            if (em.HasBuffer<ResearchBuffer>(stationEntity))
            {
                var stationBuffer = em.GetBuffer<ResearchBuffer>(stationEntity);

                for (int i = stationBuffer.Length - 1; i >= 0; i--)
                {
                    if (stationBuffer[i].ResearchGuid == researchGuid)
                    {
                        stationBuffer.RemoveAt(i);
                        Plugin.BepinLogger.LogInfo($"Removed {DebugTool.GetPrefabName(researchGuid)} from research pool");
                        break;
                    }
                }
            }
        }

        // 2. Write to player's UnlockedProgressionElement so they can't roll it again
        //var userEntity = fromCharacter.User;
        //Plugin.BepinLogger.LogInfo($"[debug] attempting to remove from player unlockedProgression");

        //RemoveFromResearchPool(userEntity, researchGuid); // Remove from research pool so it can't be rolled again
        

        // TODO: Send Archipelago location check
        // ArchipelagoClient.SendLocationCheck(researchGuid, fromCharacter.User);

        // Block vanilla — we've handled station memory manually
        return;
    }
    
    public static Entity GetProgressionEntity(Entity userEntity)
    {
        var em = Plugin.EntityManager;

        if (em.TryGetComponentData<ProgressionMapper>(userEntity, out var progressionMapper))
        {
            return progressionMapper.ProgressionEntity._Entity;
        }

        return Entity.Null;
    }

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