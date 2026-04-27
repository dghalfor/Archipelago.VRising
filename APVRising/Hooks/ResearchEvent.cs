using APVRising;
using APVRising.Utils;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Entities;

[HarmonyPatch]
public static class UnlockResearchSystemPatch
{
    [HarmonyPatch(typeof(UnlockResearchSystem), nameof(UnlockResearchSystem.HandleEvent))]
    [HarmonyPrefix]
    public static bool Prefix(
        UnlockResearchSystem __instance,
        UnlockResearchEvent unlockResearchEvent,
        FromCharacter fromCharacter,
        ref NetworkIdLookupMap networkIdToEntityMap,
        ref PrefabLookupMap prefabLookupMap,
        ref MapZoneCollection mapZoneCollection,
        EntityCommandBuffer commandBuffer)
    {
        var researchGuid = unlockResearchEvent.ResearchGUID;
        Plugin.BepinLogger.LogInfo($"[APV] HandleEvent intercepted: {DebugTool.GetPrefabName(researchGuid)}");

        // Resolve the station NetworkId to an entity
        if (networkIdToEntityMap.TryGetValue(unlockResearchEvent.Researchstation, out var stationEntity))
        {
            Plugin.BepinLogger.LogInfo($"[APV] Station entity: {stationEntity}");

            // Dump station buffers on first call to find shared memory
            var em = Plugin.EntityManager;
            var bufferTypes = em.GetComponentTypes(stationEntity);
            foreach (var b in bufferTypes)
                Plugin.BepinLogger.LogInfo($"[APV] Station buffer: {b}");
        }

        // TODO: Send Archipelago location check
        // Return false to block both UnlockProgression AND station memory write
        return false;
    }
}