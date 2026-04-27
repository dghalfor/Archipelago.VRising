using HarmonyLib;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APVRising.Hooks;

[HarmonyPatch]
public static class GameDataManagerPatch
{
    private static bool _initialized = false;

    [HarmonyPatch(typeof(GameDataManager), nameof(GameDataManager.OnUpdate))]
    [HarmonyPostfix]
    public static void Postfix(GameDataManager __instance)
    {
        if (_initialized) return;
        if (!__instance.GameDataInitialized) return;

        _initialized = true;

        Plugin.BepinLogger.LogInfo("Game data initialized, building mappings...");

        try
        {
            Plugin.BuildResearchToRecipeMapping();
        }
        catch (System.Exception ex)
        {
            Plugin.BepinLogger.LogError($"Failed to build mappings: {ex.Message}");
        }
    }
}