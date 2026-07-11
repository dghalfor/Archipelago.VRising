using APVRising;
using APVRising.Archipelago;
using APVRising.Data;
using APVRising.Hooks;
using Il2CppSystem.Runtime.Remoting;
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
using static APVRising.Services.DataService;
using static ProjectM.ProgressionUtility;
using static VCF.Core.Basics.RoleCommands;

namespace APVRising.Utils
{
    internal class ProgressionHandler
    {

        public static bool IsResearching = false;
        public static bool isStale = false;

        public static void setResearch(bool research)
        {
            IsResearching = research;
        }

        public static void SwitchProgression(DynamicBuffer<UnlockedProgressionElement> progressionBuffer)
        {
            Plugin.BepinLogger.LogInfo($"Switching progression. IsResearching: {IsResearching}");
            if (IsResearching)
            {
                Plugin.BepinLogger.LogInfo($"Switching to researched progression");
                TechToRecipeMapping.SyncUnlockedTechs(progressionBuffer, ArchipelagoData.GetCheckedLocations());
                Plugin.BepinLogger.LogInfo(Plugin.IsServer.ToString());

                if (Plugin.IsServer)
                {
                    CheckResearchStations(ArchipelagoData.GetCheckedLocations());
                    ClearSnapshots(ArchipelagoData.GetCheckedLocations());
                    ForceSnapshotResend();
                }
                else
                {
                    CheckClientResearchStations(ArchipelagoData.GetCheckedLocations());
                    ClearClientSnapshots(ArchipelagoData.GetCheckedLocations());
                    ForceClientSnapshotResend();
                }
            }
            else
            {
                Plugin.BepinLogger.LogInfo($"Switching to AP progression");
                TechToRecipeMapping.SyncUnlockedTechs(progressionBuffer, ArchipelagoData.GetReceivedChecks());

                Plugin.BepinLogger.LogInfo(Plugin.IsServer.ToString());
                if (Plugin.IsServer)
                {
                    CheckResearchStations(ArchipelagoData.GetReceivedChecks());
                    ClearSnapshots(ArchipelagoData.GetReceivedChecks());
                    ForceSnapshotResend();
                }
                else
                {
                    CheckClientResearchStations(ArchipelagoData.GetReceivedChecks());
                    ClearClientSnapshots(ArchipelagoData.GetReceivedChecks());
                    ForceClientSnapshotResend();
                }
            }
        }
        public static void SwitchRecipe(DynamicBuffer<UnlockedRecipeElement> recipeBuffer)
        {
            Plugin.BepinLogger.LogInfo($"Switching progression. IsResearching: {IsResearching}");

        }

