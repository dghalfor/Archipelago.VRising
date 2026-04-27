using APRising.Models;
using ProjectM;
using Unity.Collections;
using Unity.Entities;

namespace APVRising.Utils;

public static class Cache
{
    //-- Cache (Wiped on plugin reload, server restart, and shutdown.)

    //-- -- Player Cache
    public static readonly LazyDictionary<FixedString64Bytes, PlayerData> NamePlayerCache = new();
    public static readonly LazyDictionary<ulong, PlayerData> SteamPlayerCache = new();
    public static readonly LazyDictionary<ulong, bool> PlayerClientUICache = new();

    //-- -- Experience System
    public static LazyDictionary<ulong, float> player_level = new();

    // Buff data
    public static LazyDictionary<Entity, LazyDictionary<UnitStatType, float>> PlayerToStatBonuses = new();

    public static bool PlayerHasUINotifications(ulong steamID)
    {
        return PlayerClientUICache.TryGetValue(steamID, out var receivingUIMessages) && receivingUIMessages;
    }
}
