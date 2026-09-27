using ApVRising;
using APVRising.Archipelago;
using APVRising.Data;
using APVRising.Utils;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using Stunlock.Core;
using System;
using Unity.Collections;
using Unity.Entities;
using VRisingArchipelago;

namespace APVRising.Hooks;

[HarmonyPatch]
public static class ChatMessage
{
    // ── Batched, ack-tracked sends (unlock/lock/spell/achievement/prog) ─────────────
    //
    // These no longer send immediately — they enqueue into BatchAccumulator, which
    // coalesces everything queued within a short window into one versioned, ack-tracked
    // ##BATCH#...## message per player. See BatchAccumulator.cs / AckTracker.cs /
    // BatchCodeTable.cs.
    //
    // NOTE: signature change from the original all-users-broadcast versions — these now
    // take a specific userEntity, since batching only makes sense per-recipient. Call
    // sites that relied on the old "send to every connected user" behavior (e.g. inside
    // ArchipelagoItemReceive.cs's per-user loop) should call these once per user in that
    // loop rather than expecting an internal broadcast.

    public static void NotifyClientUnlock(Entity userEntity, int guid) =>
        BatchAccumulator.Enqueue(userEntity, 'U', guid);

    public static void NotifyClientUnlockSpell(Entity userEntity, int guid) =>
        BatchAccumulator.Enqueue(userEntity, 'S', guid);

    public static void NotifyClientLock(Entity userEntity, int guid) =>
        BatchAccumulator.Enqueue(userEntity, 'L', guid);

    public static void NotifyClientLockSpell(Entity userEntity, int guid) =>
        BatchAccumulator.Enqueue(userEntity, 'K', guid);

    public static void NotifyClientUnlockAchievement(Entity userEntity, int guid) =>
        BatchAccumulator.Enqueue(userEntity, 'A', guid);

    public static void NotifyClientLockProg(Entity userEntity, int guid) =>
        BatchAccumulator.Enqueue(userEntity, 'P', guid);

    // "This location has been checked" ('V', visited) and "this item has been received" ('C',
    // check) bookkeeping for the client's own ArchipelagoData.CheckedLocations/ReceivedChecks
    // copies. These used to go out as unacked NotifyClientLocation(int)/NotifyClientCheck(int)
    // broadcasts with no retry at all; routing them through the same batched, ack-tracked pipe
    // as everything else means a dropped delivery gets resent instead of silently leaving the
    // client's bookkeeping permanently short an entry.
    public static void NotifyClientLocation(Entity userEntity, int guid) =>
        BatchAccumulator.Enqueue(userEntity, 'V', guid);

    public static void NotifyClientCheck(Entity userEntity, int guid) =>
        BatchAccumulator.Enqueue(userEntity, 'C', guid);

    // ── Unchanged broadcast-style notifications ──────────────────────────────────────

    public static void NotifyClient(bool isResearching)
    {
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of AP state change: IsResearching={isResearching}, Users={users.Length}");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            var message = (FixedString512Bytes)$"##AP_STATE#{isResearching}##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }
        users.Dispose();
    }

    public static void NotifyClientResearch(bool isResearching)
    {
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of AP state change: IsResearching={isResearching}, Users={users.Length}");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            var message = (FixedString512Bytes)$"##RESEARCH#{isResearching}##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }
        users.Dispose();
    }

    public static void NotifyClientSync()
    {
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of Resync");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            var message = (FixedString512Bytes)$"##RESYNC##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }
        users.Dispose();
    }

    public static void NotifyClientSnapshot()
    {
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of capture");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            var message = (FixedString512Bytes)$"##CAPTURE##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }
        users.Dispose();
    }

    public static void NotifyClientRestore()
    {
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of restore");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            var message = (FixedString512Bytes)$"##RESTORE##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }
        users.Dispose();
    }

    public static void NotifyClientLocation(int guid)
    {
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of Location change: GUID={guid}, Users={users.Length}");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            var message = (FixedString512Bytes)$"##LOCATION#{guid}##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }
        users.Dispose();
    }

