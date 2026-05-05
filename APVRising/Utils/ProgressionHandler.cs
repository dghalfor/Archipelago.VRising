using APVRising;
using APVRising.Archipelago;
using APVRising.Hooks;
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
        /*
        public static void UpdateProgression()
        {
            // Always sync server
            var serverEm = Plugin.EntityManager;
            var serverQuery = serverEm.CreateEntityQuery(
                ComponentType.ReadOnly<User>(),
                ComponentType.ReadOnly<ProgressionMapper>()
            );

            if (!serverQuery.IsEmpty)
            {
                var users = serverQuery.ToEntityArray(Allocator.Temp);
                foreach (var userEntity in users)
                {
                    var progQuery = serverEm.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
                    if (!progQuery.IsEmpty)
                    {
                        var entities = progQuery.ToEntityArray(Allocator.Temp);
                        foreach (var entity in entities)
                        {
                            SwitchProgression(serverEm.GetBuffer<UnlockedProgressionElement>(entity));
                        }
                        entities.Dispose();
                    }
                }
                users.Dispose();
            }
            /*
            // Always also sync client
            var clientEm = Plugin.ClientEntityManager;
            var clientProgQuery = clientEm.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            if (!clientProgQuery.IsEmpty)
            {
                var entities = clientProgQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    SwitchProgression(clientEm.GetBuffer<UnlockedProgressionElement>(entity));
                }
                entities.Dispose();
            }
           
        } */

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
            Plugin.BepinLogger.LogInfo($"Found {stations.Length} stations with Snapshot_ResearchBuffer to clear");
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
    }
}