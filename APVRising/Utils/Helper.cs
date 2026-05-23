using APVRising;
using BepInEx.Logging;
using Epic.OnlineServices;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using ProjectM.Scripting;
using Stunlock.Core;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;

//Shamelessly stolen from XPRising, big shout outs
namespace APVRising.Utils
{
    public static class Helper
    {
        private static Entity empty_entity = new Entity();
        private static System.Random rand = new System.Random();

        public static ServerScriptMapper ServerScriptMapper { get; internal set; }
        public static ServerGameManager ServerGameManager { get; internal set; }
        public static EntityCommandBufferSystem EntityCommandBufferSystem { get; internal set; }
        public static ClaimAchievementSystem ClaimAchievementSystem { get; internal set; }
        public static EndSimulationEntityCommandBufferSystem EndSimECBSystem { get; internal set; }

        public static Regex rxName = new Regex(@"(?<=\])[^\[].*");

        public static void Initialise()
        {
            ServerScriptMapper = Plugin.Server.GetExistingSystemManaged<ServerScriptMapper>();
            ServerGameManager = ServerScriptMapper._ServerGameManager;
            ClaimAchievementSystem = Plugin.Server.GetExistingSystemManaged<ClaimAchievementSystem>();
            EntityCommandBufferSystem = Plugin.Server.GetExistingSystemManaged<EntityCommandBufferSystem>();
            EndSimECBSystem = Plugin.Server.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        public static FixedString64Bytes GetTrueName(string name)
        {
            MatchCollection match = Helper.rxName.Matches(name);
            if (match.Count > 0)
            {
                name = match[^1].ToString();
            }
            return name;
        }

        // This refers to the localisation string for "{value} Experience"
        static readonly AssetGuid XPAssetGuid = AssetGuid.FromString("4210316d-23d4-4274-96f5-d6f0944bd0bb");
        static readonly float3 XPTextColour = new float3(1, 1, 0);
        public static void CreateXpText(float3 location, float value, Entity character, Entity userEntity)
        {
            var commandBuffer = EndSimECBSystem.CreateCommandBuffer();
            ScrollingCombatTextMessage.Create(Plugin.Server.EntityManager, commandBuffer, XPAssetGuid, location, XPTextColour, character, value);
        }

        public static void AddItemToInventory(ChatCommandContext ctx, PrefabGUID guid, int amount)
        {
            var gameData = Plugin.Server.GetExistingSystemManaged<GameDataSystem>();
            var itemSettings = AddItemSettings.Create(Plugin.Server.EntityManager, gameData.ItemHashLookupMap);
            var inventoryResponse = InventoryUtilitiesServer.TryAddItem(itemSettings, ctx.Event.SenderCharacterEntity, guid, amount);
        }

        private struct FakeNull
        {
            public int value;
            public bool has_value;
        }
        public static bool TryGiveItem(Entity characterEntity, PrefabGUID itemGuid, int amount, out Entity itemEntity)
        {
            itemEntity = Entity.Null;

            var gameData = Plugin.Server.GetExistingSystemManaged<GameDataSystem>();
            var itemSettings = AddItemSettings.Create(Plugin.Server.EntityManager, gameData.ItemHashLookupMap);

            unsafe
            {
                var bytes = stackalloc byte[Marshal.SizeOf<FakeNull>()];
                var bytePtr = new IntPtr(bytes);
                Marshal.StructureToPtr(new FakeNull { value = 0, has_value = true }, bytePtr, false);
                var boxedBytePtr = IntPtr.Subtract(bytePtr, 0x10);
                var hack = new Il2CppSystem.Nullable<int>(boxedBytePtr);
                var inventoryResponse = InventoryUtilitiesServer.TryAddItem(
                    itemSettings,
                    characterEntity,
                    itemGuid,
                    amount);
                if (inventoryResponse.Success)
                {
                    itemEntity = inventoryResponse.NewEntity;
                    return true;
                }

                return false;
            }
        }
        public static Entity AddItemToInventory(Entity recipient, PrefabGUID guid, int amount, out bool result)
        {
            result = false;
            try
            {
                ServerGameManager serverGameManager = Plugin.Server.GetExistingSystemManaged<ServerScriptMapper>()._ServerGameManager;
                var inventoryResponse = serverGameManager.TryAddInventoryItem(recipient, guid, amount);
                if (inventoryResponse.Success) {
                    result = true;
                        }
                return inventoryResponse.NewEntity;
            }
            catch (System.Exception e)
            {
                Plugin.BepinLogger.LogInfo("didn't work");
            }
            return new Entity();
        }
        public static void DropItemNearby(Entity characterEntity, PrefabGUID itemGuid, int amount)
        {
            InventoryUtilitiesServer.CreateDropItem(Plugin.Server.EntityManager, characterEntity, itemGuid, amount, new Entity());
        }


        public static PrefabGUID GetPrefabGUID(Entity entity)
        {
            var entityManager = Plugin.Server.EntityManager;
            if (entity == Entity.Null || !entityManager.TryGetComponentData<PrefabGUID>(entity, out var prefabGuid))
            {
                prefabGuid = new PrefabGUID(0);
            }

            return prefabGuid;
        }
        public static void TeleportTo(ChatCommandContext ctx, float3 position)
        {
            var entity = Plugin.Server.EntityManager.CreateEntity(
                    ComponentType.ReadWrite<FromCharacter>(),
                    ComponentType.ReadWrite<PlayerTeleportDebugEvent>()
                );

            Plugin.Server.EntityManager.SetComponentData<FromCharacter>(entity, new()
            {
                User = ctx.Event.SenderUserEntity,
                Character = ctx.Event.SenderCharacterEntity
            });

            Plugin.Server.EntityManager.SetComponentData<PlayerTeleportDebugEvent>(entity, new()
            {
                Position = new float3(position.x, position.y, position.z),
                Target = PlayerTeleportDebugEvent.TeleportTarget.Self
            });
        }

        public static bool IsInCastle(Entity user)
        {
            var userLocalToWorld = Plugin.Server.EntityManager.GetComponentData<LocalToWorld>(user);
            var userPosition = userLocalToWorld.Position;
            var query = Plugin.Server.EntityManager.CreateEntityQuery(
                ComponentType.ReadOnly<PrefabGUID>(),
                ComponentType.ReadOnly<LocalToWorld>(),
                ComponentType.ReadOnly<UserOwner>(),
                ComponentType.ReadOnly<CastleFloor>());

            foreach (var entityModel in query.ToEntityArray(Allocator.Temp))
            {
                if (!Plugin.Server.EntityManager.TryGetComponentData<LocalToWorld>(entityModel, out var localToWorld))
                {
                    continue;
                }
                var position = localToWorld.Position;
                if (Math.Abs(userPosition.x - position.x) < 3 && Math.Abs(userPosition.z - position.z) < 3)
                {
                    return true;
                }
            }
            return false;
        }
        public static string CamelCaseToSpaces(UnitStatType type)
        {
            var name = Enum.GetName(type);
            // Split words by camel case
            // ie, PhysicalPower => "Physical Power"
            return Regex.Replace(name, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
        }

        private struct IsSystemInitialised<T>()
        {
            public bool isInitialised = false;
            public T system = default;
        }

        public static EntityManager GetEntityManager()
        {
            if (Plugin.IsServer)
            {
                return Plugin.EntityManager;
            }
            else
            {
                return Plugin.ClientEntityManager;
            }
        }

        public static ModifyUnitStatBuff_DOTS MakeModifyUnitStatBuff_DOTS(UnitStatType type, float value,
            ModificationType modType)
        {
            return new ModifyUnitStatBuff_DOTS
            {
                StatType = type,
                Value = value,
                ModificationType = modType,
                Modifier = 1,
                Id = ModificationId.NewId(0)
            };
        }
    }
}