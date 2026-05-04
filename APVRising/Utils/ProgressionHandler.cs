using APVRising.Archipelago;
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

        public static bool IsResearching = false;
        public static void SwitchProgression(DynamicBuffer<UnlockedProgressionElement> progressionBuffer)
        {
            Plugin.BepinLogger.LogInfo($"Switching progression. IsResearching: {IsResearching}");
            if (IsResearching)
            {
                Plugin.BepinLogger.LogInfo($"Switching to researched progression");
                TechToRecipeMapping.SyncUnlockedTechs(progressionBuffer, ArchipelagoData.GetResearchProgression());
                //ClearUnlockBuffers();
                Plugin.BepinLogger.LogInfo(Plugin.IsServer.ToString());

                if (Plugin.IsServer)
                {
                    CheckResearchStations(ArchipelagoData.GetResearchProgression());
                    ClearSnapshots(ArchipelagoData.GetResearchProgression());

                }
                else
                {
                    CheckClientResearchStations(ArchipelagoData.GetResearchProgression());
                    ClearClientSnapshots(ArchipelagoData.GetResearchProgression());
                }
                //CheckClientResearchStations(ResearchedProgression);
                //update progressionElement to previouslyResearched elements
            }
            else
            {
                Plugin.BepinLogger.LogInfo($"Switching to AP progression");
                TechToRecipeMapping.SyncUnlockedTechs(progressionBuffer, ArchipelagoData.GetAPProgression());
                Plugin.BepinLogger.LogInfo(Plugin.IsServer.ToString());
                if (Plugin.IsServer)
                {
                    CheckResearchStations(ArchipelagoData.GetAPProgression());
                    ClearSnapshots(ArchipelagoData.GetAPProgression());

                }
                else
                {
                    CheckClientResearchStations(ArchipelagoData.GetAPProgression());
                    ClearClientSnapshots(ArchipelagoData.GetAPProgression());
                }
                //CheckClientResearchStations(APProgression);
                //ClearUnlockBuffers();
                // ClearSnapshots(unlockedTechHashes);

                //update progressionElement to Archipelago unlocked elements
            }
        }

        public static void UpdateProgression()
        {
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
                    // Sync tech unlocks with recipe unlocks directly on the buffer
                    SwitchProgression(buffer);
                    var recipeBuffer = em.GetBuffer<UnlockedRecipeElement>(entity);
                    //TechToRecipeMapping.SyncTechRecipes(recipeBuffer, unlockedTechHashes);

                }
                entities.Dispose();
            }
        }
        
        public void ClearUnlockBuffers()
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
                TechToRecipeMapping.SyncResearchStation(buffer, unlockedTechHashes);
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
                TechToRecipeMapping.SyncResearchStation(buffer, unlockedTechHashes);
            }
            stations.Dispose();
        }
        public static void ClearSnapshots(List<int> unlockedTechHashes)
        {
            var query = Plugin.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Snapshot_ResearchBuffer>());
            Plugin.BepinLogger.LogInfo($"ClearSnapshots");

            var stations = query.ToEntityArray(Allocator.Temp);
            foreach (var stationEntity in stations)
            {
                var buffer = Plugin.EntityManager.GetBuffer<Snapshot_ResearchBuffer>(stationEntity);
                TechToRecipeMapping.SyncResearchSnapshot(buffer, unlockedTechHashes);
            }
        }

        public static void ClearClientSnapshots(List<int> unlockedTechHashes)
        {
            var query = Plugin.ClientEntityManager.CreateEntityQuery(ComponentType.ReadOnly<Snapshot_ResearchBuffer>());
            Plugin.BepinLogger.LogInfo($"ClearClientSnapshots");

            var stations = query.ToEntityArray(Allocator.Temp);
            foreach (var stationEntity in stations)
            {
                var buffer = Plugin.EntityManager.GetBuffer<Snapshot_ResearchBuffer>(stationEntity);
                TechToRecipeMapping.SyncResearchSnapshot(buffer, unlockedTechHashes);
            }
        }
    }
}
