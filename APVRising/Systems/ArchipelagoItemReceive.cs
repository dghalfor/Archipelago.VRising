using APVRising.Archipelago;
using APVRising.Data;
using APVRising.Hooks;
using APVRising.Services;
using APVRising.Utils;
using Il2CppInterop.Runtime;
using KindredCommands.Models;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using VampireCommandFramework;
using static APVRising.Services.DataService;

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
        public static volatile bool PendingResync = false;
        public static bool firstTime = true;
        public override void OnUpdate()
        {

            if (Plugin.APClient != null && PendingResync)
            {
                PendingResync = false;
                Plugin.APClient.Resync();
            }

            Plugin.APClient.DeathLinkHandler?.KillPlayer();

            while (ArchipelagoClient.PendingMessages.TryDequeue(out var message))
            {
                FixedString512Bytes fixedMessage = new(message);
                ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref fixedMessage);
                if (ArchipelagoClient.PendingMessages.Count == 0 && firstTime)
                {
                    Plugin.BepinLogger.LogInfo($"[AP] First time processing messages, requesting resync");
                    PendingResync = true;
                    firstTime = false;
                }
            }

            while (Plugin.APClient != null && ProgressionHandler.IsResearching == false &&
                   ArchipelagoClient.PendingItems.TryDequeue(out var item))
            {
                Plugin.BepinLogger.LogInfo($"[AP] Processing item: {item.ItemName}");

                var userEntities = _userQuery.ToEntityArray(Allocator.Temp);
                Plugin.BepinLogger.LogInfo($"[AP] Unlocking tech for {userEntities.Length} users");
                var itemName = Plugin.APClient.GetItemNameFromId(item.ItemId);
                Plugin.BepinLogger.LogInfo($"[AP] Item name: {itemName}");
                // your unlock logic here per user entity
                foreach (var userEntity in userEntities)
                {
                    if (DataDicts.ItemToEntityName.TryGetValue(itemName, out var entityName))
                    {
                        if (DataDicts.TechToPrefab.TryGetValue(entityName, out var prefab))
                        {
                            if (entityName.StartsWith("AB"))
                            {
                                ProgressionHandler.UnlockSpellAbilityForPlayer(userEntity, prefab);
                                ChatMessage.NotifyClientUnlockSpell(prefab.GuidHash);
                            }
                            else
                            {
                                ProgressionHandler.UnlockTechForPlayer(userEntity, prefab);
                                ChatMessage.NotifyClientUnlock(prefab.GuidHash);
                            }
                        }    
                    }

                    if (itemName.StartsWith("Item"))
                    {
                        var user = Plugin.EntityManager.GetComponentData<User>(userEntity);
                        string count = itemName.Split(" ")[2];
                        string name = itemName.Replace("Item -", "").Replace($" {count} ", "");
                        if (int.TryParse(count, out int quantity))
                        {
                            if (DataDicts.ItemsToPrefab.TryGetValue(name, out var prefab))
                            {
                                string key = Plugin.ServerSaveName + "-" + user.CharacterName;
                                long itemId = item.LocationId;

                                if (PlayerDictionaries._PlayerItemReceivedData.TryGetValue(key, out var existing) && existing.Items.Contains(itemId))
                                {
                                    continue; // already received, skip
                                }
                                if (Helper.TryGiveItem(user.LocalCharacter._Entity, prefab, quantity, out var _))
                                {
                                    PlayerDictionaries._PlayerItemReceivedData.AddOrUpdate(
                                        key,
                                        _ => new PlayerItemReceivedData([itemId]),
                                        (_, existing) => new PlayerItemReceivedData([.. existing.Items, itemId])
                                    );

                                    PlayerPersistence.SavePlayerItemReceivedData();
                                }
                            }
                        }
                    }
                }
                userEntities.Dispose();
            }
        }

        // KindredCommands GiveBloodPotion/GiveBloodMerlot
        public static void GiveBloodPotion(Entity user,BloodType type = BloodType.Frailed, float quality = 100f, int quantity = 1)
        {
            quality = Mathf.Clamp(quality, 0, 100);
            for (var i = 0; i < quantity; i++)
            {
                if (!Utils.Helper.TryGiveItem(user, new PrefabGUID(1223264867), 1, out var entity)) { 
                    //Not sure how to get drop and get the entity to rewrite it
                    //Utils.Helper.DropItemNearby(user, new PrefabGUID(1223264867), 1);
                }

                var blood = new StoredBlood()
                {
                    BloodQuality = quality,
                    PrimaryBloodType = new PrefabGUID((int)type)
                };

                Plugin.EntityManager.SetComponentData(entity, blood);
            }
        }

        public static void GiveBloodMerlotCommand(Entity user, BloodType primaryType = BloodType.Frailed, float primaryQuality = 100f, BloodType secondaryType = BloodType.Frailed, float secondaryQuality = 100f, int secondaryTrait = 1, int quantity = 1)
        {
            primaryQuality = Mathf.Clamp(primaryQuality, 0, 100);
            secondaryQuality = Mathf.Clamp(secondaryQuality, 0, 100);
            secondaryTrait = Mathf.Clamp(secondaryTrait, 1, 3);
            for (var i = 0; i < quantity; i++)
            {
                if (!Utils.Helper.TryGiveItem(user, new PrefabGUID(1223264867), 1, out var entity))
                {
                    //Not sure how to get drop and get the entity to rewrite it
                    //Utils.Helper.DropItemNearby(user, new PrefabGUID(1223264867), 1);
                }

                var blood = new StoredBlood()
                {
                    BloodQuality = primaryQuality,
                    PrimaryBloodType = new PrefabGUID((int)primaryType),
                    SecondaryBlood = new()
                    {
                        Quality = secondaryQuality,
                        Type = new PrefabGUID((int)secondaryType),
                        BuffIndex = (byte)(secondaryTrait - 1)
                    }
                };

                Plugin.EntityManager.SetComponentData(entity, blood);
            }
        }
    }
}

