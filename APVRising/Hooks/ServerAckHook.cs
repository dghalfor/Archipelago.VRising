using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using APVRising.Utils;
using VRisingArchipelago;

namespace APVRising.Hooks;

/// <summary>
/// Server-side receive for client -> server ##ACK#{version}## control messages, sent
/// via the native ChatMessageEvent (see AckSender.cs on the client). Mirrors the shape
/// of ChatMessage.ClientChatOnUpdatePostfix but for the server's ChatMessageSystem.
///
/// [HarmonyBefore] is kept even though CrimsonChatFilter is not in use, as insurance
/// against any future mod patching ChatMessageSystem.OnUpdate ahead of this one.
/// </summary>
[HarmonyPatch]
public static class ServerAckHook
{
    [HarmonyPatch(typeof(ChatMessageSystem), nameof(ChatMessageSystem.OnUpdate))]
    [HarmonyPrefix]
    [HarmonyBefore("CrimsonChatFilter")]
    public static void ServerAckPrefix(ChatMessageSystem __instance)
    {
        var em = __instance.EntityManager;
        var entities = __instance.EntityQueries[0].ToEntityArray(Allocator.Temp);
        var chatMessageEvents = __instance.EntityQueries[0].ToComponentDataArray<ChatMessageEvent>(Allocator.Temp);

        try
        {
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var chatMessageEvent = chatMessageEvents[i];
                string message = chatMessageEvent.MessageText.ToString();

                if (message.StartsWith("##ACK#"))
                {
                    var fromCharacter = em.GetComponentData<FromCharacter>(entity);
                    var user = em.GetComponentData<User>(fromCharacter.User);
                    ulong steamId = user.PlatformId;
                    string seqStr = message.Replace("##ACK#", "").Replace("##", "");

                    if (int.TryParse(seqStr, out int version))
                    {
                        Plugin.BepinLogger.LogInfo($"[AP] ACK received from {steamId}: v{version}");
                        AckTracker.Acknowledge(steamId, version);
                    }
                    else
                    {
                        Plugin.BepinLogger.LogError($"[AP] Malformed ACK: {message}");
                    }

                    em.DestroyEntity(entity);
                }
            }
        }
        finally
        {
            entities.Dispose();
            chatMessageEvents.Dispose();
        }
    }
}