        public static void UpdateProgression()
        {
            EntityManager em;
            if (Plugin.IsServer)
            {
                em = Plugin.EntityManager;
            }
            else
            {
                em = Plugin.ClientEntityManager;
            }
            // Query for User entities which have ProgressionMapper
            var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<ProjectM.Network.User>(), ComponentType.ReadOnly<ProgressionMapper>());
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
                    var spellBuffer = em.GetBuffer<UnlockedSpellBookAbility>(entity);
                    // Sync tech unlocks with recipe unlocks directly on the buffer
                    SwitchProgression(buffer);
                }
                entities.Dispose();
            }
        }

        public void ClearUnlockBuffers()
        {
            var query = Plugin.ClientEntityManager.CreateEntityQuery(ComponentType.ReadWrite<HaveUnlocksInStation>());
            if (query.IsEmpty)
            {
                //Plugin.BepinLogger.LogInfo($"No entities with HaveUnlocksInStation found, skipping ClearUnlockBuffers");
                return;
            }
            // Get and update the handle
            //Plugin.BepinLogger.LogInfo($"Clearing HaveUnlocksInStation buffers");
            var testUnlocks = Plugin.ClientEntityManager.GetComponentTypeHandle<HaveUnlocksInStation>(false);

            // Iterate chunks and zero out
            var chunks = query.ToArchetypeChunkArray(Allocator.Temp);
            foreach (var chunk in chunks)
            {
                //Plugin.BepinLogger.LogInfo($"Clearing");

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
            //Plugin.BepinLogger.LogInfo($"CheckResearchStations");

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
            //Plugin.BepinLogger.LogInfo($"CheckClientResearchStations");

            // Iterate chunks and zero out
            var stations = query.ToEntityArray(Allocator.Temp);
            //Plugin.BepinLogger.LogInfo($"Found {stations.Length} stations with Snapshot_ResearchBuffer to clear");
            foreach (var stationEntity in stations)
            {
                var buffer = Plugin.ClientEntityManager.GetBuffer<ResearchBuffer>(stationEntity);
                TechToRecipeMapping.SyncResearchStation(buffer, unlockedTechHashes);
            }
            stations.Dispose();
        }
        public static void CheckRefinementStations(PrefabGUID unlockedRecipe)
        {
            EntityManager em;
            if (Plugin.IsServer)
            {
                em = Plugin.EntityManager;
            }
            else
            {
                em = Plugin.ClientEntityManager;
            }
            var query = em.CreateEntityQuery(ComponentType.ReadOnly<RefinementstationRecipesBuffer>());
            //  Plugin.BepinLogger.LogInfo($"CheckRefinementStations");

            // Iterate chunks and zero out
            var stations = query.ToEntityArray(Allocator.Temp);
            foreach (var stationEntity in stations)
            {
                var buffer = em.GetBuffer<RefinementstationRecipesBuffer>(stationEntity);
                TechToRecipeMapping.LockRefinementStationRecipe(buffer, unlockedRecipe);
            }
            stations.Dispose();
        }

        public static void CheckWorkstations(List<int> unlockedTechHashes)
        {
            var query = Plugin.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<WorkstationRecipesBuffer>());
            //Plugin.BepinLogger.LogInfo($"CheckWorkstations");

            // Iterate chunks and zero out
            var stations = query.ToEntityArray(Allocator.Temp);
            // Plugin.BepinLogger.LogInfo($"Found {stations.Length} workstations to check");

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
            // Plugin.BepinLogger.LogInfo($"CheckClientWorkstations");

            // Iterate chunks and zero out
            var stations = query.ToEntityArray(Allocator.Temp);
            // Plugin.BepinLogger.LogInfo($"Found {stations.Length} stations with WorkstationRecipesBuffer to clear");
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
            // Plugin.BepinLogger.LogInfo($"ClearSnapshots");

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
            // Plugin.BepinLogger.LogInfo($"ClearClientSnapshots");

            var stations = query.ToEntityArray(Allocator.Temp);
            //  Plugin.BepinLogger.LogInfo($"Found {stations.Length} stations with Snapshot_ResearchBuffer to clear");
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

            // Plugin.BepinLogger.LogInfo($"Found {clients.Length} clients to force resend");

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
                    var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<ProjectM.Network.User>());
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


        public static void UnlockTechForPlayer(Entity userEntity, PrefabGUID techPrefab)
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
            ArchipelagoData.AddReceivedCheck(techPrefab._Value);

            // Plugin.BepinLogger.LogInfo($"Unlocking research for player {userEntity.Index} and tech {techPrefab._Value}");
            var query = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            if (query.IsEmpty) return;

            var entities = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                //UnlockedRecipeElement, UnlockedBlueprintElement, UnlockedVBlood, (maybe) UnlockedSpellBookAbility
                // Sync tech unlocks with recipe unlocks directly on the buffer
                var buffer = em.GetBuffer<UnlockedProgressionElement>(entity);

                bool alreadyInProgression = false;
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i].UnlockedPrefab == techPrefab)
                    {
                        alreadyInProgression = true;
                        break;
                    }
                }

                if (!alreadyInProgression)
                {
                    buffer.Add(new UnlockedProgressionElement { UnlockedPrefab = techPrefab });
                }

                var recipeBuffer = em.GetBuffer<UnlockedRecipeElement>(entity);
                // Plugin.BepinLogger.LogInfo($"Tech {techPrefab._Value} unlock added to UnlockedProgressionElement buffer. Syncing recipes...");

                if (!prefabCollectionSystem._PrefabLookupMap.TryGetValue(techPrefab, out Entity researchEntity))
                {
                    //  Plugin.BepinLogger.LogWarning($"[AP] Could not find entity for PrefabGUID {techPrefab._Value}");
                    return;
                }

                //Unlock recipes
                if (em.HasBuffer<TechUnlockRecipeBuffer>(researchEntity))
                {
                    var techBuffer = em.GetBuffer<TechUnlockRecipeBuffer>(researchEntity);
                    var unlockedBuffer = em.GetBuffer<UnlockedRecipeElement>(entity);
                    // Plugin.BepinLogger.LogInfo($"Found {techBuffer.Length} recipes to unlock for tech {techPrefab._Value}");
                    for (int i = 0; i < techBuffer.Length; i++)
                    {
                        var element = techBuffer[i];

                        bool alreadyUnlocked = false;
                        for (int j = 0; j < unlockedBuffer.Length; j++)
                        {
                            if (unlockedBuffer[j].UnlockedRecipe == element.Guid)
                            {
                                //    Plugin.BepinLogger.LogInfo($"Recipe {element.Guid} already unlocked for player {userEntity.Index}, skipping");
                                alreadyUnlocked = true;
                                break;
                            }
                        }

                        if (alreadyUnlocked)
                            continue;
                        // Plugin.BepinLogger.LogInfo($"Adding element to unlocked buffer: {element.Guid}");
                        unlockedBuffer.Add(new UnlockedRecipeElement { UnlockedRecipe = element.Guid, UserHasRequiredContentFlags = true });
                    }
                }

                //Unlock blueprints
                if (em.HasBuffer<TechUnlockBlueprintBuffer>(researchEntity))
                {
                    Plugin.BepinLogger.LogInfo($"Found TechUnlockBlueprintBuffer for tech {techPrefab._Value}");
                    var blueprintBuffer = em.GetBuffer<TechUnlockBlueprintBuffer>(researchEntity);
                    var unlockedBPBuffer = em.GetBuffer<UnlockedBlueprintElement>(entity);
                    // Plugin.BepinLogger.LogInfo($"Found {blueprintBuffer.Length} blueprints to unlock for tech {techPrefab._Value}");
                    for (int i = 0; i < blueprintBuffer.Length; i++)
                    {
                        var element = blueprintBuffer[i];

                        bool alreadyUnlocked = false;
                        for (int j = 0; j < unlockedBPBuffer.Length; j++)
                        {
                            if (unlockedBPBuffer[j].UnlockedBlueprint == element.Guid)
                            {
                                //  Plugin.BepinLogger.LogInfo($"Blueprint {element.Guid} already unlocked for player {userEntity.Index}, skipping");
                                alreadyUnlocked = true;
                                break;
                            }
                        }

                        if (alreadyUnlocked)
                            continue;
                        Plugin.BepinLogger.LogInfo($"Adding element to unlocked buffer: {element.Guid}");
                        unlockedBPBuffer.Add(new UnlockedBlueprintElement { UnlockedBlueprint = element.Guid, UserHasRequiredContentFlags = true });
                    }
                }
                if (em.HasBuffer<ProgressionBookShapeshiftElement>(researchEntity))
                {
                    var progressionShapeshiftBuffer = em.GetBuffer<ProgressionBookShapeshiftElement>(researchEntity);
                    var unlockedShapeshiftBuffer = em.GetBuffer<UnlockedShapeshiftElement>(entity);
                    var user = em.GetComponentData<ProjectM.Network.User>(userEntity);

                    string playerKey = Plugin.ServerSaveName + "-" + user.CharacterName;
                    string techKey = techPrefab._Value.ToString();

                    // check if we have saved state to restore
                    bool hasSavedState = false;
                    List<ShapeshiftEntryData> savedEntries = null;

                    if (PlayerDictionaries._PlayerShapeshifts.TryGetValue(playerKey, out var shapeshiftData) &&
                        shapeshiftData.shapeshiftGuidOwned.TryGetValue(techKey, out savedEntries))
                    {
                        hasSavedState = true;
                    }
                    if (hasSavedState)
                    {
                        // restore exactly what was saved, preserving UserHasRequiredContentFlags
                        foreach (var entry in savedEntries)
                        {
                            var guid = new PrefabGUID(entry.Guid);
                            bool alreadyUnlocked = false;
                            for (int j = 0; j < unlockedShapeshiftBuffer.Length; j++)
                            {
                                if (unlockedShapeshiftBuffer[j].UnlockedShapeshift == guid)
                                {
                                    alreadyUnlocked = true;
                                    break;
                                }
                            }
                            if (!alreadyUnlocked)
                            {
                                Plugin.BepinLogger.LogInfo($"Restoring shapeshift {guid} with HasRequiredContent={entry.UserHasRequiredContentFlags}");
                                unlockedShapeshiftBuffer.Add(new UnlockedShapeshiftElement
                                {
                                    UnlockedShapeshift = guid,
                                    UserHasRequiredContentFlags = entry.UserHasRequiredContentFlags
                                });
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < progressionShapeshiftBuffer.Length; i++)
                        {
                            var element = progressionShapeshiftBuffer[i];

                            // add the base group as true
                            bool alreadyUnlocked = false;
                            for (int j = 0; j < unlockedShapeshiftBuffer.Length; j++)
                            {
                                if (unlockedShapeshiftBuffer[j].UnlockedShapeshift == element.Shapeshift)
                                {
                                    alreadyUnlocked = true;
                                    break;
                                }
                            }
                            if (!alreadyUnlocked)
                            {
                                Plugin.BepinLogger.LogInfo($"[Shapeshift] First unlock: adding base group {element.Shapeshift} as true");
                                unlockedShapeshiftBuffer.Add(new UnlockedShapeshiftElement
                                {
                                    UnlockedShapeshift = element.Shapeshift,
                                    UserHasRequiredContentFlags = true
                                });
                            }

                            // add skin variants as false
                            if (prefabCollectionSystem._PrefabLookupMap.TryGetValue(element.Shapeshift, out Entity groupEntity) &&
                                em.HasBuffer<ProgressionDependencyElement>(groupEntity))
                            {
                                var dependencyBuffer = em.GetBuffer<ProgressionDependencyElement>(groupEntity);
                                for (int d = 0; d < dependencyBuffer.Length; d++)
                                {
                                    var skinGuid = dependencyBuffer[d].PrefabGuid;
                                    bool skinAlreadyUnlocked = false;
                                    for (int j = 0; j < unlockedShapeshiftBuffer.Length; j++)
                                    {
                                        if (unlockedShapeshiftBuffer[j].UnlockedShapeshift == skinGuid)
                                        {
                                            skinAlreadyUnlocked = true;
                                            break;
                                        }
                                    }
                                    if (!skinAlreadyUnlocked)
                                    {
                                        Plugin.BepinLogger.LogInfo($"[Shapeshift] First unlock: adding skin {skinGuid} as false");
                                        unlockedShapeshiftBuffer.Add(new UnlockedShapeshiftElement
                                        {
                                            UnlockedShapeshift = skinGuid,
                                            UserHasRequiredContentFlags = false
                                        });
                                    }
                                }
                            }
                        }
                    }
                
                }

            }
            entities.Dispose();
        }

        public static void UnlockAchievementForPlayer(Entity userEntity, PrefabGUID techPrefab)
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
            ArchipelagoData.AddReceivedCheck(techPrefab._Value);

            // Plugin.BepinLogger.LogInfo($"Unlocking research for player {userEntity.Index} and tech {techPrefab._Value}");
            var query = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            if (query.IsEmpty) return;

            var entities = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                //UnlockedRecipeElement, UnlockedBlueprintElement, UnlockedVBlood, (maybe) UnlockedSpellBookAbility
                // Sync tech unlocks with recipe unlocks directly on the buffer
                var buffer = em.GetBuffer<UnlockedProgressionElement>(entity);

                bool alreadyInProgression = false;
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i].UnlockedPrefab == techPrefab)
                    {
                        alreadyInProgression = true;
                        break;
                    }
                }

                if (!alreadyInProgression)
                {
                    buffer.Add(new UnlockedProgressionElement { UnlockedPrefab = techPrefab });
                }

                var recipeBuffer = em.GetBuffer<UnlockedRecipeElement>(entity);
                // Plugin.BepinLogger.LogInfo($"Tech {techPrefab._Value} unlock added to UnlockedProgressionElement buffer. Syncing recipes...");

                if (!prefabCollectionSystem._PrefabLookupMap.TryGetValue(techPrefab, out Entity researchEntity))
                {
                    //  Plugin.BepinLogger.LogWarning($"[AP] Could not find entity for PrefabGUID {techPrefab._Value}");
                    return;
                }

                //Unlock recipes
                if (em.HasBuffer<ProgressionBookRecipeElement>(researchEntity))
                {
                    var techBuffer = em.GetBuffer<ProgressionBookRecipeElement>(researchEntity);
                    var unlockedBuffer = em.GetBuffer<UnlockedRecipeElement>(entity);
                    // Plugin.BepinLogger.LogInfo($"Found {techBuffer.Length} recipes to unlock for tech {techPrefab._Value}");
                    for (int i = 0; i < techBuffer.Length; i++)
                    {
                        var element = techBuffer[i];

                        bool alreadyUnlocked = false;
                        for (int j = 0; j < unlockedBuffer.Length; j++)
                        {
                            if (unlockedBuffer[j].UnlockedRecipe == element.Recipe)
                            {
                                //    Plugin.BepinLogger.LogInfo($"Recipe {element.Guid} already unlocked for player {userEntity.Index}, skipping");
                                alreadyUnlocked = true;
                                break;
                            }
                        }

                        if (alreadyUnlocked)
                            continue;
                        // Plugin.BepinLogger.LogInfo($"Adding element to unlocked buffer: {element.Guid}");
                        unlockedBuffer.Add(new UnlockedRecipeElement { UnlockedRecipe = element.Recipe, UserHasRequiredContentFlags = true });
                    }
                }

                if (em.HasBuffer<ProgressionBookBlueprintElement>(researchEntity))
                {
                    var blueprintBuffer = em.GetBuffer<ProgressionBookBlueprintElement>(researchEntity);
                    var unlockedBPBuffer = em.GetBuffer<UnlockedBlueprintElement>(entity);
                    // Plugin.BepinLogger.LogInfo($"Found {blueprintBuffer.Length} blueprints to unlock for tech {techPrefab._Value}");
                    for (int i = 0; i < blueprintBuffer.Length; i++)
                    {
                        var element = blueprintBuffer[i];

                        bool alreadyUnlocked = false;
                        for (int j = 0; j < unlockedBPBuffer.Length; j++)
                        {
                            if (unlockedBPBuffer[j].UnlockedBlueprint == element.Blueprint)
                            {
                                //  Plugin.BepinLogger.LogInfo($"Blueprint {element.Guid} already unlocked for player {userEntity.Index}, skipping");
                                alreadyUnlocked = true;
                                break;
                            }
                        }

                        if (alreadyUnlocked)
                            continue;
                        // Plugin.BepinLogger.LogInfo($"Adding element to unlocked buffer: {element.Guid}");
                        unlockedBPBuffer.Add(new UnlockedBlueprintElement { UnlockedBlueprint = element.Blueprint, UserHasRequiredContentFlags = true });
                    }
                }
                if (em.HasBuffer<ProgressionBookTechElement>(researchEntity))
                {
                    var unlockedTechElementBuffer = em.GetBuffer<ProgressionBookTechElement>(researchEntity);
                    for (int i = 0; i < unlockedTechElementBuffer.Length; i++)
                    {
                        UnlockTechForPlayer(userEntity, unlockedTechElementBuffer[i].Tech);
                    }
                }
                entities.Dispose();
            }
        }
        public static void LockTechForPlayer(Entity userEntity, PrefabGUID techPrefab)
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
            ArchipelagoData.AddLocationCheck(techPrefab._Value);

            if (ArchipelagoData.ReceivedChecks.Contains(techPrefab._Value))
            {
                Plugin.BepinLogger.LogInfo($"Player already has {techPrefab._Value}");
                ChatMessage.NotifyClientLocation(techPrefab._Value);
                return;
            }

            if (Plugin.IsServer && DataDicts.EntityNameToAPLocation.TryGetValue(DebugTool.GetPrefabName(techPrefab), out var locationName))
            {
                if (!Plugin.APClient.IsConfiguredLocation(locationName))
                {
                    Plugin.BepinLogger.LogInfo($"Player does not have {techPrefab._Value} but it is not a configured location, skipping lock");
                    ArchipelagoData.AddReceivedCheck(techPrefab._Value);
                    ArchipelagoData.AddLocationCheck(techPrefab._Value);
                    if (Plugin.IsServer)
                    {
                        ChatMessage.NotifyClientCheck(techPrefab._Value);
                        ChatMessage.NotifyClientLocation(techPrefab._Value);
                    }
                    return;
                }
            }  
            if (Plugin.IsServer)
            {
                ChatMessage.NotifyClientLock(techPrefab.GuidHash);
            }

            Plugin.BepinLogger.LogInfo($"Lock research for player {userEntity.Index} and tech {techPrefab._Value}");
            var query = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            if (query.IsEmpty) return;
            Plugin.BepinLogger.LogInfo($"Locking tech {techPrefab._Value} for player {userEntity.Index}");
            var entities = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                //UnlockedRecipeElement, UnlockedBlueprintElement, UnlockedVBlood, (maybe) UnlockedSpellBookAbility
                Plugin.BepinLogger.LogInfo($"Tech {techPrefab._Value} should be locked but is in buffer, removing it");

                if (!prefabCollectionSystem._PrefabLookupMap.TryGetValue(techPrefab, out Entity researchEntity))
                {
                    Plugin.BepinLogger.LogWarning($"[AP] Could not find entity for PrefabGUID {techPrefab._Value}");
                    return;
                }

                if (em.HasBuffer<TechUnlockRecipeBuffer>(researchEntity))
                {
                    var techBuffer = em.GetBuffer<TechUnlockRecipeBuffer>(researchEntity);
                    var unlockedBuffer = em.GetBuffer<UnlockedRecipeElement>(entity);
                    //Plugin.BepinLogger.LogInfo($"Found {unlockedBuffer.Length} unlocked recipes for player {entity.Index}");
                    //Plugin.BepinLogger.LogInfo($"Found {techBuffer.Length} recipes to lock for tech {techPrefab._Value}");
                    for (int j = 0; j < techBuffer.Length; j++)
                    {
                        var element = techBuffer[j];
                        for (int i = unlockedBuffer.Length - 1; i >= 0; i--)
                        {
                            if (unlockedBuffer[i].UnlockedRecipe == element.Guid)
                            {
                                //   Plugin.BepinLogger.LogInfo($"Tech {techPrefab} should be locked but is in buffer, removing it");
                                unlockedBuffer.RemoveAt(i);
                                CheckRefinementStations(element.Guid);
                            }
                        }
                    }
                }
                if (em.HasBuffer<TechUnlockBlueprintBuffer>(researchEntity))
                {
                    var blueprintBuffer = em.GetBuffer<TechUnlockBlueprintBuffer>(researchEntity);
                    var unlockedBPBuffer = em.GetBuffer<UnlockedBlueprintElement>(entity);
                    Plugin.BepinLogger.LogInfo($"Found {blueprintBuffer.Length} blueprints to lock for tech {techPrefab._Value}");
                    for (int i = 0; i < blueprintBuffer.Length; i++)
                    {
                        var element = blueprintBuffer[i];

                        for (int j = unlockedBPBuffer.Length - 1; j >= 0; j--)
                        {
                            if (unlockedBPBuffer[j].UnlockedBlueprint == element.Guid)
                            {
                                Plugin.BepinLogger.LogInfo($"Blueprint {element.Guid} should be locked but is in buffer, for player {userEntity.Index}");
                                unlockedBPBuffer.RemoveAt(j);
                            }
                        }
                    }
                }
                if (em.HasBuffer<ProgressionBookShapeshiftElement>(researchEntity))
                {
                    var progressionUnlockBuffer = em.GetBuffer<ProgressionBookShapeshiftElement>(researchEntity);
                    var unlockedShapeshiftBuffer = em.GetBuffer<UnlockedShapeshiftElement>(entity);
                    var entriesToSave = new List<ShapeshiftEntryData>();

                    Plugin.BepinLogger.LogInfo($"[Shapeshift] Locking tech {techPrefab._Value}, progressionUnlockBuffer.Length={progressionUnlockBuffer.Length}, unlockedShapeshiftBuffer.Length={unlockedShapeshiftBuffer.Length}");

                    for (int i = 0; i < progressionUnlockBuffer.Length; i++)
                    {
                        var groupGuid = progressionUnlockBuffer[i].Shapeshift;
                        Plugin.BepinLogger.LogInfo($"[Shapeshift] Processing group {groupGuid._Value}");

                        for (int j = unlockedShapeshiftBuffer.Length - 1; j >= 0; j--)
                        {
                            if (unlockedShapeshiftBuffer[j].UnlockedShapeshift == groupGuid)
                            {
                                Plugin.BepinLogger.LogInfo($"[Shapeshift] Found group {groupGuid._Value} in buffer, HasRequiredContent={unlockedShapeshiftBuffer[j].UserHasRequiredContentFlags}");
                                entriesToSave.Add(new ShapeshiftEntryData(groupGuid._Value, unlockedShapeshiftBuffer[j].UserHasRequiredContentFlags));
                                unlockedShapeshiftBuffer.RemoveAt(j);
                            }
                        }

                        if (prefabCollectionSystem._PrefabLookupMap.TryGetValue(groupGuid, out Entity groupEntity) &&
                            em.HasBuffer<ProgressionDependencyElement>(groupEntity))
                        {
                            var dependencyBuffer = em.GetBuffer<ProgressionDependencyElement>(groupEntity);
                            Plugin.BepinLogger.LogInfo($"[Shapeshift] Found {dependencyBuffer.Length} skin variants for group {groupGuid._Value}");

                            for (int d = 0; d < dependencyBuffer.Length; d++)
                            {
                                var skinGuid = dependencyBuffer[d].PrefabGuid;
                                Plugin.BepinLogger.LogInfo($"[Shapeshift] Processing skin {skinGuid._Value}");

                                for (int j = unlockedShapeshiftBuffer.Length - 1; j >= 0; j--)
                                {
                                    if (unlockedShapeshiftBuffer[j].UnlockedShapeshift == skinGuid)
                                    {
                                        Plugin.BepinLogger.LogInfo($"[Shapeshift] Found skin {skinGuid._Value} in buffer, HasRequiredContent={unlockedShapeshiftBuffer[j].UserHasRequiredContentFlags}");
                                        entriesToSave.Add(new ShapeshiftEntryData(skinGuid._Value, unlockedShapeshiftBuffer[j].UserHasRequiredContentFlags));
                                        unlockedShapeshiftBuffer.RemoveAt(j);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Plugin.BepinLogger.LogWarning($"[Shapeshift] Could not find group entity or ProgressionDependencyElement buffer for {groupGuid._Value}");
                        }
                    }

                    Plugin.BepinLogger.LogInfo($"[Shapeshift] Saving {entriesToSave.Count} entries for tech {techPrefab._Value}");

                    var user = em.GetComponentData<ProjectM.Network.User>(userEntity);
                    string playerKey = Plugin.ServerSaveName + "-" + user.CharacterName;
                    string techKey = techPrefab._Value.ToString();

                    PlayerDictionaries._PlayerShapeshifts.AddOrUpdate(
                        playerKey,
                        _ => new PlayerShapeshiftData(new Dictionary<string, List<ShapeshiftEntryData>> { [techKey] = entriesToSave }),
                        (_, existing) => { existing.shapeshiftGuidOwned[techKey] = entriesToSave; return existing; }
                    );
                    PlayerPersistence.SavePlayerShapeshiftData();
                }
            }

            
            entities.Dispose();
        }

        public static void LockSpellAbilityForPlayer(Entity userEntity, PrefabGUID spellPrefab)
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
            if (!ArchipelagoData.CheckedLocations.Contains(spellPrefab._Value))
            {
                ArchipelagoData.CheckedLocations.Add(spellPrefab._Value);
            }
            if (ArchipelagoData.ReceivedChecks.Contains(spellPrefab._Value))
            {
                return;
            }
            var query = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            if (query.IsEmpty) return;
            Plugin.BepinLogger.LogInfo($"lockable spell {spellPrefab.GuidHash}");
            var entities = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                var unlockedSpellBuffer = em.GetBuffer<UnlockedSpellBookAbility>(entity);
                for (int j = unlockedSpellBuffer.Length - 1; j >= 0; j--)
                {
                    if (unlockedSpellBuffer[j].Ability == spellPrefab)
                    {
                        unlockedSpellBuffer.RemoveAt(j);
                        Plugin.BepinLogger.LogInfo($"Removing element from unlocked buffer: {spellPrefab.GuidHash}");
                        break;
                    }
                }
            }
        }

        public static void UnlockSpellAbilityForPlayer(Entity userEntity, PrefabGUID spellPrefab)
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
            ArchipelagoData.AddReceivedCheck(spellPrefab._Value);

            if (!prefabCollectionSystem._PrefabLookupMap.TryGetValue(spellPrefab, out Entity spellEntity))
            {
                // Plugin.BepinLogger.LogWarning($"[AP] Could not find entity for PrefabGUID {techPrefab._Value}");
                return;
            }

            var query = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            if (query.IsEmpty) return;

            var entities = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                var unlockedSpellBuffer = em.GetBuffer<UnlockedSpellBookAbility>(entity);

                for (int i = 0; i < unlockedSpellBuffer.Length; i++)
                {
                    var element = unlockedSpellBuffer[i];

                    bool alreadyUnlocked = false;
                    for (int j = 0; j < unlockedSpellBuffer.Length; j++)
                    {
                        if (unlockedSpellBuffer[j].Ability == spellPrefab)
                        {
                            alreadyUnlocked = true;
                            break;
                        }
                    }

                    if (alreadyUnlocked)
                        continue;
                    Plugin.BepinLogger.LogInfo($"Adding element to unlocked buffer: {spellPrefab.GuidHash}");
                    var abilityComp = em.GetComponentData<AbilitySpellSchool>(spellEntity);
                    unlockedSpellBuffer.Add(new UnlockedSpellBookAbility { Ability = spellPrefab, Tier = abilityComp.Tier });
                }
            }
        }
        public static void LockProg(EntityManager em, PrefabGUID prefab)
        {
            Plugin.BepinLogger.LogInfo($"Locking progression for prefab {prefab.GuidHash}");
            var revokeProgQuery = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            var revokeProgEntities = revokeProgQuery.ToEntityArray(Allocator.Temp);
            foreach (var progEntity in revokeProgEntities)
            {
                var progBuffer = em.GetBuffer<UnlockedProgressionElement>(progEntity);
                for (int i = progBuffer.Length - 1; i >= 0; i--)
                {
                    if (progBuffer[i].UnlockedPrefab == prefab)
                    {
                        Plugin.BepinLogger.LogInfo($"Removing progression element {prefab.GuidHash} from buffer");
                        progBuffer.RemoveAt(i);
                        break;
                    }
                }
            }
            revokeProgEntities.Dispose();
        }
        public static void DeduplicateBuffer<T>(DynamicBuffer<T> buffer, Func<T, PrefabGUID> getGuid)
    where T : struct
        {
            var seen = new HashSet<int>();
            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                int guid = getGuid(buffer[i])._Value;
                if (!seen.Add(guid))
                    buffer.RemoveAt(i);
            }
        }
    }
}