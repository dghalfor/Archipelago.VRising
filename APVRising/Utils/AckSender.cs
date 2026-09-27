using APVRising;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace VRisingArchipelago
{
    /// <summary>
    /// Sends ##ACK#{version}## from client to server using the game's native
    /// client-to-server chat event, constructed directly (not via typed chat).
    ///
    /// Requires local user/character entity resolution — plug in your existing
    /// lookup where marked below.
    /// </summary>
    public static class AckSender
    {
        private static readonly ComponentType[] NetworkEventComponents =
        [
            ComponentType.ReadOnly(Il2CppType.Of<FromCharacter>()),
            ComponentType.ReadOnly(Il2CppType.Of<NetworkEventType>()),
            ComponentType.ReadOnly(Il2CppType.Of<SendNetworkEventTag>()),
            ComponentType.ReadOnly(Il2CppType.Of<ChatMessageEvent>())
        ];

        private static readonly NetworkEventType ChatEventType = new()
        {
            IsAdminEvent = false,
            EventId = NetworkEvents.EventId_ChatMessageEvent,
            IsDebugEvent = false,
        };

        public static void SendAck(int version)
        {
            var localUser = LocalPlayer.LocalUser;
            var localCharacter = LocalPlayer.LocalCharacter;

            if (localUser == Entity.Null || localCharacter == Entity.Null)
            {
                Plugin.BepinLogger.LogWarning($"[AP] Cannot send ACK v{version}, local user not resolved yet.");
                return;
            }

            var userNetworkId = Plugin.ClientEntityManager.GetComponentData<NetworkId>(localUser);
            var chatMessageEvent = new ChatMessageEvent
            {
                MessageText = new FixedString512Bytes($"##ACK#{version}##"),
                MessageType = ChatMessageType.Local,
                ReceiverEntity = userNetworkId
            };

            var networkEntity = Plugin.ClientEntityManager.CreateEntity(NetworkEventComponents);
            Plugin.ClientEntityManager.SetComponentData(networkEntity, new FromCharacter { Character = localCharacter, User = localUser });
            Plugin.ClientEntityManager.SetComponentData(networkEntity, ChatEventType);
            Plugin.ClientEntityManager.SetComponentData(networkEntity, chatMessageEvent);

            Plugin.BepinLogger.LogInfo($"[AP] Sent ACK v{version}");
        }
    }

    /// <summary>
    /// Local user/character resolution using the client world from Plugin.
    /// This caches results to avoid repeated lookups.
    /// </summary>
    internal static class LocalPlayer
    {
        private static Entity _cachedLocalUser = Entity.Null;
        private static Entity _cachedLocalCharacter = Entity.Null;

        public static Entity LocalUser
        {
            get
            {
                // Return cached if still valid
                if (_cachedLocalUser != Entity.Null && Plugin.ClientEntityManager.Exists(_cachedLocalUser))
                {
                    // Verify the cached entity still has a User component
                    try
                    {
                        Plugin.ClientEntityManager.GetComponentData<ProjectM.Network.User>(_cachedLocalUser);
                        return _cachedLocalUser;
                    }
                    catch
                    {
                        _cachedLocalUser = Entity.Null;
                    }
                }

                _cachedLocalUser = Entity.Null;
                try
                {
                    var clientWorld = Plugin.Client;
                    var em = clientWorld.EntityManager;

                    // Query for entities with User component
                    var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<ProjectM.Network.User>());
                    var users = userQuery.ToEntityArray(Allocator.Temp);

                    if (users.Length > 0)
                    {
                        _cachedLocalUser = users[0];
                        Plugin.BepinLogger.LogInfo($"[AP] Resolved local user: {_cachedLocalUser.Index}");
                    }
                    else
                    {
                        Plugin.BepinLogger.LogDebug("[AP] No user entities found in client world");
                    }

                    users.Dispose();
                }
                catch (System.Exception ex)
                {
                    Plugin.BepinLogger.LogWarning($"[AP] Failed to resolve local user: {ex.Message}");
                }

                return _cachedLocalUser;
            }
        }

        public static Entity LocalCharacter
        {
            get
            {
                // Return cached if still valid
                if (_cachedLocalCharacter != Entity.Null && Plugin.ClientEntityManager.Exists(_cachedLocalCharacter))
                {
                    return _cachedLocalCharacter;
                }

                _cachedLocalCharacter = Entity.Null;
                try
                {
                    var clientWorld = Plugin.Client;
                    var em = clientWorld.EntityManager;

                    // Get the LocalCharacter from the User component
                    var userEntity = LocalUser;
                    if (userEntity != Entity.Null)
                    {
                        var user = em.GetComponentData<ProjectM.Network.User>(userEntity);
                        _cachedLocalCharacter = user.LocalCharacter._Entity;

                        if (_cachedLocalCharacter != Entity.Null)
                        {
                            Plugin.BepinLogger.LogInfo($"[AP] Resolved local character: {_cachedLocalCharacter.Index}");
                        }
                        else
                        {
                            Plugin.BepinLogger.LogDebug("[AP] User entity has no LocalCharacter");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Plugin.BepinLogger.LogWarning($"[AP] Failed to resolve local character: {ex.Message}");
                }

                return _cachedLocalCharacter;
            }
        }

        /// <summary>
        /// Resets cached entities when connecting to a new server or respawning.
        /// </summary>
        public static void ResetCache()
        {
            _cachedLocalUser = Entity.Null;
            _cachedLocalCharacter = Entity.Null;
        }
    }
}
