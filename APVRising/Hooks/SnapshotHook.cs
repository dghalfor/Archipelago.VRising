using HarmonyLib;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace APVRising.Hooks;
/*
[HarmonyPatch]
public static class CopyBufferPatch
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(
            typeof(SetSnapshotOnDestroyedEntitiesSystem.CopyDataToDestroyedEntitiesJob),
            "CopyBuffer"
        ).MakeGenericMethod(typeof(Snapshot_ResearchBuffer));
    }

    [HarmonyPrefix]
    public static bool Prefix(
        SetSnapshotOnDestroyedEntitiesSystem.CopyDataToDestroyedEntitiesJob __instance,
        Entity sourceEntity,
        Entity targetEntity,
        BufferLookup<Snapshot_ResearchBuffer> getBuffer
    )
    {
        Plugin.BepinLogger.LogInfo("COPYBUFFER");
        return true;
    }
}*/