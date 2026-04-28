using APVRising;
using APVRising.Archipelago;
using APVRising.Utils;
using BepInEx.Logging;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using static ProjectM.ProgressionUtility;
using static VCF.Core.Basics.RoleCommands;

namespace APVRising.Hooks;

[HarmonyPatch]
public static class UnlockResearch
{
	/*
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
			if (networkIdToEntityMap.TryGetValue(unlockResearchEvent.Researchstation, out var stationEntity)) {
				_lastResearchStation = stationEntity;
			} else {
				_lastResearchStation = Entity.Null;
			}
			return true;
		}
	

	public static Entity _lastResearchStation = default;
	*/
	// majority of this code adapted from VampireCommandFramework @ VCF.Core/Breadstone/ChatHook.cs
	[HarmonyPatch(typeof(UnlockResearchSystem), nameof(UnlockResearchSystem.UnlockProgression))]
    [HarmonyPrefix]
    public static bool Prefix(
    EntityManager entityManager,
    UpdateUnlockedJobData progressionJobData,
    PrefabGUID researchGuid,
    Entity user,
    EntityCommandBuffer commandBuffer,
    PrefabLookupMap prefabMapping,
    Entity progressionEntity,
    bool logOnDuplicate = true)
    {
        var name = DebugTool.GetPrefabName(researchGuid);
        Plugin.BepinLogger.LogInfo($"[AP] UnlockProgression: {DebugTool.GetPrefabName(researchGuid)}");
        return true;
    }
	/*
	[HarmonyPatch(typeof(UnlockResearchSystem), nameof(UnlockResearchSystem.UnlockProgression))]
	[HarmonyPostfix]
	public static void Postfix(
		EntityManager entityManager,
		UpdateUnlockedJobData progressionJobData,
		PrefabGUID researchGuid,
		Entity user,
		EntityCommandBuffer commandBuffer,
		PrefabLookupMap prefabMapping,
		Entity progressionEntity,
		bool logOnDuplicate = true)
	{
		// We don't have direct access to the station entity here,
		// but we stored it from the Prefix

		if (_lastResearchStation == Entity.Null) return;

		var em = Plugin.EntityManager;
		if (!em.HasBuffer<ResearchBuffer>(_lastResearchStation)) return;

		var stationBuffer = em.GetBuffer<ResearchBuffer>(_lastResearchStation);
		for (int i = stationBuffer.Length - 1; i >= 0; i--)
		{

			if (stationBuffer[i].ResearchGuid == researchGuid)
			{
				stationBuffer.RemoveAt(i);
				Plugin.BepinLogger.LogInfo($"[APV] Removed {DebugTool.GetPrefabName(researchGuid)} from station buffer");
				break;
			}
		}
		_lastResearchStation = Entity.Null;
	}*/
}