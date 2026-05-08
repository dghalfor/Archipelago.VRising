using APVRising.Archipelago;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace APVRising.Systems
{
    public class ArchipelagoItemSystem : SystemBase
    {
        public static ArchipelagoItemSystem Instance { get; set; }

        EntityQuery _userQuery;

        public override void OnCreate()
        {
            Instance = this;

            _userQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                ComponentType.ReadOnly(Il2CppType.Of<User>()),
                ComponentType.ReadOnly(Il2CppType.Of<ProgressionMapper>())
            }
            });

            Enabled = true;
        }

        public override void OnUpdate()
        {
            while (Plugin.APClient != null &&
                   ArchipelagoClient.PendingItems.TryDequeue(out var item))
            {
                Plugin.BepinLogger.LogInfo($"[AP] Processing item: {item.ItemName}");

                var userEntities = _userQuery.ToEntityArray(Allocator.Temp);
                Plugin.BepinLogger.LogInfo($"[AP] Unlocking tech for {userEntities.Length} users");

                // your unlock logic here per user entity

                userEntities.Dispose();
            }
        }
    }
}

