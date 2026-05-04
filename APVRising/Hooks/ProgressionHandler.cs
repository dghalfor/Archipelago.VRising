using APVRising.Archipelago;
using APVRising.Utils;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using static ProjectM.ProgressionUtility;

namespace APVRising.Hooks;

[HarmonyPatch]
public static class UpdateUnlockedBuffersPatch
{
    [HarmonyPatch(typeof(TriggerPersistenceSaveSystem), nameof(TriggerPersistenceSaveSystem.OnUpdate))]
    [HarmonyPostfix]
    public static void ManageProgressionElements(TriggerPersistenceSaveSystem __instance)
    {
        /*
        var em = Plugin.EntityManager;

        // Query for User entities which have ProgressionMapper
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
        if (userQuery.IsEmpty) return;

        var users = userQuery.ToEntityArray(Allocator.Temp);
        foreach (var userEntity in users)
        {
            var query = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            if (query.IsEmpty) return;

            var entities = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                //UnlockedRecipeElement, UnlockedBlueprintElement, UnlockedVBlood, (maybe) UnlockedSpellBookAbility
                var buffer = em.GetBuffer<UnlockedProgressionElement>(entity);
                var unlockedTechHashes = new List<int>();
                unlockedTechHashes.Add(507915220);
                unlockedTechHashes.Add(-54738837);
                unlockedTechHashes.Add(-2012042353);

                // Sync tech unlocks with recipe unlocks directly on the buffer
                TechToRecipeMapping.SyncUnlockedTechs(buffer, unlockedTechHashes);
                var recipeBuffer = em.GetBuffer<UnlockedRecipeElement>(entity);

                // TODO Read archipelago progression data and sync with game progression
                

                // Sync tech unlocks with recipe unlocks directly on the buffer
                TechToRecipeMapping.SyncTechRecipes(recipeBuffer, unlockedTechHashes);
            }
            entities.Dispose();
        }
        */
    }
}
