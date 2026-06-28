using APVRising.Archipelago;
using APVRising.Utils;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using Stunlock.Core;
using System;
using Unity.Collections;
using Unity.Entities;

namespace APVRising.Hooks;

[HarmonyPatch]
public static class ChatMessage
{
    // Server side - send hidden command via chat
    public static void NotifyClient(bool isResearching)
    {
        // Find the chat message system
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of AP state change: IsResearching={isResearching}, Users={users.Length}");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            // Send a special prefixed message the client can intercept
            var message = (FixedString512Bytes) $"##AP_STATE#{isResearching}##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }

        users.Dispose();
    }
    public static void NotifyClientResearch(bool isResearching)
    {
        // Find the chat message system
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of AP state change: IsResearching={isResearching}, Users={users.Length}");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            // Send a special prefixed message the client can intercept
            var message = (FixedString512Bytes)$"##RESEARCH#{isResearching}##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }

        users.Dispose();
    }
    public static void NotifyClientUnlock(int guid)
    {
        // Find the chat message system
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of AP list change: GUID={guid}, Users={users.Length}");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            // Send a special prefixed message the client can intercept
            var message = (FixedString512Bytes)$"##AP_UNLOCK#{guid}##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }

        users.Dispose();
    }
    public static void NotifyClientUnlockAchievement(int guid)
    {
        // Find the chat message system
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of AP list change: GUID={guid}, Users={users.Length}");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            // Send a special prefixed message the client can intercept
            var message = (FixedString512Bytes)$"##UNLOCKACHIEVEMENT#{guid}##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }

        users.Dispose();
    }

    public static void NotifyClientLock(int guid)
    {
        // Find the chat message system
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of AP list change: GUID={guid}, Users={users.Length}");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            // Send a special prefixed message the client can intercept
            var message = (FixedString512Bytes)$"##LOCKPROG#{guid}##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }

        users.Dispose();
    }
    public static void NotifyClientLockSpell(int guid)
    {
        // Find the chat message system
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of AP list change: GUID={guid}, Users={users.Length}");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            // Send a special prefixed message the client can intercept
            var message = (FixedString512Bytes)$"##LOCKSPELL#{guid}##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }

        users.Dispose();
    }
    public static void NotifyClientUnlockSpell(int guid)
    {
        // Find the chat message system
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of AP list change: GUID={guid}, Users={users.Length}");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            // Send a special prefixed message the client can intercept
            var message = (FixedString512Bytes)$"##UNLOCKSPELL#{guid}##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }

        users.Dispose();
    }
    public static void NotifyClientSync()
    {
        // Find the chat message system
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of Resync");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            // Send a special prefixed message the client can intercept
            var message = (FixedString512Bytes)$"##RESYNC##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }

        users.Dispose();
    }
    public static void NotifyClientSnapshot()
    {
        // Find the chat message system
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of capture");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            // Send a special prefixed message the client can intercept
            var message = (FixedString512Bytes)$"##CAPTURE##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }

        users.Dispose();
    }
    public static void NotifyClientRestore()
    {
        // Find the chat message system
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of restore");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            // Send a special prefixed message the client can intercept
            var message = (FixedString512Bytes)$"##RESTORE##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }

        users.Dispose();
    }

    public static void NotifyClientLocation(int guid)
    {
        // Find the chat message system
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of Location change: GUID={guid}, Users={users.Length}");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            // Send a special prefixed message the client can intercept
            var message = (FixedString512Bytes)$"##LOCATION#{guid}##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }

        users.Dispose();
    }
    public static void NotifyClientCheck(int guid)
    {
        // Find the chat message system
        var em = Plugin.EntityManager;
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>());
        var users = userQuery.ToEntityArray(Allocator.Temp);
        Plugin.BepinLogger.LogInfo($"Notifying clients of check change: GUID={guid}, Users={users.Length}");
        foreach (var userEntity in users)
        {
            var user = em.GetComponentData<User>(userEntity);
            // Send a special prefixed message the client can intercept
            var message = (FixedString512Bytes)$"##CHECK#{guid}##";
            ServerChatUtils.SendSystemMessageToClient(em, user, ref message);
        }

