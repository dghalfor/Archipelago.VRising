using APVRising;
using APVRising.Archipelago;
using APVRising.Utils;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using ProjectM;
using ProjectM.CastleBuilding;
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
        Plugin.EntityManager.GetBuffer<TechUnlockRecipeBuffer>(researchGuid);
        return true;
    }*/
    static List<TechUnlockRecipeBuffer> _savedRecipeBuffer;
    static Entity _savedResearchEntity;

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
        Plugin.BepinLogger.LogInfo($"[AP] UnlockProgression: {DebugTool.GetPrefabName(researchGuid)}");

        _savedResearchEntity = Entity.Null;

        if (!prefabMapping.TryGetValue(researchGuid, out Entity researchEntity))
        {
            Plugin.BepinLogger.LogWarning($"[AP] Could not find entity for PrefabGUID {researchGuid._Value}");
            return true;
        }

        if (!entityManager.HasBuffer<TechUnlockRecipeBuffer>(researchEntity))
            return true;

        DynamicBuffer<TechUnlockRecipeBuffer> buffer = progressionJobData.RecipesLookup[researchEntity];
        _savedRecipeBuffer = new List<TechUnlockRecipeBuffer>();
        for (int i = 0; i < buffer.Length; i++)
            _savedRecipeBuffer.Add(buffer[i]);

        buffer.Clear();
        Plugin.BepinLogger.LogInfo($"[AP] Cleared {_savedRecipeBuffer.Count} recipes, buffer now has {buffer.Length} entries");
        _savedResearchEntity = researchEntity;

        return true;
    }

    [HarmonyPatch(typeof(UnlockResearchSystem), nameof(UnlockResearchSystem.UnlockProgression))]
    [HarmonyPostfix]
    public static void Postfix(EntityManager entityManager)
    {
        if (_savedResearchEntity == Entity.Null || _savedRecipeBuffer == null)
            return;

        DynamicBuffer<TechUnlockRecipeBuffer> buffer = entityManager.GetBuffer<TechUnlockRecipeBuffer>(_savedResearchEntity);
        buffer.Clear();
        foreach (var element in _savedRecipeBuffer)
            buffer.Add(element);

        _savedRecipeBuffer = null;
        _savedResearchEntity = Entity.Null;

        Plugin.BepinLogger.LogInfo($"[AP] Restored TechUnlockRecipeBuffer");
    }
}