    public static void NotifyClientCheck(int guid)
    {
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of check change: GUID={guid}, Users={users.Length}");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            var message = (FixedString512Bytes)$"##CHECK#{guid}##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }
        users.Dispose();
    }

    public static void NotifyClientClearSnapshots()
    {
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of snapshot clear");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            var message = (FixedString512Bytes)$"##CLEARSNAPSHOTS##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }
        users.Dispose();
    }

    public static void NotifyClientCaptureBaseline()
    {
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of baseline capture");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            var message = (FixedString512Bytes)$"##CAPTUREBASELINE##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }
        users.Dispose();
    }

    public static void NotifyClientReconcile()
    {
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of AP reconcile");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            var message = (FixedString512Bytes)$"##RECONCILE##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
            DelaySystem.DelayDoneReconcile(user);
        }
        users.Dispose();
    }

    public static void NotifyClientDoneReconcile(User user)
    {
        var em = Plugin.EntityManager;
        var message = (FixedString512Bytes)$"##DONERECONCILE##";
        ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
    }

    public static void NotifyClientDoneConfiguring(User user)
    {
        var em = Plugin.EntityManager;
        var message = (FixedString512Bytes)$"##DONECONFIGURING##";
        ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
    }
    public static void NotifyClientConfiguredLocations()
    {
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of AP reconcile");
        foreach (var userEntity in users)
        {
            ConfiguredLocationsSync.SendToUser(userEntity);
        }
        users.Dispose();
    }
    // NOTE: the old per-item NotifyClientConfiguredLocations loop (sending one
    // ##CONFIGUREDLOCATION#{guid}## message per configured location per user) has been
    // replaced by ConfiguredLocationsSync.SendToUser(userEntity), which sends the whole
    // set as ack-tracked ##CFGLOC#...## batches. Call ConfiguredLocationsSync.SendToUser
    // at the point a player needs this sync instead of calling into ChatMessage for it.

    // ── Client-side receive ──────────────────────────────────────────────────────────

    // Cached across frames: creating an EntityQuery is not free, and this patch runs every
    // ClientChatSystem.OnUpdate. Rebuilt only if the world's EntityManager instance changes
    // (e.g. the client world is recreated).
    private static EntityManager _cachedQueryEm;
    private static EntityQuery _cachedMessageQuery;

    [HarmonyPatch(typeof(ClientChatSystem), "OnUpdate")]
    [HarmonyPrefix]
    public static void ClientChatOnUpdatePrefix(ClientChatSystem __instance)
    {
        var em = __instance.EntityManager;
        if (_cachedQueryEm != em)
        {
            _cachedQueryEm = em;
            _cachedMessageQuery = em.CreateEntityQuery(ComponentType.ReadOnly<ChatMessageServerEvent>());
        }
        var query = _cachedMessageQuery;
        if (query.IsEmpty) return;

        var events = query.ToEntityArray(Allocator.Temp);
        foreach (var eventEntity in events)
        {
            try
            {
                var chatEvent = em.GetComponentData<ChatMessageServerEvent>(eventEntity);

                // Pre-filter: only inspect System-type messages, so a player typing text
                // that happens to start with "##" can't spoof the control channel, and we
                // skip string-matching against ordinary chat.
                if (chatEvent.MessageType != ServerChatMessageType.System)
                    continue;

                string message = chatEvent.MessageText.ToString();

                if (message.StartsWith("##BATCH#", StringComparison.Ordinal))
                {
                    HandleBatch(em, message, eventEntity);
                    continue;
                }

                if (message.StartsWith("##CFGLOC#", StringComparison.Ordinal))
                {
                    HandleConfiguredLocations(message, eventEntity, em);
                    continue;
                }

                if (message.StartsWith("##UNLOCKSYNC#", StringComparison.Ordinal))
                {
                    HandleUnlockSync(em, message, eventEntity);
                    continue;
                }

                if (message.StartsWith("##AP_STATE#", StringComparison.Ordinal))
                {
                    bool isResearching = message.Contains("True");

                    var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
                    var entities = progQuery.ToEntityArray(Allocator.Temp);
                    foreach (var entity in entities)
                    {
                        if (isResearching)
                            ProgressionSnapshot.Capture(em, entity);
                    }
                    ProgressionHandler.IsResearching = isResearching;
                    ProgressionHandler.isStale = true;
                    Plugin.BepinLogger.LogInfo($"Client AP state: IsResearching={isResearching}");
                    em.DestroyEntity(eventEntity);
                }
                else if (message.StartsWith("##RESYNC#", StringComparison.Ordinal))
                {
                    Plugin.BepinLogger.LogInfo($"Client Resync");
                    em.DestroyEntity(eventEntity);
                }
                else if (message.StartsWith("##RESEARCH#", StringComparison.Ordinal))
                {
                    Plugin.BepinLogger.LogInfo($"RESEARCH");
                    bool isResearching = message.Contains("True");
                    ProgressionHandler.setResearch(isResearching);
                    em.DestroyEntity(eventEntity);
                }
                else if (message.StartsWith("##CAPTURE#", StringComparison.Ordinal))
                {
                    Plugin.BepinLogger.LogInfo($"Client Capture");
                    ProgressionHandler.IsResearching = true;
                    var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
                    var entities = progQuery.ToEntityArray(Allocator.Temp);
                    foreach (var entity in entities)
                        ProgressionSnapshot.Capture(em, entity);
                    em.DestroyEntity(eventEntity);
                }
                else if (message.StartsWith("##RESTORE#", StringComparison.Ordinal))
                {
                    Plugin.BepinLogger.LogInfo($"Client Restore");
                    var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
                    var entities = progQuery.ToEntityArray(Allocator.Temp);
                    foreach (var entity in entities)
                        ProgressionSnapshot.Restore(em, entity);
                    ProgressionHandler.IsResearching = false;
                    em.DestroyEntity(eventEntity);
                }
                else if (message.StartsWith("##LOCATION#", StringComparison.Ordinal))
                {
                    Plugin.BepinLogger.LogInfo($"LOCATION");
                    string guidStr = message.Replace("##LOCATION#", "").Replace("##", "");
                    if (int.TryParse(guidStr, out int guid))
                    {
                        Plugin.BepinLogger.LogInfo($"Location: GUID={guid}");
                        ArchipelagoData.AddLocationCheck(guid);
                        em.DestroyEntity(eventEntity);
                    }
                }
                else if (message.StartsWith("##CHECK#", StringComparison.Ordinal))
                {
                    Plugin.BepinLogger.LogInfo($"CHECK");
                    string guidStr = message.Replace("##CHECK#", "").Replace("##", "");
                    if (int.TryParse(guidStr, out int guid))
                    {
                        Plugin.BepinLogger.LogInfo($"Check: GUID={guid}");
                        ArchipelagoData.AddReceivedCheck(guid);
                        em.DestroyEntity(eventEntity);
                    }
                }
                else if (message.StartsWith("##CAPTUREBASELINE#", StringComparison.Ordinal))
                {
                    Plugin.BepinLogger.LogInfo($"Client Baseline Capture");
                    ResetVersionTracking();
                    var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
                    var entities = progQuery.ToEntityArray(Allocator.Temp);
                    foreach (var entity in entities)
                        ProgressionSnapshot.CaptureBaseline(Plugin.ClientEntityManager, entity);
                    entities.Dispose();
                    em.DestroyEntity(eventEntity);
                }
                else if (message.StartsWith("##RECONCILE#", StringComparison.Ordinal))
                {
                    if (!ArchipelagoData.doneReconciling)
                    {
                        Plugin.BepinLogger.LogInfo($"Client Reconcile");
                        var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
                        var entities = progQuery.ToEntityArray(Allocator.Temp);
                        foreach (var entity in entities)
                            ProgressionSnapshot.ReconcileWithAP(Plugin.ClientEntityManager, entity);
                        entities.Dispose();
                        em.DestroyEntity(eventEntity);
                    }
                }
                else if (message.StartsWith("##DONECONFIGURING#", StringComparison.Ordinal))
                {
                    ArchipelagoData.doneConfiguring = true;
                    em.DestroyEntity(eventEntity);
                }
                else if (message.StartsWith("##DONERECONCILE#", StringComparison.Ordinal))
                {
                    ArchipelagoData.doneReconciling = true;
                    em.DestroyEntity(eventEntity);
                }
            }
            catch (Exception e)
            {
                Plugin.BepinLogger.LogError($"Chat intercept error: {e}");
            }
        }
        events.Dispose();
    }

    // ── Version staleness guard ──────────────────────────────────────────────────────
    //
    // BATCH, CFGLOC and UNLOCKSYNC all draw from the SAME per-player monotonic counter
    // (BatchVersioning.NextVersion), so a single "highest version fully applied" watermark
    // correctly orders all three message kinds against each other, not just messages of the
    // same kind. AckTracker's retry can redeliver an old version after a newer one has
    // already landed (e.g. a slow ack on v5 while v6 has already arrived and applied); without
    // this guard, re-applying v5 after v6 would silently undo whatever v6 changed. A message at
    // or below the watermark is acked immediately, without touching game state, so the server
    // stops retrying it.
    //
    // The watermark is intentionally never reset on disconnect: BatchVersioning's server-side
    // counter for a given player is monotonic for the lifetime of the server process (it is
    // never reset either), so a reconnecting client should keep comparing against wherever it
    // left off. It IS reset on ##CAPTUREBASELINE#, which fires at the start of every
    // .connect/.reconnect sequence, to recover cleanly from the one case where the numbering
    // legitimately restarts from 1: the AP server process itself restarting.
    private static int _lastAppliedVersion = 0;

    private static bool IsStaleVersion(int version) => version <= _lastAppliedVersion;

    private static void MarkVersionApplied(int version)
    {
        if (version > _lastAppliedVersion)
            _lastAppliedVersion = version;
    }

    internal static void ResetVersionTracking()
    {
        _lastAppliedVersion = 0;
    }

    // ── Batch/sync parsing helpers ───────────────────────────────────────────────────

    private static void HandleBatch(EntityManager em, string message, Entity eventEntity)
    {
        var parts = message.Trim('#').Split('#'); // ["BATCH", version, count, entryBlock]
        if (parts.Length == 4 && int.TryParse(parts[1], out int version) && int.TryParse(parts[2], out int count)
            && parts[3].Length == count * 4)
        {
            if (IsStaleVersion(version))
            {
                Plugin.BepinLogger.LogInfo($"[AP] Skipping stale/duplicate batch v{version} (already at v{_lastAppliedVersion})");
                AckSender.SendAck(version);
                em.DestroyEntity(eventEntity);
                return;
            }

            var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
            var userEntities = userQuery.ToEntityArray(Allocator.Temp);

            bool allResolved = true;
            for (int i = 0; i < count; i++)
            {
                char action = parts[3][i * 4];
                string code = parts[3].Substring(i * 4 + 1, 3);

                if (!BatchCodeTable.CodeToPrefab.TryGetValue(code, out var prefab))
                {
                    allResolved = false;
                    Plugin.BepinLogger.LogError($"[AP] Unknown code '{code}' in batch v{version}. Registry mismatch?");
                    continue;
                }
                ApplyAction(action, prefab, userEntities);
            }
            userEntities.Dispose();

            // Reject-and-let-retry on any unresolved entry: a batch we can't fully
            // apply must not be acked, so the server's retry resends the whole thing.
            if (allResolved)
            {
                MarkVersionApplied(version);
                AckSender.SendAck(version);
            }
        }
        else
        {
            Plugin.BepinLogger.LogError($"[AP] Malformed batch header/body: {message}");
        }
        em.DestroyEntity(eventEntity);
    }

    private static void HandleConfiguredLocations(string message, Entity eventEntity, EntityManager em)
    {
        var parts = message.Trim('#').Split('#'); // ["CFGLOC", version, count, codeblock]
        if (parts.Length == 4 && int.TryParse(parts[1], out int version) && int.TryParse(parts[2], out int count)
            && parts[3].Length == count * 3)
        {
            if (IsStaleVersion(version))
            {
                Plugin.BepinLogger.LogInfo($"[AP] Skipping stale/duplicate CFGLOC v{version} (already at v{_lastAppliedVersion})");
                AckSender.SendAck(version);
                em.DestroyEntity(eventEntity);
                return;
            }

            bool allResolved = true;
            for (int i = 0; i < count; i++)
            {
                string code = parts[3].Substring(i * 3, 3);
                if (!BatchCodeTable.CodeToPrefab.TryGetValue(code, out var prefab))
                {
                    allResolved = false;
                    Plugin.BepinLogger.LogError($"[AP] Unknown CFGLOC code '{code}' in v{version}");
                    continue;
                }
                ArchipelagoData.ConfiguredLocations.Add(prefab._Value);
            }

            if (allResolved)
            {
                MarkVersionApplied(version);
                AckSender.SendAck(version);
            }
        }
        else
        {
            Plugin.BepinLogger.LogError($"[AP] Malformed CFGLOC message: {message}");
        }
        em.DestroyEntity(eventEntity);
    }

    private static void HandleUnlockSync(EntityManager em, string message, Entity eventEntity)
    {
        var parts = message.Trim('#').Split('#'); // ["UNLOCKSYNC", version, count, entryBlock]
        if (parts.Length == 4 && int.TryParse(parts[1], out int version) && int.TryParse(parts[2], out int count)
            && parts[3].Length == count * 4)
        {
            if (IsStaleVersion(version))
            {
                Plugin.BepinLogger.LogInfo($"[AP] Skipping stale/duplicate UNLOCKSYNC v{version} (already at v{_lastAppliedVersion})");
                AckSender.SendAck(version);
                em.DestroyEntity(eventEntity);
                return;
            }

            var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
            var userEntities = userQuery.ToEntityArray(Allocator.Temp);

            bool allResolved = true;
            for (int i = 0; i < count; i++)
            {
                char action = parts[3][i * 4];
                string code = parts[3].Substring(i * 4 + 1, 3);

                if (!BatchCodeTable.CodeToPrefab.TryGetValue(code, out var prefab))
                {
                    allResolved = false;
                    Plugin.BepinLogger.LogError($"[AP] Unknown UNLOCKSYNC code '{code}' in v{version}");
                    continue;
                }
                ApplyAction(action, prefab, userEntities);
            }
            userEntities.Dispose();

            if (allResolved)
            {
                MarkVersionApplied(version);
                AckSender.SendAck(version);
            }
        }
        else
        {
            Plugin.BepinLogger.LogError($"[AP] Malformed UNLOCKSYNC message: {message}");
        }
        em.DestroyEntity(eventEntity);
    }

    // Shared by ##BATCH# and ##UNLOCKSYNC# — same entry format, same actions.
    private static void ApplyAction(char action, PrefabGUID prefab, NativeArray<Entity> userEntities)
    {
        if (action == 'P')
        {
            // Matches original behavior: applied once against the client EM, not per
            // queried user entity.
            ProgressionHandler.LockProg(Plugin.ClientEntityManager, prefab);
            return;
        }

        // 'V'/'C' are plain client-side bookkeeping (ArchipelagoData is a shared static, not
        // per-user), so -- like 'P' -- these apply once, not once per queried user entity.
        if (action == 'V')
        {
            ArchipelagoData.AddLocationCheck(prefab._Value);
            return;
        }
        if (action == 'C')
        {
            ArchipelagoData.AddReceivedCheck(prefab._Value);
            return;
        }

        foreach (var userEntity in userEntities)
        {
            switch (action)
            {
                case 'U': ProgressionHandler.UnlockTechForPlayer(userEntity, prefab); break;
                case 'S': ProgressionHandler.UnlockSpellAbilityForPlayer(userEntity, prefab); break;
                case 'L': ProgressionHandler.LockTechForPlayer(userEntity, prefab); break;
                case 'K': ProgressionHandler.LockSpellAbilityForPlayer(userEntity, prefab); break;
                case 'A': ProgressionHandler.UnlockAchievementForPlayer(userEntity, prefab); break;
                default:
                    Plugin.BepinLogger.LogError($"[AP] Unknown action char '{action}'");
                    break;
            }
        }
    }
}