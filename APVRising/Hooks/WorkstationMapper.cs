using APVRising;
using APVRising.Archipelago;
using APVRising.Utils;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppSystem;
using Il2CppSystem.Net;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using ProjectM.UI;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using static ProjectM.Network.SetSnapshotOnDestroyedEntitiesSystem;
using static ProjectM.ProgressionUtility;
using static VCF.Core.Basics.RoleCommands;

namespace APVRising.Hooks;

[HarmonyPatch]
public static unsafe class Workstation
{
    private static bool _isPatching = false;
    
    [HarmonyPatch(typeof(ResearchEntry), nameof(ResearchEntry.RefreshData))]
    [HarmonyPrefix]
    public static bool RefreshDataPrefix(
        ResearchEntry entry,
        ResearchEntry.Data data,
        ControllerType controllerType,
        GridSelectionGroup<ResearchEntry, ResearchEntry.Data> parent,
        bool isBloodAltar)
    {
        if (_isPatching) return true;

        if (data.Status == ResearchEntry.ResearchStatus.Insertable)
        {
            data.Status = ResearchEntry.ResearchStatus.Researchable;

            // Write onto the entry reference before original runs
            entry.UpdatedData = data;
        }

        return true; // let original run — it should now read Researchable from entry.UpdatedData
    }

    [HarmonyPatch(typeof(ResearchEntry), nameof(ResearchEntry.RefreshData))]
    [HarmonyPostfix]
    public static void RefreshDataPostfix(
        ResearchEntry entry,
        ResearchEntry.Data data,
        ControllerType controllerType,
        GridSelectionGroup<ResearchEntry, ResearchEntry.Data> parent,
        bool isBloodAltar)
    {
        //Plugin.BepinLogger.LogInfo($"Postfix fired: {data.EntryId} Status: {data.Status}");

        if (_isPatching) return;
        if (data.Status != ResearchEntry.ResearchStatus.Insertable) return;

        // Build a corrected copy with all fields
        ResearchEntry.Data corrected = new ResearchEntry.Data();
        corrected.EntryId = data.EntryId;
        corrected.OutSideStationResearch = data.OutSideStationResearch;
        corrected.IsSelected = data.IsSelected;
        corrected.IsHovered = data.IsHovered;
        corrected.IsTreeEntry = data.IsTreeEntry;
        corrected.IsNew = data.IsNew;
        corrected.Name = data.Name;
        corrected.Desc = data.Desc;
        corrected.ItemSprite = data.ItemSprite;
        corrected.PercentualProgress = data.PercentualProgress;
        corrected.ResearchDuration = data.ResearchDuration;
        corrected.Requirements = data.Requirements;
        corrected.Status = ResearchEntry.ResearchStatus.Researchable; // override
        corrected.Sorting = data.Sorting;
        corrected.NeverShowQuestionmarkResearch = data.NeverShowQuestionmarkResearch;
        corrected.TechCategory = data.TechCategory;
        corrected.QueueSpot = data.QueueSpot;
        corrected.TechRequirements = data.TechRequirements;

        _isPatching = true;
        ResearchEntry.RefreshData(entry, corrected, controllerType, parent, isBloodAltar);
        _isPatching = false;
        //Plugin.BepinLogger.LogInfo($"Postfix complete, entry.UpdatedData.Status: {entry.UpdatedData.Status}");
        entry.ItemBackground.sprite = entry.BackgroundSprite_Normal;

    }

    [HarmonyPatch(typeof(ProgressionUtility), nameof(ProgressionUtility.HasUnlockedProgression),
    typeof(EntityManager), typeof(Entity), typeof(PrefabGUID))]
    [HarmonyPrefix]
    public static bool HasUnlockedProgression1(EntityManager entityManager, Entity progressionEntity, PrefabGUID progression)
    {
        if (!ProgressionHandler.isStale) return true;
        ProgressionHandler.SwitchProgression(entityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity), entityManager.GetBuffer<UnlockedSpellBookAbility>(progressionEntity));
        ProgressionHandler.SwitchRecipe(entityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity));
        ProgressionHandler.isStale = false;
        return true;
    }

    [HarmonyPatch(typeof(ProgressionUtility), nameof(ProgressionUtility.HasUnlockedProgression),
        typeof(EntityManager), typeof(bool), typeof(Entity), typeof(PrefabGUID))]
    [HarmonyPrefix]
    public static bool HasUnlockedProgression3(EntityManager entityManager, bool skipProgressionCheck, Entity progressionEntity, PrefabGUID progression)
    {
        if (!ProgressionHandler.isStale) return true;
        ProgressionHandler.SwitchProgression(entityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity), entityManager.GetBuffer<UnlockedSpellBookAbility>(progressionEntity));
        ProgressionHandler.SwitchRecipe(entityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity));
        ProgressionHandler.isStale = false;
        return true;
    }

    [HarmonyPatch(typeof(ProgressionUtility), nameof(ProgressionUtility.HasUnlockedProgressionOrDefault),
        typeof(EntityManager), typeof(Entity), typeof(PrefabGUID), typeof(bool))]
    [HarmonyPrefix]
    public static bool HasUnlockedProgressionOrDefault(EntityManager entityManager, Entity progressionEntity, PrefabGUID progression, bool resultIfProgressionGuidDefault)
    {
        if (!ProgressionHandler.isStale) return true;
        ProgressionHandler.SwitchProgression(entityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity), entityManager.GetBuffer<UnlockedSpellBookAbility>(progressionEntity));
        ProgressionHandler.SwitchRecipe(entityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity));
        ProgressionHandler.isStale = false;
        return true;
    }
}
