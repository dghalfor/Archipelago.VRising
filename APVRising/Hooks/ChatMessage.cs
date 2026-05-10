using APVRising.Archipelago;
using APVRising.Utils;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using System;
using Unity.Collections;
using Unity.Entities;

namespace APVRising.Hooks;

[HarmonyPatch]
public static class ChatMessage
{
    /*
    // majority of this code adapted from VampireCommandFramework @ VCF.Core/Breadstone/ChatHook.cs
    [HarmonyPatch(typeof(ChatMessageSystem), nameof(ChatMessageSystem.OnUpdate))]
	public static void Prefix(ChatMessageSystem __instance)
	{
		if (__instance.__query_661171423_0 != null)
		{
			NativeArray<Entity> entities = __instance.__query_661171423_0.ToEntityArray(Allocator.Temp);
			foreach (var entity in entities)
			{
				// keeping this in case it's decided at some point that player names should be included in messages to AP
				// var fromData = __instance.EntityManager.GetComponentData<FromCharacter>(entity);
				// var userData = __instance.EntityManager.GetComponentData<User>(fromData.User);
				var chatEventData = __instance.EntityManager.GetComponentData<ChatMessageEvent>(entity);

				var messageText = chatEventData.MessageText.ToString();

				if (!(!messageText.StartsWith(".") || messageText.StartsWith(".."))) continue;

                if (ArchipelagoClient.Authenticated)
                    Plugin.ArchipelagoClient.SendMessage(messageText);
			}
		}
	}*/
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
                            ProgressionHandler.UnlockResearchForPlayer(userEntity, new Stunlock.Core.PrefabGUID(guid));
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
                            ProgressionHandler.LockResearchUnlocksForPlayer(userEntity, new Stunlock.Core.PrefabGUID(guid));
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
                    Plugin.APClient.Resync();
                    // Destroy so it doesn't appear in chat UI
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
