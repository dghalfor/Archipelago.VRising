using APVRising.Archipelago;
using APVRising.Utils;
using HarmonyLib;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using static ProjectM.ProgressionUtility;

namespace APVRising.Hooks;
/*
[HarmonyPatch]public static class UpdateUnlockedBuffersPatchv 
{
    [HarmonyPatch(typeof(ProgressionUtility), nameof(ProgressionUtility.UpdateUnlockedBuffers),
        new Type[]
        {
            typeof(Entity),
            typeof(DynamicBuffer<UnlockedProgressionElement>),
            typeof(UpdateUnlockedJobData)
        },
        new ArgumentType[]
        {
            ArgumentType.Normal,
            ArgumentType.Normal,
            ArgumentType.Ref
        })]
    [HarmonyPrefix]
    public static bool Prefix(
        Entity progressionEntity,
        DynamicBuffer<UnlockedProgressionElement> unlockedProgressionElements,
        ref UpdateUnlockedJobData jobData)
    {
        Plugin.BepinLogger.LogInfo($"[APV] UpdateUnlockedBuffers (3 param): {progressionEntity} Length: {unlockedProgressionElements.Length}");
        foreach (var element in unlockedProgressionElements)
            Plugin.BepinLogger.LogInfo($"[APV]   Unlock: {DebugTool.GetPrefabName(element.UnlockedPrefab)}");
        return false;
    }
}
*/
[HarmonyPatch]
public static class UpdateUnlockedBuffersPatch
{
    [HarmonyPatch(typeof(TriggerPersistenceSaveSystem), nameof(TriggerPersistenceSaveSystem.OnUpdate))]
    [HarmonyPostfix]
    public static void ManageProgressionElements(TriggerPersistenceSaveSystem __instance)
    {
        var em = Plugin.EntityManager;
        var query = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
        if (query.IsEmpty) return;

        var entities = query.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            //UnlockedRecipeElement, UnlockedBlueprintElement, UnlockedVBlood, (maybe) UnlockedSpellBookAbility
            var buffer = em.GetBuffer<UnlockedProgressionElement>(entity);
            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                if (DebugTool.GetPrefabName(buffer[i].UnlockedPrefab).Contains("Armor"))
                {
                    buffer.RemoveAt(i);
                }
            }
            var recipeBuffer = em.GetBuffer<UnlockedRecipeElement>(entity);

            for (int i = recipeBuffer.Length - 1; i >= 0; i--)
            {
                Plugin.BepinLogger.LogInfo($"{DebugTool.GetPrefabName(recipeBuffer[i].UnlockedRecipe)}");
                if (DebugTool.GetPrefabName(recipeBuffer[i].UnlockedRecipe).Contains("Armor"))
                {
                    recipeBuffer.RemoveAt(i);
                }

            }
        }
        entities.Dispose();
    }
}
