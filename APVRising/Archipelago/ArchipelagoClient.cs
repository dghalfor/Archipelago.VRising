using APVRising.Data;
using APVRising.Hooks;
using APVRising.Systems;
using APVRising.Utils;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace APVRising.Archipelago;

// Shamelessly stolen (& adapted) code from ArchipelagoBepInExPluginTemplate. Same goes for the rest of the files in this directory.
public class ArchipelagoClient
{
    public const string APVersion = "0.6.7";
    private const string Game = "V Rising";

    public static bool Authenticated;
    private bool attemptingConnection;

    public static ArchipelagoData ServerData = new();
    internal DeathLinkHandler DeathLinkHandler;
    private ArchipelagoSession session;

    /// <summary>
    /// call to connect to an Archipelago session. Connection info should already be set up on ServerData
    /// </summary>
    /// <returns></returns>
    public void Connect()
    { 
        if (Authenticated || attemptingConnection) return;

        try
        {
            session = ArchipelagoSessionFactory.CreateSession(ServerData.Uri);
            SetupSession();
        }
        catch (Exception e)
        {
            Plugin.BepinLogger.LogError(e);
        }

        TryConnect();
        
    }

    /// <summary>
    /// add handlers for Archipelago events
    /// </summary>
    private void SetupSession()
    {
        session.MessageLog.OnMessageReceived += (message) =>
        {
            ArchipelagoConsole.LogMessage(message.ToString());
            FixedString512Bytes fixedMessage = new(message.ToString());
            PendingMessages.Enqueue(fixedMessage.ToString());
        };
        session.Items.ItemReceived += OnItemReceived;
        session.Socket.ErrorReceived += OnSessionErrorReceived;
        session.Socket.SocketClosed += OnSessionSocketClosed;
        //session.Locations.CompleteLocationChecksAsync += 
    }

    /// <summary>
    /// attempt to connect to the server with our connection info
    /// </summary>
    private void TryConnect()
    {
        try
        {
            // it's safe to thread this function call but unity notoriously hates threading so do not use excessively
            ThreadPool.QueueUserWorkItem(
                _ => HandleConnectResult(
                    session.TryConnectAndLogin(
                        Game,
                        ServerData.SlotName,
                        ItemsHandlingFlags.AllItems, 
                        new Version(APVersion),
                        password: ServerData.Password,
                        requestSlotData: true // ServerData.NeedSlotData
                    )));
        }
        catch (Exception e)
        {
            Plugin.BepinLogger.LogError(e);
            HandleConnectResult(new LoginFailure(e.ToString()));
            attemptingConnection = false;
        }
    }

    public static System.Collections.Concurrent.ConcurrentQueue<string> PendingMessages = new();

    /// <summary>
    /// handle the connection result and do things
    /// </summary>
    /// <param name="result"></param>
    private void HandleConnectResult(LoginResult result)
    {
        string outText;
        if (result.Successful)
        {
            var success = (LoginSuccessful)result;

            ServerData.SetupSession(success.SlotData, session.RoomState.Seed);
            Authenticated = true;

            DeathLinkHandler = new(session.CreateDeathLinkService(), ServerData.SlotName);
            session.Locations.CompleteLocationChecksAsync(ServerData.CheckedLocations1.ToArray());
            outText = $"Successfully connected to {ServerData.Uri} as {ServerData.SlotName}!";

            ArchipelagoConsole.LogMessage(outText);
        }
        else
        {
            var failure = (LoginFailure)result;
            outText = $"Failed to connect to {ServerData.Uri} as {ServerData.SlotName}.";
            outText = failure.Errors.Aggregate(outText, (current, error) => current + $"\n    {error}");

            Plugin.BepinLogger.LogError(outText);

            Authenticated = false;
            Disconnect();
        }

        FixedString512Bytes outTextFixed = new(outText);
        PendingMessages.Enqueue(outText.ToString());
        ArchipelagoConsole.LogMessage(outText);
        ArchipelagoItemSystem.PendingResync = true;
        attemptingConnection = false;
    }

