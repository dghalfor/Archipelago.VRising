using APVRising.Archipelago;
using APVRising.Hooks;
using APVRising.Utils;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.TextCore.Text;
using VampireCommandFramework;
using VRisingArchipelago;

namespace APVRising.Commands;

//[CommandGroup("archipelago", "ap")]
public static class ArchipelagoCommands
{
    [Command("connect", shortHand: "c", description: "Connect to Archipelago", adminOnly: false)]
    public static void APConnect(ICommandContext ctx, string slotName = "Player1", string uri = "archipelago.gg:38281", string password = "")
    {
        ArchipelagoClient.ServerData.Uri = uri;
        ArchipelagoClient.ServerData.Password = password;
        ArchipelagoClient.ServerData.SlotName = slotName;
        Plugin.APClient.Connect();
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
        var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
        var entities = progQuery.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            ProgressionSnapshot.Capture(Plugin.EntityManager, entity);
        }
        ChatMessage.NotifyClient(true);
        ctx.Reply($"Starting research...");
    }
    [Command("stopresearch")]
    public static void APStopResearch(ICommandContext ctx)
    {
        var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
        var entities = progQuery.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            ProgressionSnapshot.Restore(Plugin.EntityManager, entity);
        }
        ProgressionHandler.IsResearching = false;
        ProgressionHandler.UpdateProgression();
        ChatMessage.NotifyClient(false);
        Plugin.APClient.Resync();
        ctx.Reply($"Stopping research...");
    }

    [Command("srd")]
    public static void APStopResearchDeferred(ICommandContext ctx)
    {
        DelaySystem.StopResearchDeferred();
        ctx.Reply($"Stopping research deferred...");
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
            ProgressionHandler.UnlockTechForPlayer(userEntity, new Stunlock.Core.PrefabGUID(guid));
        }
        userEntities.Dispose();
        ChatMessage.NotifyClientUnlock(guid);
        ctx.Reply($"Unlocking tech with GUID: {guid}");
    }

    [Command("lockTech")]
    public static void APLockTech(ICommandContext ctx, int guid)
    {
        var log = Plugin.BepinLogger;

        var query = Plugin.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
        var userEntities = query.ToEntityArray(Allocator.Temp);
        log.LogInfo($"Locking tech for {userEntities.Length} users");
        foreach (var userEntity in userEntities)
        {
            ProgressionHandler.LockTechForPlayer(userEntity, new Stunlock.Core.PrefabGUID(guid));
        }
        userEntities.Dispose();
        ChatMessage.NotifyClientLock(guid);
        ctx.Reply($"Locking tech with GUID: {guid}");
    }

    [Command("lockSpell")]
    public static void APLockSpell(ICommandContext ctx, int guid)
    {
        var log = Plugin.BepinLogger;

        var query = Plugin.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
        var userEntities = query.ToEntityArray(Allocator.Temp);
        log.LogInfo($"Locking spell for {userEntities.Length} users");
        foreach (var userEntity in userEntities)
        {
            ProgressionHandler.LockSpellAbilityForPlayer(userEntity, new Stunlock.Core.PrefabGUID(guid));
        }
        userEntities.Dispose();
        ChatMessage.NotifyClientLockSpell(guid);
        ctx.Reply($"Locking spell with GUID: {guid}");
    }

    [Command("unlockSpell")]
    public static void APUnlockSpell(ICommandContext ctx, int guid)
    {
        var log = Plugin.BepinLogger;

        var query = Plugin.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
        var userEntities = query.ToEntityArray(Allocator.Temp);
        log.LogInfo($"unlocking spell for {userEntities.Length} users");
        foreach (var userEntity in userEntities)
        {
            ProgressionHandler.UnlockSpellAbilityForPlayer(userEntity, new Stunlock.Core.PrefabGUID(guid));
        }
        userEntities.Dispose();
        ChatMessage.NotifyClientUnlockSpell(guid);
        ctx.Reply($"Unlock spell with GUID: {guid}");
    }

    [Command("giveItem")]
    public static void GiveItem(ICommandContext ctx, int guid)
    {

        var query = Plugin.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var userEntities = query.ToEntityArray(Allocator.Temp);
        foreach (var userEntity in userEntities)
        {
            var user = Plugin.EntityManager.GetComponentData<User>(userEntity);

            var entity = Helper.AddItemToInventory(user.LocalCharacter._Entity, new PrefabGUID(guid), 1, out var result);
            if (result) {
                ctx.Reply($"Gave item with guid: {guid}");
            }
            ctx.Reply($"Could not give item with guid: {guid}");
        }
    }
    [Command("sync")]
    public static void APSync(ICommandContext ctx)
    {
        Plugin.APClient.Resync();
        ctx.Reply($"Synced Progression");
    }
    [Command("dedup", description: "Deduplicate progression buffers", adminOnly: true)]
    public static void APDedup(ICommandContext ctx)
    {
        var em = Plugin.EntityManager;
        var query = em.CreateEntityQuery(
            ComponentType.ReadOnly<User>(),
            ComponentType.ReadOnly<ProgressionMapper>());

        if (query.IsEmpty)
        {
            ctx.Reply("No user entities found.");
            return;
        }

        var userEntities = query.ToEntityArray(Allocator.Temp);
        foreach (var userEntity in userEntities)
        {
            var progressionQuery = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            var progressionEntities = progressionQuery.ToEntityArray(Allocator.Temp);

            foreach (var entity in progressionEntities)
            {
                int bpBefore = em.GetBuffer<UnlockedBlueprintElement>(entity).Length;
                int recipeBefore = em.GetBuffer<UnlockedRecipeElement>(entity).Length;

                ProgressionHandler.DeduplicateBuffer(em.GetBuffer<UnlockedBlueprintElement>(entity), e => e.UnlockedBlueprint);
                ProgressionHandler.DeduplicateBuffer(em.GetBuffer<UnlockedRecipeElement>(entity), e => e.UnlockedRecipe);
                ProgressionHandler.DeduplicateBuffer(em.GetBuffer<UnlockedShapeshiftElement>(entity), e => e.UnlockedShapeshift);

                int bpAfter = em.GetBuffer<UnlockedBlueprintElement>(entity).Length;
                int recipeAfter = em.GetBuffer<UnlockedRecipeElement>(entity).Length;

                ctx.Reply($"Deduped: Blueprints {bpBefore}→{bpAfter}, Recipes {recipeBefore}→{recipeAfter}");
            }
            ProgressionHandler.DeduplicateBuffer(em.GetBuffer<UnlockedProgressionElement>(userEntity), e => e.UnlockedPrefab);

            progressionEntities.Dispose();
        }
        userEntities.Dispose();
    }
}