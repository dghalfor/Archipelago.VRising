using APVRising;
using APVRising.Archipelago;
using APVRising.Hooks;
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
using UnityEngine.TextCore.Text;
using static ProjectM.ProgressionUtility;

namespace APVRising.Utils
{
    internal class ProgressionHandler
    {

        public static bool IsResearching = false;
        public static bool isStale = false;

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
                    ForceSnapshotResend();
                }
                else
                {
                    CheckClientResearchStations(ArchipelagoData.GetResearchProgression());
                    ClearClientSnapshots(ArchipelagoData.GetResearchProgression());
                    ForceClientSnapshotResend();
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
                    ForceSnapshotResend();
                }
                else
                {
                    CheckClientResearchStations(ArchipelagoData.GetAPProgression());
                    ClearClientSnapshots(ArchipelagoData.GetAPProgression());
                    ForceClientSnapshotResend();
                }
                //CheckClientResearchStations(APProgression);
                //ClearUnlockBuffers();
                // ClearSnapshots(unlockedTechHashes);

                //update progressionElement to Archipelago unlocked elements
            }
        }
        public static void SwitchRecipe(DynamicBuffer<UnlockedRecipeElement> recipeBuffer)
        {
            Plugin.BepinLogger.LogInfo($"Switching progression. IsResearching: {IsResearching}");
            
        }

        public static void UpdateProgression()
        {
            EntityManager em;
            if (Plugin.IsServer) {
                em = Plugin.EntityManager;
            }
            else
            {
                em = Plugin.ClientEntityManager;
            }
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
            Plugin.BepinLogger.LogInfo($"Found {stations.Length} stations with Snapshot_ResearchBuffer to clear");
            foreach (var stationEntity in stations)
            {
                var buffer = Plugin.ClientEntityManager.GetBuffer<ResearchBuffer>(stationEntity);
                TechToRecipeMapping.SyncResearchStation(buffer, unlockedTechHashes);
            }
            stations.Dispose();
        }

        public static void CheckWorkstations(List<int> unlockedTechHashes)
        {
            var query = Plugin.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<WorkstationRecipesBuffer>());
            Plugin.BepinLogger.LogInfo($"CheckWorkstations");

            // Iterate chunks and zero out
            var stations = query.ToEntityArray(Allocator.Temp);
            Plugin.BepinLogger.LogInfo($"Found {stations.Length} workstations to check");

            foreach (var stationEntity in stations)
            {
                var buffer = Plugin.EntityManager.GetBuffer<WorkstationRecipesBuffer>(stationEntity);
                TechToRecipeMapping.SyncWorkstation(buffer, unlockedTechHashes);
            }
            stations.Dispose();
        }
        public static void CheckClientWorkstations(List<int> unlockedTechHashes)
        {
            var query = Plugin.ClientEntityManager.CreateEntityQuery(ComponentType.ReadOnly<WorkstationRecipesBuffer>());
            Plugin.BepinLogger.LogInfo($"CheckClientWorkstations");

            // Iterate chunks and zero out
            var stations = query.ToEntityArray(Allocator.Temp);
            Plugin.BepinLogger.LogInfo($"Found {stations.Length} stations with WorkstationRecipesBuffer to clear");
            foreach (var stationEntity in stations)
            {
                var buffer = Plugin.ClientEntityManager.GetBuffer<WorkstationRecipesBuffer>(stationEntity);
                TechToRecipeMapping.SyncWorkstation(buffer, unlockedTechHashes);
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
            Plugin.BepinLogger.LogInfo($"Found {stations.Length} stations with Snapshot_ResearchBuffer to clear");
            foreach (var stationEntity in stations)
            {
                var buffer = Plugin.ClientEntityManager.GetBuffer<Snapshot_ResearchBuffer>(stationEntity);
                TechToRecipeMapping.SyncResearchSnapshot(buffer, unlockedTechHashes);
            }
        }
        public static void ForceClientSnapshotResend()
        {
            var em = Plugin.ClientEntityManager;
            var query = em.CreateEntityQuery(ComponentType.ReadOnly<ClientNetworkSnapshotState>());
            var clients = query.ToEntityArray(Allocator.Temp);

            Plugin.BepinLogger.LogInfo($"Found {clients.Length} clients to force resend");

            foreach (var clientEntity in clients)
            {
                var state = em.GetComponentData<ClientNetworkSnapshotState>(clientEntity);
                //state.LastFrameReceived = 0; // force full resend

                em.SetComponentData(clientEntity, state);
            }

            clients.Dispose();
        }
        public static void ForceSnapshotResend()
        {
            var em = Plugin.EntityManager;
            var query = em.CreateEntityQuery(ComponentType.ReadOnly<ResearchBuffer>());
            var stations = query.ToEntityArray(Allocator.Temp);

            foreach (var station in stations)
            {
                // Force FrameChanged to current frame to mark entity as dirty
                if (em.HasComponent<FrameChanged>(station))
                {
                    var frameChanged = em.GetComponentData<FrameChanged>(station);
                    Plugin.BepinLogger.LogInfo($"FrameChanged before: {frameChanged.Value}");
                    frameChanged.Value = int.MaxValue;
                    em.SetComponentData(station, frameChanged);
                    Plugin.BepinLogger.LogInfo($"FrameChanged after: {frameChanged.Value}");
                }

                // Also update NetworkSnapshot if it has a version/frame field
                // Clear UpToDateUserBitMask to force resend to all clients
                if (em.HasComponent<UpToDateUserBitMask>(station))
                {
                    var bitMask = em.GetComponentData<UpToDateUserBitMask>(station);

                    // Clear bits for all connected users
                    var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
                    var users = userQuery.ToEntityArray(Allocator.Temp);

                    for (int i = 0; i < users.Length; i++)
                    {
                        bitMask.Value.RemoveUserBit(i);
                    }

                    users.Dispose();
                    em.SetComponentData(station, bitMask);
                    Plugin.BepinLogger.LogInfo($"Cleared UpToDateUserBitMask for station");
                }
            }

            stations.Dispose();
        }
    

    public static void UnlockResearchForPlayer(Entity userEntity, PrefabGUID techPrefab)
        {
            EntityManager em;
            PrefabCollectionSystem prefabCollectionSystem;
            if (Plugin.IsServer)
            {
                em = Plugin.EntityManager;
                prefabCollectionSystem = Plugin.PrefabCollectionSystem;
            }
            else
            {
                em = Plugin.ClientEntityManager;
                prefabCollectionSystem = Plugin.ClientCollectionSystem;
            }
            Plugin.BepinLogger.LogInfo($"Unlocking research for player {userEntity.Index} and tech {techPrefab._Value}");
            var query = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            if (query.IsEmpty) return;

            var entities = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                //UnlockedRecipeElement, UnlockedBlueprintElement, UnlockedVBlood, (maybe) UnlockedSpellBookAbility
                var buffer = em.GetBuffer<UnlockedProgressionElement>(entity);
                // Sync tech unlocks with recipe unlocks directly on the buffer
                buffer.Add(new UnlockedProgressionElement { UnlockedPrefab = techPrefab });

                var recipeBuffer = em.GetBuffer<UnlockedRecipeElement>(entity);
                Plugin.BepinLogger.LogInfo($"Tech {techPrefab._Value} unlock added to UnlockedProgressionElement buffer. Syncing recipes...");

                if (!prefabCollectionSystem._PrefabLookupMap.TryGetValue(techPrefab, out Entity researchEntity))
                {
                    Plugin.BepinLogger.LogWarning($"[AP] Could not find entity for PrefabGUID {techPrefab._Value}");
                    return;
                }

                if (!em.HasBuffer<TechUnlockRecipeBuffer>(researchEntity))
                    return;

                var techBuffer = em.GetBuffer<TechUnlockRecipeBuffer>(researchEntity);
                var unlockedBuffer = em.GetBuffer<UnlockedRecipeElement>(entity);
                Plugin.BepinLogger.LogInfo($"Found {techBuffer.Length} recipes to unlock for tech {techPrefab._Value}");
                for (int i = 0; i < techBuffer.Length; i++)
                {
                    var element = techBuffer[i];

                    bool alreadyUnlocked = false;
                    for (int j = 0; j < unlockedBuffer.Length; j++)
                    {
                        if (unlockedBuffer[j].UnlockedRecipe == element.Guid)
                        {
                            Plugin.BepinLogger.LogInfo($"Recipe {element.Guid} already unlocked for player {userEntity.Index}, skipping");
                            alreadyUnlocked = true;
                            break;
                        }
                    }

                    if (alreadyUnlocked)
                        continue;
                    Plugin.BepinLogger.LogInfo($"Adding element to unlocked buffer: {element.Guid}");
                    unlockedBuffer.Add(new UnlockedRecipeElement { UnlockedRecipe = element.Guid, UserHasRequiredContentFlags = true });
                }
            }
            entities.Dispose();
        }
    }
}