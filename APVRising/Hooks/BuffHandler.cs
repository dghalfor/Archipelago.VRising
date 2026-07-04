using APVRising;
using APVRising.Archipelago;
using APVRising.Utils;
using APVRising.Utils.Prefabs;
using BepInEx.Logging;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.EventSystems;
using VRisingArchipelago;

namespace APVRising.Hooks;

//Hooking into the buff system when a V Blood is killed
[HarmonyPatch]
public class BuffSystemSpawnServerPatch
{
    private static Entity _bossEntitiesToDestroy = Entity.Null;

    [HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
    [HarmonyPrefix]
    public static void Prefix(BuffSystem_Spawn_Server __instance)
    {
        _bossEntitiesToDestroy = Entity.Null; // Reset the boss entity to destroy at the start of each update

        var entities = __instance.__query_401358634_0.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            var prefabGuid = Helper.GetPrefabGUID(entity);

            switch (prefabGuid.GuidHash)
            {
                case (int)Effects.AB_FeedBoss_03_Complete_Trigger:
                case (int)Effects.AB_FeedBoss_04_Complete_AreaTriggerBuff:
                case (int)Effects.AB_FeedBoss_FeedOnDracula_03_Complete_Trigger:
                case (int)Effects.AB_FeedBoss_FeedOnDracula_04_Complete_AreaTriggerBuff:
                    var em = __instance.EntityManager;
                    Plugin.BepinLogger.LogInfo($"[AP] Boss Buff Detected: {DebugTool.GetPrefabName(prefabGuid)}");
                    if (em.HasBuffer<CreateGameplayEventsOnSpawn>(entity))
                    {
                        ProgressionHandler.IsResearching = true;
                        ChatMessage.NotifyClientResearch(true);
                        ChatMessage.NotifyClientSnapshot();
                        var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
                        var ProgEntities = progQuery.ToEntityArray(Allocator.Temp);
                        foreach (var progEntity in ProgEntities)
                        {
                            Plugin.BepinLogger.LogInfo("Saving buffer");

                            ProgressionSnapshot.Capture(Plugin.EntityManager, progEntity);
                        }

                        _bossEntitiesToDestroy = entity;
                    }
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
    [HarmonyPostfix]
    public static void Postfix(BuffSystem_Spawn_Server __instance)
    {
        if (_bossEntitiesToDestroy == Entity.Null) return; // If no boss entity was marked for destruction, skip the rest of the logic

        var em = __instance.EntityManager;
        // Get the boss entity from the buff

        if (em.TryGetComponentData<SpellTarget>(_bossEntitiesToDestroy, out var spellTarget))
        {
            var bossEntity = spellTarget.Target._Entity;

            // Handle VBloodUnlockTechBuffer buffer contents
            if (em.HasBuffer<VBloodUnlockTechBuffer>(bossEntity))
            {
                var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
                var ProgEntities = progQuery.ToEntityArray(Allocator.Temp);
                foreach (var progEntity in ProgEntities)
                {
                    DelaySystem.RestoreDeferred(Plugin.EntityManager, progEntity);
                }

                var techBuffer = em.GetBuffer<VBloodUnlockTechBuffer>(bossEntity);
                var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
                var userEntities = userQuery.ToEntityArray(Allocator.Temp);

                for (int i = 0; i < techBuffer.Length; i++)
                {
                    var tech = techBuffer[i];

                    Plugin.BepinLogger.LogInfo($"[AP] Lock Progression: {DebugTool.GetPrefabName(tech.Guid)}");
                    Plugin.APClient.SendLocationCheck(DebugTool.GetPrefabName(tech.Guid));

                    foreach (var userEntity in userEntities)
                    {
                        DelaySystem.LockResearchDeferred(userEntity, new Stunlock.Core.PrefabGUID(tech.Guid.GuidHash));
                        // force complete the journal achievement
                        var user = Plugin.Server.EntityManager.GetComponentData<ProjectM.Network.User>(userEntity);
                        var claimSystem = Plugin.Server.GetExistingSystemManaged<ClaimAchievementSystem>();

                        var achievementOwnerQuery = Plugin.Server.EntityManager.CreateEntityQuery(
                            ComponentType.ReadOnly<AchievementClaimedElement>()
                        );

                        Plugin.BepinLogger.LogInfo($"[AP] Achievement owner entities: {achievementOwnerQuery.CalculateEntityCount()}");

                        if (!achievementOwnerQuery.IsEmpty)
                        {
                            var achievementOwners = achievementOwnerQuery.ToEntityArray(Allocator.Temp);
                            foreach (var owner in achievementOwners)
                            {
                                Plugin.BepinLogger.LogInfo($"[AP] Trying achievement owner: {owner}");
                                var ecb = new EntityCommandBuffer(Allocator.Temp);
                                claimSystem.CompleteAchievement(ecb, new PrefabGUID(-302458684), userEntity, user.LocalCharacter._Entity, owner);
                                ecb.Playback(Plugin.Server.EntityManager);
                                ecb.Dispose();
                            }
                            achievementOwners.Dispose();
                        }

                        achievementOwnerQuery.Dispose();
                    }
                }
                DelaySystem.StopResearchDeferred();
                //DelaySystem.ResyncDeferred();
            }
        }
    }
}
