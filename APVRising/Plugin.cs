using APVRising.Archipelago;
using APVRising.Utils;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProjectM;
using ProjectM.Scripting;
using ProjectM.UI;
using Stunlock.Core;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using VampireCommandFramework;

namespace APVRising;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("gg.deca.VampireCommandFramework")]
//[BepInDependency("gg.deca.Bloodstone")]
public class Plugin : BasePlugin
{
    public const string PluginGUID = "APVRising";
    public const string PluginName = "Archipelago";
    public const string PluginVersion = "0.0.1";

    public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
    private const string APDisplayInfo = $"Archipelago v{ArchipelagoClient.APVersion}";
    public static ManualLogSource BepinLogger;
    public static ArchipelagoClient ArchipelagoClient;
    Harmony _harmony;
    private static World _serverWorld;

    public static bool IsServer => Application.productName == "VRisingServer";

    public override void Load()
    {
        // Plugin startup logic
        BepinLogger = Log;
        ArchipelagoClient = new ArchipelagoClient();
        ArchipelagoConsole.Awake();

        // Harmony patching
        _harmony = new Harmony(PluginGUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        var patched = _harmony.GetPatchedMethods().ToList();
        BepinLogger.LogInfo($"[Harmony] Total patched methods: {patched.Count}");
        foreach (var m in patched)
            BepinLogger.LogInfo($"[Harmony] Patched -> {m.DeclaringType?.Name}.{m.Name}");

        if (IsServer)
        {
            ArchipelagoClient.Instance = new ArchipelagoClient();

            // Register all commands in the assembly with VCF
            CommandRegistry.RegisterAll();

            ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");
        }
        Plugin.BepinLogger.LogInfo("Do I even exist");
        Plugin.BepinLogger.LogInfo($"Is this the server? {Application.productName}");
    }

    public override bool Unload()
    {
        Plugin.BepinLogger.LogInfo("Unload");

        CommandRegistry.UnregisterAssembly();
        _harmony?.UnpatchSelf();
        return true;
    }

    public static EntityManager EntityManager => Server.EntityManager;
    public static PrefabCollectionSystem PrefabCollectionSystem => Server.GetExistingSystemManaged<PrefabCollectionSystem>();
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
			if (_serverWorld != null) return _serverWorld;

			_serverWorld = GetWorld("Server")
				?? throw new System.Exception("There is no Server world (yet). Did you install a server mod on the client?");
			return _serverWorld;
		}
	}

	private static World GetWorld(string name)
	{
		foreach (var world in World.s_AllWorlds)
		{
			if (world.Name == name)
			{
				_serverWorld = world;
				return world;
			}
		}

		return null;
	}
    public static readonly Dictionary<int, PrefabGUID> ResearchToRecipeMap = new();

    public static void BuildResearchToRecipeMapping()
    {
        ResearchToRecipeMap.Clear();

        var em = Server.EntityManager;
        var prefabSystem = Server.GetExistingSystemManaged<PrefabCollectionSystem>();

        int mapped = 0;

        foreach (var kvp in prefabSystem._PrefabGuidToEntityMap)
        {
            var entity = kvp.Value;
            if (entity.Index < 0) continue;

            try
            {
                // We want entities that have a research requirement
                if (!em.HasBuffer<RecipeRequirementBuffer>(entity)) continue;

                var requirements = em.GetBuffer<RecipeRequirementBuffer>(entity);
                foreach (var req in requirements)
                {
                    var reqName = DebugTool.GetPrefabName(req.Guid);
                    if (reqName.Contains("Tech_") || reqName.Contains("_T_"))
                    {
                        // req.Guid is the researchGuid, kvp.Key is the recipe/blueprint
                        ResearchToRecipeMap[req.Guid.GuidHash] = kvp.Key;
                        Plugin.BepinLogger.LogInfo($"[APV] {reqName} -> {DebugTool.GetPrefabName(kvp.Key)}");
                        mapped++;
                        break;
                    }
                }
            }
            catch { continue; }
        }

        Plugin.BepinLogger.LogInfo($"[APV] ResearchToRecipeMap built: {mapped} entries");
    }
    public static bool TryGetRecipe(PrefabGUID techGuid, out PrefabGUID recipeGuid)
        => ResearchToRecipeMap.TryGetValue(techGuid.GuidHash, out recipeGuid);
}
