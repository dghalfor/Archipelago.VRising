using HarmonyLib;
using ProjectM;
using Unity.Entities;
using Unity.Collections;
using Stunlock.Core;
using APVRising.Utils;

namespace APVRising.Patches
{
    [HarmonyPatch(typeof(VBloodSystem), nameof(VBloodSystem.UnlockProgression))]
    public static class VBloodSystem_UnlockProgression_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            EntityManager entityManager,
            ProgressionUtility.UpdateUnlockedJobData progressionJobData,
            PrefabGUID vBloodUnit,
            Entity userEntity,
            EntityCommandBuffer commandBuffer,
            PrefabLookupMap prefabMapping,
            Entity progressionEntity,
            bool logOnDuplicate)
        {
            // Your sync logic goes here — e.g. GrantResearchDeskView-style handling.
            // Return false to block the native UnlockProgression body entirely.
            Plugin.BepinLogger.LogInfo($"VBloodSystem.UnlockProgression called for {DebugTool.GetPrefabName(vBloodUnit)}");
            return true;
        }
    }
}