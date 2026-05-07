using APVRising.Utils;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using ProjectM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Unity.Collections;

namespace APVRising.Archipelago;

// Shamelessly stolen (& adapted) code from ArchipelagoBepInExPluginTemplate. Same goes for the rest of the files in this directory.
public class ArchipelagoClient
{
    public static ArchipelagoClient Instance;

    public const string APVersion = "0.6.7";
    private const string Game = "Manual_VRising_Phye";

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
            ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref fixedMessage);
        };
        session.Items.ItemReceived += OnItemReceived;
        session.Socket.ErrorReceived += OnSessionErrorReceived;
        session.Socket.SocketClosed += OnSessionSocketClosed;
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
                        ItemsHandlingFlags.NoItems, // TODO make sure to change this line
                        new Version(APVersion),
                        password: ServerData.Password,
                        requestSlotData: false // ServerData.NeedSlotData
                    )));
        }
        catch (Exception e)
        {
            Plugin.BepinLogger.LogError(e);
            HandleConnectResult(new LoginFailure(e.ToString()));
            attemptingConnection = false;
        }
    }

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
            session.Locations.CompleteLocationChecksAsync(ServerData.CheckedLocations.ToArray());
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
        ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref outTextFixed);
        ArchipelagoConsole.LogMessage(outText);
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

    /// <summary>
    /// we received an item so reward it here
    /// </summary>
    /// <param name="helper">item helper which we can grab our item from</param>
    private void OnItemReceived(ReceivedItemsHelper helper)
    {
        var receivedItem = helper.DequeueItem();

        if (helper.Index <= ServerData.Index) return;

        ServerData.Index++;

        FixedString512Bytes fixedString = new($"Recieved {receivedItem.ItemName} from {receivedItem.Player} ({receivedItem.LocationDisplayName})");
        ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref fixedString);

        // TODO reward the item here
        // if items can be received while in an invalid state for actually handling them, they can be placed in a local
        // queue/collection to be handled later
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