        users.Dispose();
    }

    [HarmonyPatch(typeof(ClientChatSystem), "OnUpdate")]
    [HarmonyPrefix]
    public static void ClientChatOnUpdatePostfix(ClientChatSystem __instance)
    {
        var em = __instance.EntityManager;
        var query = em.CreateEntityQuery(ComponentType.ReadOnly<ChatMessageServerEvent>());
        if (query.IsEmpty) return;

        var events = query.ToEntityArray(Allocator.Temp);
        foreach (var eventEntity in events)
        {
            try
            {
                var chatEvent = em.GetComponentData<ChatMessageServerEvent>(eventEntity);
                string message = chatEvent.MessageText.ToString();

                if (message.StartsWith("##AP_STATE#"))
                {
                    bool isResearching = message.Contains("True");

                    var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
                    var entities = progQuery.ToEntityArray(Allocator.Temp);
                    foreach (var entity in entities) { 
                        if (isResearching) {
                            ProgressionSnapshot.Capture(em, entity);
                        } else
                        {
                            ProgressionSnapshot.Restore(em, entity);
                        }
                    }
                    ProgressionHandler.IsResearching = isResearching;
                    ProgressionHandler.isStale = true;
                    Plugin.BepinLogger.LogInfo($"Client AP state: IsResearching={isResearching}");

                    // Destroy so it doesn't appear in chat UI
                    em.DestroyEntity(eventEntity);
                }
                if (message.StartsWith("##AP_UNLOCK#"))
                {
                    string guidStr = message.Replace("##AP_UNLOCK#", "").Replace("##", "");
                    if (int.TryParse(guidStr, out int guid))
                    {
                        Plugin.BepinLogger.LogInfo($"Client AP unlock: GUID={guid}");
                        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
                        var userEntities = userQuery.ToEntityArray(Allocator.Temp);
                        foreach (var userEntity in userEntities)
                        {
                            ProgressionHandler.UnlockTechForPlayer(userEntity, new Stunlock.Core.PrefabGUID(guid));
                        }
                    }
                    else
                    {
                        Plugin.BepinLogger.LogError($"Failed to parse AP unlock GUID from message: {message}");
                    }
                    // Destroy so it doesn't appear in chat UI
                    em.DestroyEntity(eventEntity);
                }
                if (message.StartsWith("##LOCKPROG#"))
                {
                    string guidStr = message.Replace("##LOCKPROG#", "").Replace("##", "");
                    if (int.TryParse(guidStr, out int guid))
                    {
                        Plugin.BepinLogger.LogInfo($"Client lock: GUID={guid}");
                        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
                        var userEntities = userQuery.ToEntityArray(Allocator.Temp);
                        foreach (var userEntity in userEntities)
                        {
                            ProgressionHandler.LockTechForPlayer(userEntity, new Stunlock.Core.PrefabGUID(guid));
                        }
                    }
                    else
                    {
                        Plugin.BepinLogger.LogError($"Failed to parse AP unlock GUID from message: {message}");
                    }
                    // Destroy so it doesn't appear in chat UI
                    em.DestroyEntity(eventEntity);
                }
                if (message.StartsWith("##LOCKSPELL#"))
                {
                    string guidStr = message.Replace("##LOCKSPELL#", "").Replace("##", "");
                    if (int.TryParse(guidStr, out int guid))
                    {
                        Plugin.BepinLogger.LogInfo($"Client lock spell: GUID={guid}");
                        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
                        var userEntities = userQuery.ToEntityArray(Allocator.Temp);
                        foreach (var userEntity in userEntities)
                        {
                            ProgressionHandler.LockSpellAbilityForPlayer(userEntity, new Stunlock.Core.PrefabGUID(guid));
                        }
                    }
                    else
                    {
                        Plugin.BepinLogger.LogError($"Failed to parse AP unlock GUID from message: {message}");
                    }
                    // Destroy so it doesn't appear in chat UI
                    em.DestroyEntity(eventEntity);
                }
                if (message.StartsWith("##UNLOCKSPELL#"))
                {
                    string guidStr = message.Replace("##UNLOCKSPELL#", "").Replace("##", "");
                    if (int.TryParse(guidStr, out int guid))
                    {
                        Plugin.BepinLogger.LogInfo($"Client unlock spell: GUID={guid}");
                        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
                        var userEntities = userQuery.ToEntityArray(Allocator.Temp);
                        foreach (var userEntity in userEntities)
                        {
                            ProgressionHandler.UnlockSpellAbilityForPlayer(userEntity, new Stunlock.Core.PrefabGUID(guid));
                        }
                    }
                    else
                    {
                        Plugin.BepinLogger.LogError($"Failed to parse AP unlock GUID from message: {message}");
                    }
                    // Destroy so it doesn't appear in chat UI
                    em.DestroyEntity(eventEntity);
                }
                if (message.StartsWith("##RESYNC#"))
                {
                    Plugin.BepinLogger.LogInfo($"Client Resync");
                    //Plugin.APClient.Resync();
                    // Destroy so it doesn't appear in chat UI
                    em.DestroyEntity(eventEntity);
                }
                if (message.StartsWith("##RESEARCH#"))
                {
                    Plugin.BepinLogger.LogInfo($"RESEARCH");
                    bool isResearching = message.Contains("True");

                    ProgressionHandler.setResearch(isResearching);
                    // Destroy so it doesn't appear in chat UI
                    em.DestroyEntity(eventEntity);
                }
                if (message.StartsWith("##CAPTURE#"))
                {
                    Plugin.BepinLogger.LogInfo($"Client Capture");
                    ProgressionHandler.IsResearching = true;
                    var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
                    var entities = progQuery.ToEntityArray(Allocator.Temp);
                    foreach (var entity in entities)
                    {
                        ProgressionSnapshot.Capture(em, entity);
                    }
                    // Destroy so it doesn't appear in chat UI
                    em.DestroyEntity(eventEntity);
                }
                if (message.StartsWith("##RESTORE#"))
                {
                    Plugin.BepinLogger.LogInfo($"Client Restore");
                    var progQuery = Helper.GetEntityManager().CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
                    var entities = progQuery.ToEntityArray(Allocator.Temp);
                    foreach (var entity in entities)
                    {
                        ProgressionSnapshot.Restore(em, entity);
                    }
                    ProgressionHandler.IsResearching = false;
                    // Destroy so it doesn't appear in chat UI
                    em.DestroyEntity(eventEntity);
                }
                if (message.StartsWith("##LOCATION#"))
                {
                    Plugin.BepinLogger.LogInfo($"LOCATION");
                    string guidStr = message.Replace("##LOCATION#", "").Replace("##", "");
                    if (int.TryParse(guidStr, out int guid))
                    {
                        Plugin.BepinLogger.LogInfo($"Location: GUID={guid}");
                        ArchipelagoData.AddLocationCheck(guid);
                        // Destroy so it doesn't appear in chat UI
                        em.DestroyEntity(eventEntity);
                    }
                    }
                if (message.StartsWith("##CHECK#"))
                {
                    Plugin.BepinLogger.LogInfo($"CHECK");
                    string guidStr = message.Replace("##CHECK#", "").Replace("##", "");
                    if (int.TryParse(guidStr, out int guid))
                    {
                        Plugin.BepinLogger.LogInfo($"Check: GUID={guid}");
                        ArchipelagoData.AddReceivedCheck(guid);
                        // Destroy so it doesn't appear in chat UI
                        em.DestroyEntity(eventEntity);
                    }
                }
                if (message.StartsWith("##UNLOCKACHIEVEMENT#"))
                {
                    Plugin.BepinLogger.LogInfo($"UNLOCKACHIEVEMENT");
                    string guidStr = message.Replace("##UNLOCKACHIEVEMENT#", "").Replace("##", "");
                    if (int.TryParse(guidStr, out int guid))
                    {
                        Plugin.BepinLogger.LogInfo($"UNLOCKACHIEVEMENT: GUID={guid}");
                        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
                        var userEntities = userQuery.ToEntityArray(Allocator.Temp);
                        foreach (var userEntity in userEntities)
                        {
                            ProgressionHandler.UnlockAchievementForPlayer(userEntity, new Stunlock.Core.PrefabGUID(guid));
                        }
                        // Destroy so it doesn't appear in chat UI
                        em.DestroyEntity(eventEntity);
                    }
                }
                if (message.StartsWith("##AP_RESEARCH#"))
                {
                    string guidStr = message.Replace("##AP_RESEARCH#", "").Replace("##", "");
                    if (int.TryParse(guidStr, out int guid))
                    {
                        Plugin.BepinLogger.LogInfo($"Client AP research: GUID={guid}");
                        var researchGuid = new PrefabGUID(guid);
                        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<User>(), ComponentType.ReadOnly<ProgressionMapper>());
                        var userEntities = userQuery.ToEntityArray(Allocator.Temp);
                        foreach (var userEntity in userEntities)
                        {
                            ResearchDeskHandler.DisplayUnlockInResearchStation(userEntity, researchGuid, em);
                        }
                    }
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
}
