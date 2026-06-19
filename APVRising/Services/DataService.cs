using APVRising;
using Archipelago.MultiClient.Net.Helpers;
using ProjectM;
using ProjectM.Shared;
using Stunlock.Core;
using static APVRising.Services.DataService.PlayerDictionaries;
using static APVRising.Services.DataService.PlayerPersistence;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace APVRising.Services;

internal static class DataService
{
    static readonly object PersistenceSuppressionLock = new();
    static int persistenceSuppressionDepth;

    internal static IDisposable SuppressPersistence()
    {
        return new PersistenceSuppressionScope();
    }

    static bool IsPersistenceSuppressed
    {
        get
        {
            lock (PersistenceSuppressionLock)
            {
                return persistenceSuppressionDepth > 0;
            }
        }
    }

    sealed class PersistenceSuppressionScope : IDisposable
    {
        bool disposed;

        public PersistenceSuppressionScope()
        {
            lock (PersistenceSuppressionLock)
            {
                persistenceSuppressionDepth++;
            }
        }

        public void Dispose()
        {
            lock (PersistenceSuppressionLock)
            {
                if (disposed)
                {
                    return;
                }

                persistenceSuppressionDepth--;
                disposed = true;
            }
        }
    }

    static readonly Lazy<List<string>> _directoryPaths = new(() =>
    {
        return
        [
        Path.Combine(BepInEx.Paths.ConfigPath, Plugin.PluginName)
        ];
    });
    public static List<string> DirectoryPaths => _directoryPaths.Value;
    public record ArchipelagoConnectionData(string IP, string Password, string SlotName, string ServerIndex);
    public record PlayerItemReceivedData(List<long> Items);
    public static void SetArchipelagoData(ConcurrentDictionary<string, ArchipelagoConnectionData> data)
    {
        _ArchipelagoData = data;
        SaveArchipelagoData();
    }
    public static void SetPlayerItemReceivedData(ConcurrentDictionary<string, PlayerItemReceivedData> data)
    {
        _PlayerItemReceivedData = data;
        SavePlayerItemReceivedData();
    }

    public static class PlayerDictionaries
    {
        public static ConcurrentDictionary<string, ArchipelagoConnectionData> _ArchipelagoData = [];
        public static ConcurrentDictionary<string, PlayerItemReceivedData> _PlayerItemReceivedData = [];
    }
    public static class PlayerPersistence
    {
        static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true
        };

        static readonly Dictionary<string, string> _filePaths = new()
        {
            {"Archipelago", JsonFilePaths.ArchipelagoJson},
            {"PlayerItemReceived", JsonFilePaths.PlayerItemReceivedJson}
        };
        public static class JsonFilePaths
        {
            public static readonly string ArchipelagoJson = Path.Combine(DirectoryPaths[0], "archipelagoData.json");
            public static readonly string PlayerItemReceivedJson = Path.Combine(DirectoryPaths[0], "playerItemReceivedData.json");
        }
        static void LoadData<T>(ref ConcurrentDictionary<string, T> dataStructure, string key)
        {
            string path = _filePaths[key];

            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                string json = File.ReadAllText(path);


                if (string.IsNullOrWhiteSpace(json))
                {
                    dataStructure = [];
                }
                else
                {
                    var data = JsonSerializer.Deserialize<ConcurrentDictionary<string, T>>(json, _jsonOptions);
                    dataStructure = data ?? [];
                }
            }
            catch (IOException ex)
            {
                Plugin.BepinLogger.LogWarning($"Failed to read {key} data from file: {ex.Message}");
            }
        }
        
        static void SaveData<T>(ConcurrentDictionary<string, T> data, string key)
        {
            if (IsPersistenceSuppressed)
            {
                return;
            }

            string path = _filePaths[key];
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!); 
                string json = JsonSerializer.Serialize(data, _jsonOptions);
                File.WriteAllText(path, json);
            }
            catch (IOException ex)
            {
                Plugin.BepinLogger.LogWarning($"Failed to write {key} data to file: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Plugin.BepinLogger.LogWarning($"JSON serialization error when saving {key} data: {ex.Message}");
            }
        }
       

        // load methods
        public static void LoadArchipelagoData() => LoadData(ref _ArchipelagoData, "Archipelago");
        public static void LoadPlayerItemReceivedData() => LoadData(ref _PlayerItemReceivedData, "PlayerItemReceived");

        // save methods
        public static void SaveArchipelagoData() => SaveData(_ArchipelagoData, "Archipelago");
        public static void SavePlayerItemReceivedData() => SaveData(_PlayerItemReceivedData, "PlayerItemReceived"); 
       
    }
}
