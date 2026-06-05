using APVRising.Archipelago;
using APVRising.Services;
using HarmonyLib;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using VRisingArchipelago;

namespace APVRising.Hooks
{
    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    public static class InitializationPatch
    {
        [HarmonyPostfix]
        static void OnUserConnectedPostfix()
        {
            Plugin.BepinLogger.LogInfo("Persistence Load Detected. Attempting to connect to Archipelago Server if data is found.");
            try
            {
                DataService.PlayerPersistence.LoadArchipelagoData();
                DataService.PlayerDictionaries._ArchipelagoData.TryGetValue(Plugin.ServerSaveName, out var connectionData);
                ArchipelagoClient.ServerData.Uri = connectionData.IP;
                ArchipelagoClient.ServerData.Password = connectionData.Password;
                ArchipelagoClient.ServerData.SlotName = connectionData.SlotName;
                Plugin.APClient.Connect();
                DelaySystem.DisconnectReminderDeferred();
            }
            catch
            {
                var fixedString = new FixedString512Bytes("Archipelago Data could not be found. Please connect to an archipelago by opening the chat window and typing .connect [Playername] [IP:port] [Password]");
                ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref fixedString);
            }
        }
    }
}
