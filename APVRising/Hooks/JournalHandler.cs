using APVRising;
using APVRising.Utils;
using BepInEx.Logging;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using Stunlock.Core;
using Stunlock.Core.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using VRisingArchipelago;
using static ProjectM.ProgressionUtility;

namespace APVRising.Hooks
{
    [HarmonyPatch]
    public static class JournalHandler {
        [HarmonyPatch(typeof(ClaimAchievementSystem), nameof(ClaimAchievementSystem.CompleteAchievement))]
        [HarmonyPrefix]
        public static bool Prefix(
            ClaimAchievementSystem __instance,
                EntityCommandBuffer commandBuffer,
                PrefabGUID achievementPrefabGuid,
                Entity userEntity,
                Entity characterEntity,
                Entity achievementOwnerEntity,
                bool reApplyMode,
                bool logOnDuplicate)
        {
            ProgressionHandler.IsResearching = true;
            ChatMessage.NotifyClientResearch(true);
            ChatMessage.NotifyClientSnapshot();
            var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            var ProgEntities = progQuery.ToEntityArray(Allocator.Temp);
            foreach (var progEntity in ProgEntities)
            {
                Plugin.BepinLogger.LogInfo("Saving buffer");

                ProgressionSnapshot.Capture(Helper.GetEntityManager(), progEntity);
                ChatMessage.NotifyClientSnapshot();
            }
            Plugin.APClient.SendLocationCheck(DebugTool.GetPrefabName(achievementPrefabGuid));

            return true;
        }
        [HarmonyPatch(typeof(ClaimAchievementSystem), nameof(ClaimAchievementSystem.CompleteAchievement))]
        [HarmonyPostfix]
        public static void Postfix(
                ClaimAchievementSystem __instance,
                EntityCommandBuffer commandBuffer,
                PrefabGUID achievementPrefabGuid,
                Entity userEntity,
                Entity characterEntity,
                Entity achievementOwnerEntity,
                bool reApplyMode,
                bool logOnDuplicate)
            {
            var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            var ProgEntities = progQuery.ToEntityArray(Allocator.Temp);
            foreach (var progEntity in ProgEntities)
            {
                DelaySystem.RestoreDeferred(Helper.GetEntityManager(), progEntity);
            }

            var query = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
            var userEntities = query.ToEntityArray(Allocator.Temp);
            Plugin.BepinLogger.LogInfo($"Unlocking tech for {userEntities.Length} users");
            Plugin.PrefabCollectionSystem._PrefabLookupMap.TryGetValue(achievementPrefabGuid, out Entity achievementEntity);
            var achievementData = Helper.GetEntityManager().GetComponentData<AchievementData>(achievementEntity);

            foreach (var usEntity in userEntities)
            {
                DelaySystem.UnlockAchievementDeferred(usEntity, achievementData.Reward);
            }
            DelaySystem.StopResearchDeferred();

            try
            {
                    Plugin.BepinLogger.LogInfo($"[ClaimAchievementPatch] CompleteAchievement fired | Achievement: {achievementPrefabGuid} | User: {userEntity} | Character: {characterEntity} | ReApply: {reApplyMode}");
                    //DelaySystem.ResyncDeferred();
                }
                catch (Exception ex)
                {
                    Plugin.BepinLogger.LogError($"[ClaimAchievementPatch] Exception in CompleteAchievement postfix: {ex}");
                }
            }  
        }
    }

