using APVRising.Archipelago;
using APVRising.Services;
using APVRising.Utils;
using HarmonyLib;
using ProjectM;
using Stunlock.Core.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
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
                var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
                var ProgEntities = progQuery.ToEntityArray(Allocator.Temp);
                foreach (var progEntity in ProgEntities)
                {
                    Plugin.BepinLogger.LogInfo("Saving baseline prog entity");

                    ProgressionSnapshot.CaptureBaseline(Plugin.EntityManager, progEntity);
                    DelaySystem.ClientBaselineCapture();
                }
                DataService.PlayerPersistence.LoadArchipelagoData();
                DataService.PlayerDictionaries._ArchipelagoData.TryGetValue(Plugin.ServerSaveName, out var connectionData);
                ArchipelagoClient.ServerData.Uri = connectionData.IP;
                ArchipelagoClient.ServerData.Password = connectionData.Password;
                ArchipelagoClient.ServerData.SlotName = connectionData.SlotName;
                ProgressionHandler.IsResearching = true;
                Plugin.APClient.Connect();
                DelaySystem.DisconnectReminderDeferred();
                DataService.PlayerPersistence.LoadPlayerItemReceivedData();
                DataService.PlayerPersistence.LoadPlayerShapeshiftData();
                //DelaySystem.NotifyClientConfiguredLocations();
                foreach (var progEntity in ProgEntities)
                {
                    DelaySystem.ReconcileWithAP(progEntity);
                    DelaySystem.SlowRestoreDeferred(progEntity);
                }
                DelaySystem.StopResearchDeferredSlow();
            }
            catch
            {
                var fixedString = new FixedString512Bytes("Archipelago Data could not be found. Please connect to an archipelago by opening the chat window and typing .connect [Playername] [IP:port] [Password]");
                ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref fixedString);
            }
        }
    }
}
