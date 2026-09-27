using APVRising.Data;
using APVRising.Hooks;
using APVRising.Services;
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
using Stunlock.Core;
using Stunlock.Core.Animation;
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
using VRisingArchipelago;

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
        attemptingConnection = true;

        try
        {
            session = ArchipelagoSessionFactory.CreateSession(ServerData.Uri);
            SetupSession();
        }
        catch (Exception e)
        {
            Plugin.BepinLogger.LogError(e);
            session = null;
            attemptingConnection = false;
            return;
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
            var text = message.ToString();
            ArchipelagoConsole.LogMessage(text);
            PendingMessages.Enqueue(text); // truncated to chat size on the main thread
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
        var connectSession = session;
        // it's safe to thread this function call but unity notoriously hates threading so do not use excessively
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                HandleConnectResult(
                    connectSession.TryConnectAndLogin(
                        Game,
                        ServerData.SlotName,
                        ItemsHandlingFlags.AllItems,
                        new Version(APVersion),
                        password: ServerData.Password,
                        requestSlotData: true // ServerData.NeedSlotData
                    ));
            }
            catch (Exception e)
            {
                // An exception escaping a thread-pool thread would take the whole process down.
                Plugin.BepinLogger.LogError(e);
                try { HandleConnectResult(new LoginFailure(e.ToString())); }
                catch (Exception inner) { Plugin.BepinLogger.LogError(inner); attemptingConnection = false; }
            }
        });
    }

    /// <summary>
    /// Set by HandleConnectResult (thread pool) and consumed by ArchipelagoItemSystem.OnUpdate (main thread),
    /// which does the file loads and DeferredActionSystem scheduling that must not run off-thread.
    /// </summary>
    internal static volatile bool PendingPostConnect;

    /// <summary>
    /// Chat text queued from any thread; ArchipelagoItemSystem.OnUpdate sends it to clients on the main thread.
    /// </summary>
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

            DeathLinkHandler = new(session.CreateDeathLinkService(), ServerData.SlotName, ServerData.IsDeathLinkEnabled());
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

        PendingMessages.Enqueue(outText);
        ArchipelagoConsole.LogMessage(outText);
        attemptingConnection = false;

        // This method runs on a thread-pool thread: hand file loads and deferred scheduling to the main thread.
        PendingPostConnect = true;
    }

    /// <summary>
    /// something went wrong, or we need to properly disconnect from the server. cleanup and re null our session
    /// </summary>
    public void Disconnect()
    {
        Plugin.BepinLogger.LogDebug("disconnecting from server...");
        PendingMessages.Enqueue("Disconnecting from server");
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
            // Resolve which AP location names this game event maps to first, so unmapped
            // (vanilla) unlocks are ignored quietly even while disconnected.
            var locationNames = new List<string>();

            if (DataDicts.EntityNameToAPLocation.TryGetValue(locationName, out var primaryLocation))
            {
                locationNames.Add(primaryLocation);
            }

            var bonusLocationDicts = new[]
            {
                DataDicts.BonusVictoryLocations,
                DataDicts.BonusSpellPointLocations,
            };

            foreach (var dict in bonusLocationDicts)
                if (dict.TryGetValue(locationName, out var bonusLocation))
                    locationNames.Add(bonusLocation);

            if (DataDicts.BonusVBloodLocations.TryGetValue(locationName, out var bonusLocations))
                locationNames.AddRange(bonusLocations);

            if (locationNames.Count == 0)
                return;

            if (session == null)
                throw new InvalidOperationException("not connected to Archipelago");

            if (primaryLocation != null)
                CheckGoalLocation(primaryLocation);

            var locationIds = new List<long>();
            foreach (var name in locationNames)
            {
                if (string.IsNullOrEmpty(name))
                    continue; // placeholder entry with no AP location yet

                var id = session.Locations.GetLocationIdFromName(Game, name);
                if (id < 0)
                {
                    Plugin.BepinLogger.LogWarning($"[AP] Location '{name}' is not known to the AP server, skipping");
                    continue;
                }
                locationIds.Add(id);
            }

            if (locationIds.Count > 0)
                session.Locations.CompleteLocationChecksAsync(locationIds.ToArray());
        }
        catch (Exception e)
        {
            Plugin.BepinLogger.LogError(e);
            var fixedString = Helper.ToFixed512($"Could not send location check, please make sure you are connected by entering the command '.connect', or exception: {e.Message}");
            ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref fixedString);
        }
    }

    public bool IsConnected()
    {
        return session != null && session.Socket.Connected;
    }

    public bool IsConfiguredLocation(string locationName)
    {
        if (session == null || !session.Socket.Connected)
        {
            Plugin.BepinLogger.LogWarning($"Attempted to check if {locationName} is a configured location, but session is null or not connected.");
            return false;
        }
        var id = session.Locations.GetLocationIdFromName(Game, locationName);
        var result = session.Locations.AllLocations.Contains(id);
        return result;
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
        PendingMessages.Enqueue($"{message}");
        ArchipelagoConsole.LogMessage(message);
    }

    /// <summary>
    /// something went wrong closing our connection. disconnect and clean up
    /// </summary>
    /// <param name="reason"></param>
    private void OnSessionSocketClosed(string reason)
    {
        Plugin.BepinLogger.LogError($"Connection to Archipelago lost: {reason}");
        PendingMessages.Enqueue($"Connection to Archipelago lost: {reason}");
        Disconnect();
    }

    // Resync removes all progression unlocks for locations that are checked but not received from the player. This undoes the progression changes that occur during startup.
    public void Resync()
    {
        if (session == null || !session.Socket.Connected)
        {
            Plugin.BepinLogger.LogWarning("[AP Resync] Skipped: not connected to Archipelago");
            return;
        }

        Plugin.BepinLogger.LogInfo(
        $"[AP Resync] Starting. session.Items.AllItemsReceived.Count={session.Items.AllItemsReceived.Count}, " +
        $"IsConnected={session.Socket.Connected}"); // adjust property name to whatever your client exposes
        var em = Helper.GetEntityManager();

        // Hoisted from where this used to be declared (just before the grant/revoke section) so
        // every notification below can batch-notify per connected player instead of broadcasting.
        var userQuery = em.CreateEntityQuery(
            ComponentType.ReadOnly<User>(),
            ComponentType.ReadOnly<ProgressionMapper>());
        var userEntities = userQuery.ToEntityArray(Allocator.Temp);

        // --- Pre-pass: acknowledge unlocks that exist in the player's buffer but are
        //     not configured for this AP session (option-gated or vanilla unlocks).
        //     Marking them locally prevents the revoke pass from stripping them. ---
        var progressionQuery = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
        var progressionEntities = progressionQuery.ToEntityArray(Allocator.Temp);

        foreach (var entity in progressionEntities)
        {
            var progBuffer = em.GetBuffer<UnlockedProgressionElement>(entity);
            for (int i = 0; i < progBuffer.Length; i++)
            {
                var prefabName = DebugTool.GetPrefabName(progBuffer[i].UnlockedPrefab);
                if (string.IsNullOrEmpty(prefabName))
                {
                    Plugin.BepinLogger.LogWarning($"[AP Resync] Pre-pass: could not resolve name for prefab {progBuffer[i].UnlockedPrefab}, skipping");
                    continue;
                }

                if (DataDicts.EntityNameToAPLocation.TryGetValue(prefabName, out var locationName) &&
                    !IsConfiguredLocation(locationName))
                {
                    Plugin.BepinLogger.LogInfo($"[AP Resync] Not a configured location, acknowledging locally: {prefabName}");
                    ArchipelagoData.AddLocationCheck(progBuffer[i].UnlockedPrefab._Value);
                    ArchipelagoData.AddReceivedCheck(progBuffer[i].UnlockedPrefab._Value);
                    foreach (var notifyUser in userEntities)
                    {
                        ChatMessage.NotifyClientLocation(notifyUser, progBuffer[i].UnlockedPrefab._Value);
                        ChatMessage.NotifyClientCheck(notifyUser, progBuffer[i].UnlockedPrefab._Value);
                    }
                }
            }
        }
        progressionEntities.Dispose();

        // --- Build "should have" set from AP received items ---
        var shouldHavePrefabs = new HashSet<PrefabGUID>();
        foreach (var networkItem in session.Items.AllItemsReceived)
        {
            var itemName = session.Items.GetItemName(networkItem.ItemId);
            if (string.IsNullOrEmpty(itemName))
            {
                Plugin.BepinLogger.LogWarning($"[AP Resync] Could not resolve item name for ItemId={networkItem.ItemId}, skipping");
                continue;
            }

            if (DataDicts.ItemToEntityName.TryGetValue(itemName, out var entityName) &&
                DataDicts.TechToPrefab.TryGetValue(entityName, out var prefab))
            {
                shouldHavePrefabs.Add(prefab);
            }
        }

        // --- Mirror checked locations into ArchipelagoData ---
        foreach (var locationId in session.Locations.AllLocationsChecked)
        {
            var locationName = session.Locations.GetLocationNameFromId(locationId);
            if (string.IsNullOrEmpty(locationName))
            {
                Plugin.BepinLogger.LogWarning($"[AP Resync] Could not resolve location name for LocationId={locationId}, skipping");
                continue;
            }

            if (DataDicts.APLocationToEntityName.TryGetValue(locationName, out var entityName) &&
                DataDicts.TechToPrefab.TryGetValue(entityName, out var prefab))
            {
                ArchipelagoData.AddLocationCheck(prefab.GuidHash);
                foreach (var notifyUser in userEntities)
                    ChatMessage.NotifyClientLocation(notifyUser, prefab.GuidHash);
            }
        }

        // --- Per-user: grant missing, revoke extras ---
        foreach (var userEntity in userEntities)
        {
            // Snapshot what they currently have in UnlockedProgressionElement
            var progQuery = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            var progEntities = progQuery.ToEntityArray(Allocator.Temp);

            var doesHavePrefabs = new HashSet<PrefabGUID>();
            foreach (var progEntity in progEntities)
            {
                var progBuffer = em.GetBuffer<UnlockedProgressionElement>(progEntity);
                for (int i = 0; i < progBuffer.Length; i++)
                    doesHavePrefabs.Add(progBuffer[i].UnlockedPrefab);
            }

            // Grant: received by AP but missing from buffer
            foreach (var prefab in shouldHavePrefabs)
            {
                if (!doesHavePrefabs.Contains(prefab))
                {
                    Plugin.BepinLogger.LogInfo($"[AP Resync] Granting missing: {prefab.GuidHash}");
                    ArchipelagoData.AddReceivedCheck(prefab.GuidHash);
                    foreach (var notifyUser in userEntities)
                        ChatMessage.NotifyClientCheck(notifyUser, prefab.GuidHash);
                }
            }

            // Collect prefabs to revoke: in buffer, AP-managed, configured, not in shouldHave
            var toRevoke = new List<PrefabGUID>();
            foreach (var prefab in doesHavePrefabs)
            {
                if (shouldHavePrefabs.Contains(prefab))
                    continue;

                var prefabName = DebugTool.GetPrefabName(prefab);
                if (string.IsNullOrEmpty(prefabName))
                {
                    Plugin.BepinLogger.LogWarning($"[AP Resync] Revoke pass: could not resolve name for prefab {prefab}, skipping");
                    continue;
                }

                if (!DataDicts.EntityNameToAPLocation.TryGetValue(prefabName, out var locationName))
                    continue; // not AP-managed, leave it alone

                if (!IsConfiguredLocation(locationName))
                    continue; // excluded by player's options, pre-pass already acknowledged it

                toRevoke.Add(prefab);
            }

            // Revoke: two-phase per prefab
            foreach (var prefab in toRevoke)
            {
                var prefabName = DebugTool.GetPrefabName(prefab);
                if (string.IsNullOrEmpty(prefabName))
                {
                    Plugin.BepinLogger.LogWarning($"[AP Resync] Revoke: could not resolve name for prefab {prefab}, skipping revoke");
                    continue;
                }

                Plugin.BepinLogger.LogInfo($"[AP Resync] Revoking: {prefab.GuidHash} ({prefabName})");

                // Phase 1: strip recipes/blueprints/shapeshifts via existing methods.
                // Note: LockTechForPlayer has an early-return guard on ReceivedChecks —
                // ReceivedChecks was rebuilt from scratch above so this should be clean,
                // but if you ever see revokes being silently skipped, that guard is why.
                if (!DataDicts.EntityNameToAPLocation.TryGetValue(prefabName, out var entityName))
                {
                    Plugin.BepinLogger.LogWarning($"[AP Resync] Revoke: '{prefabName}' no longer resolves in EntityNameToAPLocation, skipping revoke");
                    continue;
                }

                if (entityName.StartsWith("AB"))
                {
                    ProgressionHandler.LockSpellAbilityForPlayer(userEntity, prefab);
                    //ChatMessage.NotifyClientLockSpell(prefab.GuidHash);
                }
                else
                {
                    ProgressionHandler.LockTechForPlayer(userEntity, prefab);
                }

                // Phase 2: remove from UnlockedProgressionElement so research mode
                // cannot re-derive access from a stale entry in that buffer.
                var revokeProgQuery = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
                var revokeProgEntities = revokeProgQuery.ToEntityArray(Allocator.Temp);
                foreach (var progEntity in revokeProgEntities)
                {
                    var progBuffer = em.GetBuffer<UnlockedProgressionElement>(progEntity);
                    for (int i = progBuffer.Length - 1; i >= 0; i--)
                    {
                        if (progBuffer[i].UnlockedPrefab == prefab)
                        {
                            progBuffer.RemoveAt(i);
                            ChatMessage.NotifyClientLockProg(userEntity, prefab.GuidHash);
                            break;
                        }
                    }
                }
                revokeProgEntities.Dispose();
            }

            Plugin.BepinLogger.LogInfo(
                $"[AP Resync] User done. ShouldHave={shouldHavePrefabs.Count}, " +
                $"DoesHave={doesHavePrefabs.Count}, " +
                $"Revoked={toRevoke.Count}");
            progEntities.Dispose();
        }

        // Progression entities are global, so capture and notify once rather than once per user.
        if (userEntities.Length > 0)
        {
            var snapQuery = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            var snapEntities = snapQuery.ToEntityArray(Allocator.Temp);
            foreach (var progEntity in snapEntities)
                ProgressionSnapshot.Capture(em, progEntity);
            snapEntities.Dispose();
            ChatMessage.NotifyClientSnapshot();
        }

        userEntities.Dispose();

        Plugin.BepinLogger.LogInfo($"[AP Resync] Complete. ShouldHave={shouldHavePrefabs.Count}");
    }
}