    /// <summary>
    /// something went wrong, or we need to properly disconnect from the server. cleanup and re null our session
    /// </summary>
    private void Disconnect()
    {
        Plugin.BepinLogger.LogDebug("disconnecting from server...");
        FixedString512Bytes fixedString = new($"Disconnecting from server");
        ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref fixedString);
        session?.Socket.DisconnectAsync();
        session = null;
        Authenticated = false;
    }

    public void SendMessage(string message)
    {
        session.Socket.SendPacketAsync(new SayPacket { Text = message });
    }

    public string GetItemNameFromId(long itemId)
    {
        return session.Items.GetItemName(itemId);
    }

    public void SendLocationCheck(string locationName)
    {
        try
        {
            var locationIds = new List<long>();

            if (DataDicts.EntityNameToAPLocation.TryGetValue(locationName, out var primaryLocation))
                locationIds.Add(session.Locations.GetLocationIdFromName(Game, primaryLocation));
            CheckGoalLocation(primaryLocation);

            var bonusLocationDicts = new[]
            {
            DataDicts.BonusVictoryLocations,
            DataDicts.BonusSpellPointLocations,
        };

            foreach (var dict in bonusLocationDicts)
                if (dict.TryGetValue(locationName, out var bonusLocation))
                    locationIds.Add(session.Locations.GetLocationIdFromName(Game, bonusLocation));

            if (locationIds.Count > 0)
                session.Locations.CompleteLocationChecksAsync(locationIds.ToArray());
        }
        catch (Exception e)
        {
            FixedString512Bytes fixedString = new($"Could not send location check, please make sure you are connected by entering the command '.connect', or exception: {e.ToString()}");
            ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref fixedString);
        }
    }

    public bool IsConfiguredLocation(string locationName)
    {
        return session.Locations.AllLocations.Contains(session.Locations.GetLocationIdFromName(Game, locationName));
    }

    private void CheckGoalLocation(string locationName)
    {
        Plugin.BepinLogger.LogInfo($"Checking if {locationName} is goal location {ServerData.SlotDataOpts()}");
        if (locationName == ServerData.SlotDataOpts())
        {
            session.SetGoalAchieved();
        }
    }

    /// <summary>
    /// we received an item so reward it here
    /// </summary>
    /// <param name="helper">item helper which we can grab our item from</param>
    /// 
    public static ConcurrentQueue<ItemInfo> PendingItems = new ConcurrentQueue<ItemInfo>();

    private void OnItemReceived(ReceivedItemsHelper helper)
    {
        var receivedItem = helper.DequeueItem();
        Plugin.BepinLogger.LogInfo($"[AP] Received item: {receivedItem.ItemName} (ID: {receivedItem.ItemId})");
        if (helper.Index <= ServerData.Index) return;

        ServerData.Index++;

        // Don't touch EntityManager here - just queue the item
        PendingItems.Enqueue(receivedItem);
        Plugin.BepinLogger.LogInfo($"[AP] Queued item: {receivedItem.ItemName}");
    }

    /// <summary>
    /// something went wrong with our socket connection
    /// </summary>
    /// <param name="e">thrown exception from our socket</param>
    /// <param name="message">message received from the server</param>
    private void OnSessionErrorReceived(Exception e, string message)
    {
        Plugin.BepinLogger.LogError(e);
        FixedString512Bytes fixedString = new($"{message}");
        ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref fixedString);
        ArchipelagoConsole.LogMessage(message);
    }

    /// <summary>
    /// something went wrong closing our connection. disconnect and clean up
    /// </summary>
    /// <param name="reason"></param>
    private void OnSessionSocketClosed(string reason)
    {
        Plugin.BepinLogger.LogError($"Connection to Archipelago lost: {reason}");
        FixedString512Bytes fixedString = new($"Connection to Archipelago lost: {reason}");
        ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref fixedString);
        Disconnect();
    }

    // Resync removes all progression unlocks for locations that are checked but not received from the player. This undoes the progression changes that occur during startup.
    public void Resync()
    {

        var query = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
        var userEntities = query.ToEntityArray(Allocator.Temp);
        var progressionQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
        var progressionEntities = progressionQuery.ToEntityArray(Allocator.Temp);

        // If progression element contains something that does not exist in the locations it will be granted here
        foreach (var entity in progressionEntities)
        {
            var progBuffer = Helper.GetEntityManager().GetBuffer<UnlockedProgressionElement>(entity);
            for (int i = 0; i < progBuffer.Length; i++)
            {
                if (DataDicts.EntityNameToAPLocation.TryGetValue(DebugTool.GetPrefabName(progBuffer[i].UnlockedPrefab), out var unlockedLocationName))
                {
                    if (!Plugin.APClient.IsConfiguredLocation(unlockedLocationName))
                    {
                        Plugin.BepinLogger.LogInfo($"Player does not have {progBuffer[i].UnlockedPrefab._Value} but it is not a configured location, adding to unlocks");
                        ArchipelagoData.AddLocationCheck(progBuffer[i].UnlockedPrefab._Value);
                        ChatMessage.NotifyClientLocation(progBuffer[i].UnlockedPrefab._Value);
                        ArchipelagoData.AddReceivedCheck(progBuffer[i].UnlockedPrefab._Value);
                        ChatMessage.NotifyClientCheck(progBuffer[i].UnlockedPrefab._Value);
                    }
                }
            }
        }

        var checkedLocations = session.Locations.AllLocationsChecked;
        var receivedItemLocationIds = session.Items.AllItemsReceived
            .Select(item => item.ItemId)
            .ToHashSet();
        var validLocationNames = session.Locations.AllLocations;
        foreach (var checks in checkedLocations)
        {
            Plugin.BepinLogger.LogInfo($"checked {checks}");

            if (DataDicts.EntityNameToAPLocation.TryGetValue(session.Locations.GetLocationNameFromId(checks), out var locEntityName))
            {

                if (DataDicts.TechToPrefab.TryGetValue(locEntityName, out var prefab))
                {
                    Plugin.BepinLogger.LogInfo($"checked {prefab.GuidHash}");
                    ArchipelagoData.AddLocationCheck(prefab.GuidHash);
                    ChatMessage.NotifyClientLocation(prefab.GuidHash);
                }
            }
        }

        foreach (var received in receivedItemLocationIds) 
        {
                if (DataDicts.ItemToEntityName.TryGetValue(session.Items.GetItemName(received), out var checkEntityName))
                {
                    if (DataDicts.TechToPrefab.TryGetValue(checkEntityName, out var prefab))
                    {
                    Plugin.BepinLogger.LogInfo($"Check Entity Name {checkEntityName}");
                    Plugin.BepinLogger.LogInfo($"recieved {prefab.GuidHash}");

                    ArchipelagoData.AddReceivedCheck(prefab.GuidHash);
                    ChatMessage.NotifyClientCheck(prefab.GuidHash);
                    }
                }
        }
        var locationsToLock = checkedLocations
            .Where(locationId => !receivedItemLocationIds.Contains(locationId) && validLocationNames.Contains(locationId))
            .ToList();

        Plugin.BepinLogger.LogInfo($"[AP] Sync: {checkedLocations.Count} checked, {receivedItemLocationIds.Count} received, {locationsToLock.Count} to lock");

        foreach (var locationId in locationsToLock)
        {
            var locationName = session.Locations.GetLocationNameFromId(locationId);
            Plugin.BepinLogger.LogInfo($"[AP] Locking: {locationId} ({locationName})");
            foreach (var userEntity in userEntities) { 
                if (DataDicts.APLocationToEntityName.TryGetValue(locationName, out var entityName))
                {
                    if (DataDicts.TechToPrefab.TryGetValue(entityName, out var prefab))
                    {
                        //spells
                        if (entityName.StartsWith("AB"))
                        {
                            ProgressionHandler.LockSpellAbilityForPlayer(userEntity, prefab);
                            ChatMessage.NotifyClientLockSpell(prefab.GuidHash);
                        } else
                        {
                            ProgressionHandler.LockTechForPlayer(userEntity, prefab);
                        }
                        
                    }
                } 
            }
        }
        userEntities.Dispose();
    }

    private static Dictionary<string, string> entityNameToAPLocation;
    /// <summary>
    /// Fetch a dictionary of entity names and AP location names. May not be all-inclusive, check at runtime.
    /// </summary>
    public static Dictionary<string, string> EntityNameToAPLocation
    {
        get
        {
            if (entityNameToAPLocation == null)
            {
                string json = string.Empty;
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("APVRising.Data.EntityNameToAPLocation.json"))
                using (var reader = new StreamReader(stream))
                {
                    json = reader.ReadToEnd();
                }
                JsonNode node = JsonNode.Parse(json);
                Plugin.BepinLogger.LogInfo(json);
                entityNameToAPLocation = node.Deserialize<Dictionary<string, string>>();
            }

            return entityNameToAPLocation;
        }
    }
}