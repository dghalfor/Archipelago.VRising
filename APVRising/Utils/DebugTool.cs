using APVRising;
using BepInEx.Logging;
using ProjectM;
using Stunlock.Core;
using Stunlock.Localization;
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace APVRising.Utils;

//Shamelessly stolen from XPRising, big shout outs
public static class DebugTool
{
    private static string MaybeAddSpace(string input)
    {
        return input.Length > 0 ? input.TrimEnd() + " " : input;
    }

    private static string DebugEntity(Entity entity)
    {
        return Plugin.Server.EntityManager.Debug.GetEntityInfo(entity);
    }

    public static string DumpEntity(Entity entity, bool fullDump = true)
    {
        var sb = new Il2CppSystem.Text.StringBuilder();
        ProjectM.EntityDebuggingUtility.DumpEntity(Plugin.Server, entity, fullDump, sb);
        return sb.ToString();
    }
    public static string DumpClientEntity(Entity entity, bool fullDump = true)
    {
        var sb = new Il2CppSystem.Text.StringBuilder();
        ProjectM.EntityDebuggingUtility.DumpEntity(Plugin.Client, entity, fullDump, sb);
        return sb.ToString();
    }
    /// <summary>
    /// Logs prefab name and guid hash (and returns the PrefabGUID)
    /// </summary>
    public static PrefabGUID GetAndLogPrefabGuid(Entity entity, string logPrefix = "", bool forceLog = false)
    {
        var guid = Helper.GetPrefabGUID(entity);
        LogPrefabGuid(guid, logPrefix, forceLog);
        return guid;
    }

    /// <summary>
    /// Logs prefab name and guid hash
    /// </summary>
    public static void LogPrefabGuid(PrefabGUID guid, string logPrefix = "", bool forceLog = false)
    {
        Plugin.BepinLogger.LogDebug($"{MaybeAddSpace(logPrefix)}Prefab: {GetPrefabName(guid)} ({guid.GuidHash})");
    }

    /// <summary>
    /// Logs entity and prefab name
    /// </summary>
    public static void LogEntity(
        Entity entity,
        string logPrefix = "",
        bool forceLog = false)
    {
        Plugin.BepinLogger.LogInfo($"{MaybeAddSpace(logPrefix)}{entity} - {GetPrefabName(entity)}");
    }

    /// <summary>
    /// Logs all the components on an entity
    /// </summary>
    public static void LogDebugEntity(
        Entity entity,
        string logPrefix = "",
        bool forceLog = false)
    {
        Plugin.BepinLogger.LogInfo($"{MaybeAddSpace(logPrefix)}Entity: {entity} ({DebugEntity(entity)})");
    }

    /// <summary>
    /// Logs all the components on an entity and their values
    /// </summary>
    public static void LogFullEntityDebugInfo(Entity entity, string logPrefix = "", bool forceLog = false)
    {
        Plugin.BepinLogger.LogInfo($"{MaybeAddSpace(logPrefix)}Debug entity: {entity}\n{DumpEntity(entity)}");
    }

    private static IEnumerable<string> BufferToEnumerable<T>(DynamicBuffer<T> buffer, Func<T, string> valueToString, string logPrefix = "")
    {
        for (var i = 0; i < buffer.Length; i++)
        {
            var data = buffer[i];
            yield return $"{MaybeAddSpace(logPrefix)}B[{i}]: {valueToString(data)}";
        }
    }

    public static void LogStatsBuffer(
        DynamicBuffer<ModifyUnitStatBuff_DOTS> buffer,
        string logPrefix = "",
        bool forceLog = false)
    {
        Func<ModifyUnitStatBuff_DOTS, string> printStats = (data) =>
            $"{data.StatType} {data.Value} {data.ModificationType} {data.Id.Id} {data.Priority} {data.ValueByStacks} {data.IncreaseByStacks}";
        Plugin.BepinLogger.LogInfo(BufferToEnumerable(buffer, printStats, logPrefix));
    }

    public static void LogBuffBuffer(
        DynamicBuffer<BuffBuffer> buffer,
        string logPrefix = "",
        bool forceLog = false)
    {
        Func<BuffBuffer, string> printStats = (data) =>
            $"Prefab: {GetPrefabName(data.PrefabGuid)}\nDebug BuffBuffer:{DumpEntity(data.Entity, false)}";
        Plugin.BepinLogger.LogInfo(BufferToEnumerable(buffer, printStats, logPrefix));
    }

    public static void LogBufferContents(EntityManager em, Entity entity)
    {
        if (!em.HasBuffer<CreateGameplayEventsOnSpawn>(entity)) return;
    
        var buffer = em.GetBuffer<CreateGameplayEventsOnSpawn>(entity);
    
        // Get the prefab collection to resolve event IDs to names
        var prefabCollectionSystem = em.World.GetExistingSystemManaged<PrefabCollectionSystem>();
    
        foreach (var element in buffer)
        {
            var eventGuid = new PrefabGUID(element.EventId.EventId);
            string eventName = "unknown";
        
            if (prefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(eventGuid, out var eventEntity))
            {
                eventName = DebugTool.GetPrefabName(eventEntity);
                Plugin.BepinLogger.LogInfo($"[AP] Unlock: {eventName}");
            }
        
            Plugin.BepinLogger.LogInfo($"[AP] Event {element.EventId.EventId} -> {eventName}");
        }
    }

    public static string GetPrefabName(PrefabGUID hashCode)
    {
        PrefabCollectionSystem s;
        if (Plugin.IsServer)
        {
            s = Plugin.PrefabCollectionSystem;
        }
        else
        {
            s = Plugin.ClientCollectionSystem;
        }
        //var s = Plugin.Server.GetExistingSystemManaged<PrefabCollectionSystem>();
        string name = "Nonexistent";
        if (hashCode.GuidHash == 0)
        {
            return name;
        }
        try
        {
            name = s._PrefabLookupMap.GetName(hashCode);
        }
        catch
        {
            name = "NoPrefabName";
        }
        return name;
    }

    public static string GetPrefabName(Entity entity)
    {
        return GetPrefabName(Helper.GetPrefabGUID(entity));
    }
}