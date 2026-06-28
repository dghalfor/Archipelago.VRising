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

namespace APVRising.Utils
{
    public class ResearchDeskHandler
    {
        public static void DisplayUnlockInResearchStation(Entity userEntity, PrefabGUID researchGuid, EntityManager entityManager, bool skipIngredientRemoval = false)
        {
            if (Plugin.IsServer)
            {
                var userData = entityManager.GetComponentData<User>(userEntity);
                Entity characterEntity = userData.LocalCharacter._Entity;
                if (!skipIngredientRemoval)
                {
                    if (Plugin.PrefabCollectionSystem._PrefabLookupMap.TryGetValue(researchGuid, out Entity techEntity))
                    {
                        if (entityManager.HasBuffer<TechItemRequirementBuffer>(techEntity))
                        {
                            var requirements = entityManager.GetBuffer<TechItemRequirementBuffer>(techEntity);
                            for (int i = 0; i < requirements.Length; i++)
                                Plugin.serverGameManager.TryRemoveInventoryItem(characterEntity, requirements[i].Guid, requirements[i].Stacks);
                        }
                    }
                }

                var em = Plugin.EntityManager;
                var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
                var users = userQuery.ToEntityArray(Allocator.Temp);
                foreach (var u in users)
                {
                    var user = em.GetComponentData<User>(u);
                    var message = (FixedString512Bytes)$"##AP_RESEARCH#{researchGuid.GuidHash}##";
                    ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
                }
                users.Dispose();
            }
            

            var progQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            var progEntities = progQuery.ToEntityArray(Allocator.Temp);
            foreach (var entity in progEntities)
            {
                var progressionBuffer = entityManager.GetBuffer<UnlockedProgressionElement>(entity);
                bool alreadyPresent = false;
                for (int i = 0; i < progressionBuffer.Length; i++)
                {
                    if (progressionBuffer[i].UnlockedPrefab == researchGuid)
                    {
                        alreadyPresent = true;
                        break;
                    }
                }
                if (!alreadyPresent)
                    progressionBuffer.Add(new UnlockedProgressionElement { UnlockedPrefab = researchGuid });
            }
            progEntities.Dispose();

            var stationQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<ResearchBuffer>());
            var stations = stationQuery.ToEntityArray(Allocator.Temp);
            foreach (var stationEntity in stations)
            {
                var buffer = entityManager.GetBuffer<ResearchBuffer>(stationEntity);
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i].ResearchGuid == researchGuid)
                    {
                        var entry = buffer[i];
                        entry.IsResearchByStation = true;
                        buffer[i] = entry;
                        break;
                    }
                }
            }
            stations.Dispose();

            var snapshotQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<Snapshot_ResearchBuffer>());
            var snapshotStations = snapshotQuery.ToEntityArray(Allocator.Temp);
            foreach (var stationEntity in snapshotStations)
            {
                var buffer = entityManager.GetBuffer<Snapshot_ResearchBuffer>(stationEntity);
                if (!Snapshot_ResearchBuffer.TryGetSerializedSnapshot(buffer, readOnly: false, out Snapshot_ResearchBuffer.BufferSnapshotPtr snapshotPtr))
                    continue;
                unsafe
                {
                    if (snapshotPtr.Elements == null || snapshotPtr.Length == 0) continue;
                    for (int i = 0; i < snapshotPtr.Length; i++)
                    {
                        ref Snapshot_ResearchBuffer_Data data = ref snapshotPtr.Elements[i];
                        if (data.ResearchGuid == researchGuid)
                        {
                            data.IsResearchByStation = true;
                            break;
                        }
                    }
                }
            }
            snapshotStations.Dispose();

            ProgressionHandler.ForceSnapshotResend();
        }
    }
}

