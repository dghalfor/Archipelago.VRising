using APVRising.Archipelago;
using APVRising.Hooks;
using APVRising.Utils;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using ProjectM;
using ProjectM.Scripting;
using ProjectM.UI;
using Stunlock.Core;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using VampireCommandFramework;
using VRisingArchipelago;

namespace APVRising;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("gg.deca.VampireCommandFramework")]
public class Plugin : BasePlugin
{
    public const string PluginGUID = "APVRising";
    public const string PluginName = "Archipelago";
    public const string PluginVersion = "0.0.1";

    public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
    private const string APDisplayInfo = $"Archipelago v{ArchipelagoClient.APVersion}";
    public static ManualLogSource BepinLogger;
    public static ArchipelagoClient APClient;
    Harmony _harmony;
    private static World _serverWorld;
    private static World _clientWorld;

    public static bool IsServer => Application.productName == "VRisingServer";

    public override void Load()
    {
        // Plugin startup logic
        BepinLogger = Log;
        APClient = new ArchipelagoClient();
        ArchipelagoConsole.Awake();
        
        // Harmony patching
        _harmony = new Harmony(PluginGUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        var patched = _harmony.GetPatchedMethods().ToList();
        BepinLogger.LogInfo($"[Harmony] Total patched methods: {patched.Count}");

        //UpdateUnlockedBuffersHook.Initialize();
        foreach (var m in patched)
            BepinLogger.LogInfo($"[Harmony] Patched -> {m.DeclaringType?.Name}.{m.Name}");
        CommandRegistry.RegisterAll();

        if (IsServer)
        {
            // Register all commands in the assembly with VCF
            ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");
        }
        ClassInjector.RegisterTypeInIl2Cpp<DeferredActionSystem>();


    }

    public override bool Unload()
    {
        Plugin.BepinLogger.LogInfo("Unload");
        _serverWorld = null;
        _clientWorld = null;
        CommandRegistry.UnregisterAssembly();
        _harmony?.UnpatchSelf();
        return true;
    }

    public static EntityManager EntityManager => Server.EntityManager;

    public static EntityManager ClientEntityManager => Client.EntityManager;
    public static PrefabCollectionSystem PrefabCollectionSystem => Server.GetExistingSystemManaged<PrefabCollectionSystem>();
    public static PrefabCollectionSystem ClientCollectionSystem => Client.GetExistingSystemManaged<PrefabCollectionSystem>();
    public static GameDataSystem GameDataSystem => Server.GetExistingSystemManaged<GameDataSystem>();
    public static ManagedDataRegistry ManagedDataRegistry => GameDataSystem.ManagedDataRegistry;
    public static DebugEventsSystem DebugEventsSystem => Server.GetExistingSystemManaged<DebugEventsSystem>();
    public static UnitSpawnerUpdateSystem UnitSpawnerUpdateSystem => Server.GetExistingSystemManaged<UnitSpawnerUpdateSystem>();
    public static ServerScriptMapper ServerScriptMapper => Server.GetExistingSystemManaged<ServerScriptMapper>();

    /// <summary>
    /// Return the Unity ECS World instance used on the server build of VRising.
    /// </summary>
    public static World Server
    {
        get
        {
            if (_serverWorld != null && _serverWorld.IsCreated)
                return _serverWorld;

            _serverWorld = GetWorld("Server")
                ?? throw new System.Exception("There is no Server world (yet). Did you install a server mod on the client?");
            return _serverWorld;
        }
    }

    public static World Client
    {
        get
        {
            if (_clientWorld != null && _clientWorld.IsCreated)
                return _clientWorld;

            _clientWorld = GetClientWorld("Client_0")
                ?? throw new System.Exception("There is no Client world (yet). Did you install a client mod on the server?");
            return _clientWorld;
        }
    }

    private static World GetWorld(string name)
    {
        foreach (var world in World.s_AllWorlds)
        {
            Plugin.BepinLogger.LogInfo($"Found world: {world.Name}");
            if (world.Name == name)
            {
                _serverWorld = world;
                return world;
            }
        }

        return null;
    }

    private static World GetClientWorld(string name)
    {
        foreach (var world in World.s_AllWorlds)
        {
            Plugin.BepinLogger.LogInfo($"Found world: {world.Name}");
            if (world.Name == name)
            {
                _clientWorld = world;
                return world;
            }
        }

        return null;
    }

}