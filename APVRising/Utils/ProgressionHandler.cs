using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.TextCore.Text;

namespace APVRising.Utils
{
    internal class ProgressionHandler
    {
        public static List<int> APProgression = new List<int> { 507915220, -54738837 };
        public static List<int> ResearchedProgression = new List<int> { -632708133, -997169234, -212104516 };

        public static bool IsResearching;
        public static void SwitchProgression(DynamicBuffer<UnlockedProgressionElement> progressionBuffer)
        {
            if (IsResearching)
            {
                TechToRecipeMapping.SyncUnlockedTechs(progressionBuffer, ResearchedProgression);
                //ClearUnlockBuffers();
                CheckResearchStations(ResearchedProgression);
                //CheckClientResearchStations(unlockedTechHashes);
                //ClearSnapshots(unlockedTechHashes);
                //update progressionElement to previouslyResearched elements
            }
            else
            {
                TechToRecipeMapping.SyncUnlockedTechs(progressionBuffer, APProgression);
                CheckResearchStations(APProgression);

                //CheckClientResearchStations(unlockedTechHashes);
                //ClearUnlockBuffers();
                // ClearSnapshots(unlockedTechHashes);

                //update progressionElement to Archipelago unlocked elements
            }
        }

        public static void UpdateProgression()
        {
            var em = Plugin.EntityManager;
            // Query for User entities which have ProgressionMapper
            //var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
            var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<ProgressionMapper>());
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

                    // Sync tech unlocks with recipe unlocks directly on the buffer
                    SwitchProgression(buffer);
                    var recipeBuffer = em.GetBuffer<UnlockedRecipeElement>(entity);
                    //TechToRecipeMapping.SyncTechRecipes(recipeBuffer, unlockedTechHashes);

                }
                entities.Dispose();
            }
        }
        
        public static void ClearUnlockBuffers()
        {
            var query = Plugin.ClientEntityManager.CreateEntityQuery(ComponentType.ReadWrite<HaveUnlocksInStation>());
            if (query.IsEmpty) {
                Plugin.BepinLogger.LogInfo($"No entities with HaveUnlocksInStation found, skipping ClearUnlockBuffers");
                return;
            }
            // Get and update the handle
            Plugin.BepinLogger.LogInfo($"Clearing HaveUnlocksInStation buffers");
            var testUnlocks = Plugin.ClientEntityManager.GetComponentTypeHandle<HaveUnlocksInStation>(false);

            // Iterate chunks and zero out
            var chunks = query.ToArchetypeChunkArray(Allocator.Temp);
            foreach (var chunk in chunks)
            {
                Plugin.BepinLogger.LogInfo($"Clearing");

                var components = chunk.GetNativeArray(ref testUnlocks);
                for (int i = 0; i < components.Length; i++)
                {
                    var comp = components[i];
                    comp.CanUnlock = false;
                    components[i] = comp;
                }
            }
            chunks.Dispose();
        }
        
        public static void CheckResearchStations(List<int> unlockedTechHashes)
        {
            var query = Plugin.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<ResearchBuffer>());
            Plugin.BepinLogger.LogInfo($"CheckResearchStations");

            // Iterate chunks and zero out
            var stations = query.ToEntityArray(Allocator.Temp);
            foreach (var stationEntity in stations)
            {
                var buffer = Plugin.EntityManager.GetBuffer<ResearchBuffer>(stationEntity);
                for (var i = 0; i < buffer.Length; i++) 
                    {
                    var research = buffer[i];
                    Plugin.BepinLogger.LogInfo($"Station has research: {research.ResearchGuid}{research.IsResearchByStation}");
                }
                TechToRecipeMapping.SyncResearchStation(buffer, unlockedTechHashes);
                Plugin.BepinLogger.LogInfo($"Post-sync");

                for (var i = 0; i < buffer.Length; i++)
                {
                    var research = buffer[i];
                    Plugin.BepinLogger.LogInfo($"Station has research: {research.ResearchGuid}{research.IsResearchByStation}");
                }
            }
            stations.Dispose();
        }
        public static void CheckClientResearchStations(List<int> unlockedTechHashes)
        {
            var query = Plugin.ClientEntityManager.CreateEntityQuery(ComponentType.ReadOnly<ResearchBuffer>());
            Plugin.BepinLogger.LogInfo($"CheckClientResearchStations");

            // Iterate chunks and zero out
            var stations = query.ToEntityArray(Allocator.Temp);
            foreach (var stationEntity in stations)
            {
                var buffer = Plugin.ClientEntityManager.GetBuffer<ResearchBuffer>(stationEntity);
                for (var i = 0; i < buffer.Length; i++)
                {
                    var research = buffer[i];
                    Plugin.BepinLogger.LogInfo($"Client Station has research: {research.ResearchGuid}{research.IsResearchByStation}");
                }
                TechToRecipeMapping.SyncResearchStation(buffer, unlockedTechHashes);
                Plugin.BepinLogger.LogInfo($"Client Post-sync");

                for (var i = 0; i < buffer.Length; i++)
                {
                    var research = buffer[i];
                    Plugin.BepinLogger.LogInfo($"Client Station has research: {research.ResearchGuid}{research.IsResearchByStation}");
                }
            }
            stations.Dispose();
        }
        public static void ClearSnapshots(List<int> unlockedTechHashes)
        {
            var query = Plugin.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Snapshot_ResearchBuffer_Data>());
            Plugin.BepinLogger.LogInfo($"ClearSnapshots");

            // Iterate chunks and zero out
            var stations = query.ToEntityArray(Allocator.Temp);
            foreach (var stationEntity in stations)
            {
                var buffer = Plugin.EntityManager.GetBuffer<Snapshot_ResearchBuffer_Data>(stationEntity);
                for (var i = 0; i < buffer.Length; i++)
                {
                    var research = buffer[i];
                    Plugin.BepinLogger.LogInfo($"Station has research: {research.ResearchGuid}{research.IsResearchByStation}");
                }
                TechToRecipeMapping.SyncResearchSnapshot(buffer, unlockedTechHashes);
                Plugin.BepinLogger.LogInfo($"Post-sync");

                for (var i = 0; i < buffer.Length; i++)
                {
                    var research = buffer[i];
                    Plugin.BepinLogger.LogInfo($"Station has research: {research.ResearchGuid}{research.IsResearchByStation}");
                }
            }
            stations.Dispose();
        }
    }
}
