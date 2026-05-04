using APVRising.Utils;
using HarmonyLib;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using ProjectM;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace APVRising.Hooks;
[HarmonyPatch]
internal class BuildMenuHook
{
    /*
    // Discover fix
    [HarmonyPatch(typeof(BuildMenuAvailability), nameof(BuildMenuAvailability.IsAvailable))]
    [HarmonyPrefix]
    public static void Prefix(EntityManager entityManager, Entity characterEntity, PrefabGUID progressionRequirement)
    {
        Plugin.BepinLogger.LogInfo($"BuildMenuAvailability.IsAvailable: {DebugTool.GetPrefabName(progressionRequirement)}");

    }
    */
}