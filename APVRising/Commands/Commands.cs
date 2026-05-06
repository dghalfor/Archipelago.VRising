using APVRising.Archipelago;
using APVRising.Hooks;
using APVRising.Utils;
using ProjectM;
using ProjectM.Network;
using System;
using Unity.Collections;
using Unity.Entities;
using VampireCommandFramework;

namespace APVRising.Commands;

//[CommandGroup("archipelago", "ap")]
public static class ArchipelagoCommands
{
    //[Command("connect", shortHand: "c", description: "Connect to Archipelago", adminOnly: false)]
    public static void APConnect(ICommandContext ctx, string slotName = "Player1", string uri = "archipelago.gg:38281", string password = "")
    {
        Archipelago.ArchipelagoClient.ServerData.Uri = uri;
        Archipelago.ArchipelagoClient.ServerData.Password = password;
        Archipelago.ArchipelagoClient.ServerData.SlotName = slotName;
        Archipelago.ArchipelagoClient.Instance.Connect();
    }

    //[Command("deathlink", shortHand: "dl", description: "Toggle Death Link", adminOnly: false)]
    public static void APDeathLinkToggle(ICommandContext ctx, bool? value = null)
    {
        if (value != null)
        {
            Archipelago.DeathLinkHandler.deathLinkEnabled = value.Value;
        }
        ctx.Reply($"[Archipelago] Death link is {(Archipelago.DeathLinkHandler.deathLinkEnabled ? "on" : "off")}");
    }
    [Command("startresearch")]
    public static void APStartResearch(ICommandContext ctx)
    {
        ProgressionHandler.IsResearching = true;
        ProgressionHandler.UpdateProgression();
        ChatMessage.NotifyClient(true);
        ctx.Reply($"Starting research...");
    }
    [Command("stopresearch")]
    public static void APStopResearch(ICommandContext ctx)
    {
        ProgressionHandler.IsResearching = false;
        ProgressionHandler.UpdateProgression();
        ChatMessage.NotifyClient(false);
        ctx.Reply($"Stopping research...");
    }
    [Command("unlockTech")]
    public static void APUnlockTech(ICommandContext ctx, int guid)
    {
        var log = Plugin.BepinLogger;

        var query = Plugin.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
        var userEntities = query.ToEntityArray(Allocator.Temp);
        log.LogInfo($"Unlocking tech for {userEntities.Length} users");
        foreach (var userEntity in userEntities)
        {
            ProgressionHandler.UnlockResearchForPlayer(userEntity, new Stunlock.Core.PrefabGUID(guid));
        }
        ArchipelagoData.APProgression.Add(guid);
        userEntities.Dispose();
        ChatMessage.NotifyClientUnlock(guid);
        ctx.Reply($"Unlocking tech with GUID: {guid}");
    }
}