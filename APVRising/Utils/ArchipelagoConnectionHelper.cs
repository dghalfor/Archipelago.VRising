using APVRising.Services;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using VRisingArchipelago;

namespace APVRising.Utils
{
    public class ArchipelagoConnectionHelper
    {
        public static void PerformFullArchipelagoConnect()
        {
            Plugin.BepinLogger.LogInfo("PerformFullArchipelagoConnect: starting connect + capture + reconcile sequence");

            ProgressionHandler.IsResearching = true;

            // Fire first so the background handshake has maximum time to complete
            // while the rest of this synchronous work runs.
            Plugin.APClient.Connect();

            var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            var progEntities = progQuery.ToEntityArray(Allocator.Temp);

            foreach (var progEntity in progEntities)
            {
                Plugin.BepinLogger.LogInfo("Saving baseline prog entity");
                ProgressionSnapshot.CaptureBaseline(Plugin.EntityManager, progEntity);
                DelaySystem.ClientBaselineCapture();
            }

            DelaySystem.DisconnectReminderDeferred();
            DataService.PlayerPersistence.LoadPlayerItemReceivedData();
            DataService.PlayerPersistence.LoadPlayerShapeshiftData();

            foreach (var progEntity in progEntities)
            {
                var capturedEntity = progEntity; // struct copy, safe past Allocator.Temp scope
                DelaySystem.WaitForAuthenticationThenDeferred(
                    onAuthenticated: () =>
                    {
                        DelaySystem.ReconcileWithAP(capturedEntity);
                        DelaySystem.SlowRestoreDeferred(capturedEntity);
                    },
                    onTimeout: () =>
                    {
                        var msg = new FixedString512Bytes("<color=red>Archipelago connection timed out. Progression could not be reconciled. Try '.connect' manually.</color>");
                        ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref msg);
                    }
                );
            }

            DelaySystem.StopResearchDeferredSlow();
        }
    }
   
}

