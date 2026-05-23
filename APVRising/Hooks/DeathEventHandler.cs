using APVRising.Archipelago;
using HarmonyLib;
using ProjectM;
using Stunlock.Core;
using System;
using Unity.Collections;

namespace APVRising.Hooks;

[HarmonyPatch]
public class DeathEventHandler
{
    [HarmonyPatch(typeof(DeathEventListenerSystem), "OnUpdate")]
    public static void Postfix(DeathEventListenerSystem __instance)
    {
        NativeArray<DeathEvent> deathEvents = __instance._DeathEventQuery.ToComponentDataArray<DeathEvent>(Allocator.Temp);
        foreach (DeathEvent ev in deathEvents)
        {
            var killer = ev.Killer;

            // If the entity killing is a minion, switch the killer to the owner of the minion.
            if (__instance.EntityManager.HasComponent<Minion>(killer))
            {
                if (__instance.EntityManager.TryGetComponentData<EntityOwner>(killer, out var entityOwner))
                {
                    killer = entityOwner.Owner;
                }
            }

            // If the killer is the victim, it probably shouldnt send data to AP.
            if (!killer.Equals(ev.Died))
            {
                if (__instance.EntityManager.HasComponent<PlayerCharacter>(killer))
                {
                    var entityName = Plugin.PrefabCollectionSystem._PrefabDataLookup[Plugin.EntityManager.GetComponentData<PrefabGUID>(ev.Died)].AssetName;
                    
                    string str = entityName.Value;
                    if (str.StartsWith("CHAR"))
                    {
                        Plugin.BepinLogger.LogInfo($"An instance of {str} was killed. Attempting AP check send.");
                        try
                        {
                            // TODO: Call to a System to queue an AP check
                            // ArchipelagoClient.EntityNameToAPLocation[str];
                        }
                        catch (Exception e)
                        {
                            FixedString512Bytes errorTruncated = new($"Error converting entity name to AP check: {e}");
                            Plugin.BepinLogger.LogError(errorTruncated);
                            ServerChatUtils.SendSystemMessageToAllClients(Plugin.Server.EntityManager, ref errorTruncated);
                        }
                    }
                }
            }

            // TODO: Kill everyone on death as AP setting? Send death link for each player or for everyone at once?
            // Player death
            if (DeathLinkHandler.deathLinkEnabled && __instance.EntityManager.TryGetComponentData<RespawnCharacter>(ev.Died, out var respawnData))
            {
                Plugin.APClient.DeathLinkHandler.SendDeathLink();
            }
        }
    }
}