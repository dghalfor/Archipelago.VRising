using APVRising.Archipelago;
using ProjectM.Network;
using ProjectM;
using Unity.Entities;
using Unity.Collections;
using HarmonyLib;
using BepInEx.Logging;
using APVRising;

namespace APVRising.Hooks;
/*
[HarmonyPatch]
public static class ProgressionUpdate
{
    /// majority of this code adapted from VampireCommandFramework @ VCF.Core/Breadstone/ChatHook.cs
    [HarmonyPatch(typeof(ProgressionUtility), nameof(ProgressionUtility.UpdateUnlockedBuffers))]
	public static void Prefix(ProgressionUtility __instance)
	{
        Plugin.BepinLogger.LogMessage("test ProgUpdate");
    }
}

*/