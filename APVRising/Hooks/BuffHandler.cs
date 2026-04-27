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
using Unity.Collections;
using Unity.Entities;
using UnityEngine.EventSystems;

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

            var prefabGuid = DebugTool.GetAndLogPrefabGuid(entity, "BuffSystem_Spawn_Server:", true);

            switch (prefabGuid.GuidHash)
            {
                case (int)Effects.AB_FeedBoss_03_Complete_Trigger:
                case (int)Effects.AB_FeedBoss_04_Complete_AreaTriggerBuff:
                    var em = __instance.EntityManager;
                    if (em.HasBuffer<CreateGameplayEventsOnSpawn>(entity))
                    {
                        Plugin.BepinLogger.LogInfo($"BuffSystem_Spawn_Server: Vblood kill triggered a spawn with the same buff, likely the blood pool. Entity: {entity}");
                        em.GetBuffer<CreateGameplayEventsOnSpawn>(entity).Clear();
                        _bossEntitiesToDestroy = entity;

                    }
                    SendPlayerUpdate(__instance.EntityManager, entity, true);
                    DestroyUtility.Destroy(__instance.EntityManager, entity);
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
    [HarmonyPostfix]
    public static void Postfix(BuffSystem_Spawn_Server __instance)
    {
        var em = __instance.EntityManager;
        // Get the boss entity from the buff before destroying

        if (em.TryGetComponentData<SpellTarget>(_bossEntitiesToDestroy, out var spellTarget))
        {
            var bossEntity = spellTarget.Target._Entity;

            if (em.TryGetComponentData<VBloodConsumeSource>(bossEntity, out var consumeSource))
            {
                Plugin.BepinLogger.LogInfo($"[APV] VBloodConsumeSource: School {consumeSource.SpellSchool}, Tier {consumeSource.Tier}, SchoolPoints {consumeSource.SpellSchoolPoints}, passivePoints {consumeSource.PassivePoints}");
            }

            // Log VBloodUnlockTechBuffer buffer contents
            if (em.HasBuffer<VBloodUnlockTechBuffer>(bossEntity))
            {
                var techBuffer = em.GetBuffer<VBloodUnlockTechBuffer>(bossEntity);
                Plugin.BepinLogger.LogInfo($"[APV] VBloodUnlockTechBuffer length: {techBuffer.Length}");
                for (int i = 0; i < techBuffer.Length; i++)
                {
                    var tech = techBuffer[i];
                    Plugin.BepinLogger.LogInfo($"[APV] Tech[{i}]: {tech.Guid}");
                    // If tech has a named field, try:
                    // Plugin.BepinLogger.LogInfo($"[APV] Tech[{i}]: {DebugTool.GetPrefabName(tech.TechPrefabGuid)}");
                }
            }
            Plugin.BepinLogger.LogInfo($"BuffSystem_Spawn_Server Postfix: Attempting to destroy boss entity {bossEntity} associated with buff entity {_bossEntitiesToDestroy}");
            DestroyUtility.Destroy(__instance.EntityManager, bossEntity);
        }

        _bossEntitiesToDestroy = Entity.Null; // Clear the boss entity to destroy after handling
    }

    private static void SendPlayerUpdate(EntityManager em, Entity entity, bool killOnly)
    {
        if (em.TryGetComponentData<SpellTarget>(entity, out var target))
        {
            // If the owner is not a player character, ignore this entity
            if (!em.TryGetComponentData<EntityOwner>(entity, out var entityOwner)) return;
            if (!em.TryGetComponentData<PlayerCharacter>(entityOwner.Owner, out var playerCharacter)) return;

            PlayerCache.FindPlayer(playerCharacter.Name.ToString(), true, out _, out var userEntity);
            // target.BloodConsumeSource can buff/debuff the blood quality
            Plugin.BepinLogger.LogInfo($"user {userEntity.ToString}, killed {DebugTool.GetPrefabName(target.Target._Entity)}");
            //Plugin.BepinLogger.LogInfo(userEntity, $"{(killOnly ? "Killed" : "Consumed")}: {DebugTool.GetPrefabName(target.Target._Entity)}");
        }
    }
}
