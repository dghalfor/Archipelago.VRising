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

      [HarmonyPatch(typeof(ProgressionUtility), nameof(ProgressionUtility.GatherUnlockedRecipes))]
      [HarmonyPrefix]
      public static bool GatherUnlockedRecipesPrefix(EntityManager entityManager,
          Entity progressionEntity,
          NativeParallelHashSet<PrefabGUID> unlockedRecipes)
      {

          //Plugin.BepinLogger.LogInfo("Workstation On Create Prefix");
          return true;
      }

    [HarmonyPatch(typeof(ResearchstationMenuMapper), nameof(ResearchstationMenuMapper.InitializeUI))]
    [HarmonyPrefix]
    public static bool UIPrefix()
    {
        Plugin.BepinLogger.LogInfo("Prefix On InitializeUi");
        //ProgressionHandler.ClearUnlockBuffers();
        //ProgressionHandler.CheckResearchStations();
        return true;
    }
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
    
    private static int _lastProgressionHash = 0;

    [HarmonyPatch(typeof(ProgressionUtility), nameof(ProgressionUtility.HasUnlockedProgression), typeof(EntityManager), typeof(Entity), typeof(PrefabGUID))]
    [HarmonyPrefix]
    public static bool HasUnlockedProgressionPrefix(EntityManager entityManager, Entity progressionEntity, PrefabGUID progression)
    {
        var buffer = entityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity);

        ProgressionHandler.SwitchProgression(buffer);

        return true;
    }
    /*
  [HarmonyPatch(typeof(ProgressionUtility), nameof(ProgressionUtility.HasUnlockedProgression), typeof(EntityManager), typeof(Entity), typeof(PrefabGUID))]
  [HarmonyPostfix]
  public static void HasUnlockedProgressionPostfix(EntityManager entityManager, Entity progressionEntity, PrefabGUID progression)
  {
      foreach (var recipe in entityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity))
      {
          Plugin.BepinLogger.LogInfo($"HasUnlockedProgression: {recipe.UnlockedRecipe}");
      }
      Plugin.BepinLogger.LogInfo("HasUnlockedProgression On Create Postfix");
  }
    /*
    [HarmonyPatch(typeof(ResearchstationSubMenuMapper), nameof(ResearchstationSubMenuMapper.OnUpdate))]
    [HarmonyPrefix]
    public static bool UpdatePrefix(ResearchstationSubMenuMapper __instance)
    {

        Plugin.BepinLogger.LogInfo("Workstation On Update Prefix");

        var keys = new System.Collections.Generic.List<TechCategory>();
        foreach (var kvp in __instance._ResearchDatas)
            keys.Add(kvp.Key);

        // Now iterate the keys safely
        foreach (TechCategory key in keys)
        {
            Il2CppSystem.Collections.Generic.List<ResearchEntry.Data> entries = __instance._ResearchDatas[key];

            unsafe
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    ResearchEntry entry = entries[i]; // get the ResearchEntry, not ResearchEntry.Data
                    if (entry == null) continue;

                    ResearchEntry.Data data = entry.UpdatedData; // boxes a copy
                    if (data.Status != ResearchEntry.ResearchStatus.Insertable) continue;

                    // Modify the copy
                    data.Status = ResearchEntry.ResearchStatus.Researchable;

                    // Write back via the setter which uses cpblk to copy into the field
                    entry.UpdatedData = data;

                    // Verify
                    Plugin.BepinLogger.LogInfo($"After: {entry.UpdatedData.Status}");
                }
            }

        }
        foreach (TechCategory key in keys)
        {
            for (int i = 0; i < __instance._ResearchDatas[key].Count; i++)
            {
                ResearchEntry.Data entry = __instance._ResearchDatas[key][i];
                if (entry == null) continue;
                Plugin.BepinLogger.LogInfo($"Post: [{__instance._ResearchDatas[key][i].EntryId}] Status: {__instance._ResearchDatas[key][i].Status}");
            }
        }
            return true;
    }
    private static System.IntPtr _statusFieldPtr = System.IntPtr.Zero;

    static System.IntPtr GetStatusFieldPtr()
    {
        if (_statusFieldPtr != System.IntPtr.Zero) return _statusFieldPtr;

        var field = typeof(ResearchEntry.Data)
            .GetField("NativeFieldInfoPtr_Status",
                      System.Reflection.BindingFlags.NonPublic |
                      System.Reflection.BindingFlags.Static);

        _statusFieldPtr = (System.IntPtr)field.GetValue(null);
        return _statusFieldPtr;
    }
    /*
    [HarmonyPatch(typeof(ResearchstationSubMenuMapper), nameof(ResearchstationSubMenuMapper.OnUpdate))]
    [HarmonyPrefix]
    public static bool UpdatePrefix(ResearchstationSubMenuMapper __instance)
    {
        Plugin.BepinLogger.LogInfo("Workstation On Update Prefix");
        __instance.Force
        if (__instance._UnlockedRecipes.Count() != _UnlockedRecipes.Count())
        {
            Plugin.BepinLogger.LogInfo("Workstation On Update Prefix");

            foreach (var recipe in _UnlockedRecipes)
            {
                if (!_UnlockedRecipes.Contains(recipe))
                    Plugin.BepinLogger.LogInfo($"[Update] Unlocked recipe: {recipe}");
            }
            _UnlockedRecipes = __instance._UnlockedRecipes;
        }
        return true;
    }
    [HarmonyPatch(typeof(EventHelper), nameof(EventHelper.TryShareRefinement))]
    [HarmonyPrefix]
    public static bool shareRefinementPrefix(EntityManager entityManager, Entity target)
    {
        /*
        Plugin.BepinLogger.LogInfo("ShareRefinement");
        Plugin.BepinLogger.LogInfo(entityManager.Debug.GetEntityInfo(target));
        Plugin.BepinLogger.LogInfo(entityManager.GetComponentData<HaveUnlocksInStation>(target).CanUnlock.ToString());
        if (!entityManager.HasBuffer<Snapshot_ResearchBuffer>(target))
        {
            Plugin.BepinLogger.LogWarning("No Snapshot_ResearchBuffer on target");
            return true;
        }
        var attachedBuffer = entityManager.GetBuffer<AttachedBuffer>(target);
        Plugin.BepinLogger.LogInfo($"Attached buffers:");
        foreach (var bufferElement in attachedBuffer)
        {
            Plugin.BepinLogger.LogInfo(bufferElement.ToString());
        }
        var buffer = entityManager.GetBuffer<Snapshot_ResearchBuffer>(target, isReadOnly: true);

        if (!Snapshot_ResearchBuffer.TryGetSerializedSnapshot(buffer, readOnly: true, out Snapshot_ResearchBuffer.BufferSnapshotPtr snapshotPtr))
        {
            Plugin.BepinLogger.LogWarning("TryGetSerializedSnapshot failed");
            return true;
        }

        if (snapshotPtr.Elements == null || snapshotPtr.Length == 0)
        {
            Plugin.BepinLogger.LogWarning("Snapshot has no elements");
            return true;
        }

        unsafe
        {
            for (int j = 0; j < snapshotPtr.Length; j++)
            {
                Snapshot_ResearchBuffer_Data data = snapshotPtr.Elements[j];
                Plugin.BepinLogger.LogInfo($"ResearchBuffer entry {data.ResearchGuid}: {data.IsResearchByStation}");
            }
        }
        return true;
    }
    /*
    [HarmonyPatch(typeof(ShareRefinementSystem), nameof(ShareRefinementSystem.ShareResearchJob_Execute))]
    [HarmonyPrefix]
    public static bool shareResearch()
    {
        Plugin.BepinLogger.LogInfo("shareResearch");
        //ProgressionHandler.ClearUnlockBuffers();
        //ProgressionHandler.CheckResearchStations();
        return true;
    }

    /*
    [HarmonyPatch(typeof(SetSnapshotOnDestroyedEntitiesSystem), nameof(SetSnapshotOnDestroyedEntitiesSystem.OnCreate))]
    [HarmonyPrefix]
    public static bool snapshotPrefix()
    {
        Plugin.BepinLogger.LogInfo("Prefix On SetSnapshotOnDestroyedEntitiesSystem");
        //ProgressionHandler.ClearUnlockBuffers();
        //ProgressionHandler.CheckResearchStations();
        return true;
    }

    [HarmonyPatch(typeof(SetSnapshotOnDestroyedEntitiesSystem), nameof(SetSnapshotOnDestroyedEntitiesSystem.OnUpdate))]
    [HarmonyPrefix]
    public static bool updatesnapshotPrefix(SetSnapshotOnDestroyedEntitiesSystem __instance)
    {
        // Access the BufferLookup to check if buffer exists
        var bufferLookup = __instance.__TypeHandle.__ProjectM_Network_Snapshot_ResearchBuffer_RW_BufferLookup;
        Plugin.BepinLogger.LogInfo("Prefix OnUpdate SetSnapshotOnDestroyedEntitiesSystem");
        //ProgressionHandler.ClearUnlockBuffers();
        //ProgressionHandler.CheckResearchStations();
        return true;
    }

    [HarmonyPatch(typeof(SetSnapshotOnDestroyedEntitiesSystem), nameof(SetSnapshotOnDestroyedEntitiesSystem.SetupJob))]
    [HarmonyPrefix]
    public static bool setupJobsnapshotPrefix(SetSnapshotOnDestroyedEntitiesSystem __instance,
        ref CopyDataToDestroyedEntitiesJob.JobParams jobParams)
    {
        // Log the contents of Snapshot_ResearchBuffer
        var snapshotBuffer = jobParams.GetSnapshot_ResearchBuffer;
      
        Plugin.BepinLogger.LogInfo("Prefix OnUpdate SetUpJob");
        //ProgressionHandler.ClearUnlockBuffers();
        //ProgressionHandler.CheckResearchStations();
        return true;
    }

    
    [HarmonyPatch(typeof(EventHelper), nameof(EventHelper.TryShareRefinement))]
    [HarmonyPrefix]
    public static bool shareRefinementPrefix(EntityManager entityManager, Entity target)
    {
        Plugin.BepinLogger.LogInfo("ShareRefinement");
        //DebugTool.DumpClientEntity(target);
        //ProgressionHandler.ClearUnlockBuffers();
        //ProgressionHandler.CheckResearchStations();
        return true;
    }
    /*
        [HarmonyPatch(typeof(SetSnapshotOnDestroyedEntitiesSystem.CopyDataToDestroyedEntitiesJob), nameof(SetSnapshotOnDestroyedEntitiesSystem.CopyDataToDestroyedEntitiesJob.CopySnapshotData))]
    [HarmonyPrefix]
    public static bool copySnapshotDataPrefix(SetSnapshotOnDestroyedEntitiesSystem __instance)
    {
        Plugin.BepinLogger.LogInfo("COPYSNAPSHOTDATA");
        //ProgressionHandler.ClearUnlockBuffers();
        //ProgressionHandler.CheckResearchStations();
        return true;
    }
    [HarmonyPatch(typeof(ShareRefinementSystem), nameof(ShareRefinementSystem.ShareResearchJob_Execute))]
    [HarmonyPrefix]
    public static bool shareResearch()
    {
        Plugin.BepinLogger.LogInfo("shareResearch");
        //ProgressionHandler.ClearUnlockBuffers();
        //ProgressionHandler.CheckResearchStations();
        return true;
    }
    /*
    [HarmonyPatch(typeof(Snapshot_ResearchBuffer), nameof(Snapshot_ResearchBuffer.))]
    [HarmonyPrefix]
    public static bool researchstationMenuMapper()
    {
        Plugin.BepinLogger.LogInfo("Prefix On ResearchstationMenuMapper OnDestroy");
        //ProgressionHandler.ClearUnlockBuffers();
        //ProgressionHandler.CheckResearchStations();
        return true;
    }
    /*
    private static ComponentTypeHandle<ResearchStation> testStation;
    private static ComponentTypeHandle<HaveUnlocksInStation> testUnlocks;

    [HarmonyPatch(typeof(ActiveResearchstationSequenceSystem), nameof(ActiveResearchstationSequenceSystem.OnCreate))]
    [HarmonyPrefix]
    public static bool Prefix(ActiveResearchstationSequenceSystem __instance)
    {
        // Build a query for entities that have this component
        var query = Plugin.ClientEntityManager.CreateEntityQuery(ComponentType.ReadWrite<HaveUnlocksInStation>());

        // Get and update the handle

        var testUnlocks = Plugin.ClientEntityManager.GetComponentTypeHandle<HaveUnlocksInStation>(false);

        // Iterate chunks and zero out
        var chunks = query.ToArchetypeChunkArray(Allocator.Temp);
        foreach (var chunk in chunks)
        {
            var components = chunk.GetNativeArray(ref testUnlocks);
            for (int i = 0; i < components.Length; i++)
            {
                var comp = components[i];
                comp.CanUnlock = false;
                components[i] = comp;
            }
        }
        chunks.Dispose();
        //Plugin.BepinLogger.LogInfo("Workstation On Create Prefix");
        return true;
    }
    [HarmonyPatch(typeof(ActiveResearchstationSequenceSystem), nameof(ActiveResearchstationSequenceSystem.OnCreate))]
    [HarmonyPostfix]
    public static void Postfix(ActiveResearchstationSequenceSystem __instance)
    {
        testStation = __instance.__TypeHandle.__ProjectM_ResearchStation_RO_ComponentTypeHandle;
        testUnlocks = __instance.__TypeHandle.__ProjectM_HaveUnlocksInStation_RO_ComponentTypeHandle;
        Plugin.BepinLogger.LogInfo("Workstation On Create Postfix");
    }

    [HarmonyPatch(typeof(ActiveResearchstationSequenceSystem), nameof(ActiveResearchstationSequenceSystem.OnUpdate))]
    [HarmonyPrefix]
    public static bool UpdatePrefix(ActiveResearchstationSequenceSystem __instance)
    {
        // Build a query for entities that have this component
        var query = Plugin.ClientEntityManager.CreateEntityQuery(ComponentType.ReadWrite<HaveUnlocksInStation>());

        // Get and update the handle
        var testUnlocks = Plugin.ClientEntityManager.GetComponentTypeHandle<HaveUnlocksInStation>(false);

        // Iterate chunks and zero out
        var chunks = query.ToArchetypeChunkArray(Allocator.Temp);
        foreach (var chunk in chunks)
        {
            var components = chunk.GetNativeArray(ref testUnlocks);
            for (int i = 0; i < components.Length; i++)
            {
                Plugin.BepinLogger.LogInfo($"Zeroing out HaveUnlocksInStation component for entity in chunk. Original value: {components[i]}");
                var comp = components[i];
                comp.CanUnlock = false;
                components[i] = comp;
            }
        }
        chunks.Dispose();
       // Plugin.BepinLogger.LogInfo("Workstation On Update Prefix");
        return true;
    }
    [HarmonyPatch(typeof(ActiveResearchstationSequenceSystem), nameof(ActiveResearchstationSequenceSystem.OnUpdate))]
    [HarmonyPostfix]
    public static void UpdatePostfix(ActiveResearchstationSequenceSystem __instance)
    {
        var query = Plugin.ClientEntityManager.CreateEntityQuery(ComponentType.ReadWrite<HaveUnlocksInStation>());

        // Get and update the handle
        var testUnlocks = Plugin.ClientEntityManager.GetComponentTypeHandle<HaveUnlocksInStation>(false);

        // Iterate chunks and zero out
        var chunks = query.ToArchetypeChunkArray(Allocator.Temp);
        foreach (var chunk in chunks)
        {
            var components = chunk.GetNativeArray(ref testUnlocks);
            for (int i = 0; i < components.Length; i++)
            {
                var comp = components[i];
                comp.CanUnlock = false;
                components[i] = comp;
            }
        }
        chunks.Dispose();
        //Plugin.BepinLogger.LogInfo("Workstation On Update Postfix");
    }
    /*
      [HarmonyPatch(typeof(ResearchstationMenuMapper), nameof(ResearchstationMenuMapper.InitializeUI))]
      [HarmonyPostfix]
      public static bool Prefix()
      {
          try
          {
              Plugin.BepinLogger.LogInfo("Postfix On GetLocalUser");
              var em = Plugin.EntityManager; // Use Server EntityManager

              if (em == null)
              {
                  Plugin.BepinLogger.LogWarning("EntityManager is null, skipping patch");
                  return true;
              }

              var progQuery2 = em.CreateEntityQuery(ComponentType.ReadWrite<UnlockedProgressionElement>());
              if (progQuery2.IsEmpty) return true;
              Plugin.BepinLogger.LogInfo("Postfix On GetLocalUser 2");

              var progQuery = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
              if (progQuery.IsEmpty) return true;
              Plugin.BepinLogger.LogInfo("Postfix On GetLocalUser 2");

              var entities = progQuery.ToEntityArray(Allocator.Temp);
              foreach (var entity in entities)
              {
                  Plugin.BepinLogger.LogInfo("Postfix On GetLocalUser " + entity);
              }
              // Query for User entities which have ProgressionMapper
              var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<ProjectM.Network.User>(), ComponentType.ReadOnly<ProgressionMapper>());
              if (userQuery.IsEmpty) return true;

              var users = userQuery.ToEntityArray(Allocator.Temp);
              foreach (var userEntity in users)
              {
                  var query = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
                  if (query.IsEmpty) return true;

                  entities = query.ToEntityArray(Allocator.Temp);
                  foreach (var entity in entities)
                  {
                      //UnlockedRecipeElement, UnlockedBlueprintElement, UnlockedVBlood, (maybe) UnlockedSpellBookAbility
                      var buffer = em.GetBuffer<UnlockedProgressionElement>(entity);
                      var unlockedTechHashes = new List<int>();
                      //unlockedTechHashes.Add(507915220); - mace is not unlocked in research station
                      unlockedTechHashes.Add(-54738837);
                      unlockedTechHashes.Add(-2012042353);

                      // Sync tech unlocks with recipe unlocks directly on the buffer
                      TechToRecipeMapping.SyncUnlockedTechs(buffer, unlockedTechHashes);
                      var recipeBuffer = em.GetBuffer<UnlockedRecipeElement>(entity);

                      // TODO Read archipelago progression data and sync with game progression


                      // Sync tech unlocks with recipe unlocks directly on the buffer
                      TechToRecipeMapping.SyncTechRecipes(recipeBuffer, unlockedTechHashes);
                  }
                  entities.Dispose();
              }
              return true;
          }
          catch (Exception ex)
          {
              Plugin.BepinLogger.LogError($"Error in ResearchstationMenuMapper.InitializeUI patch: {ex}");
              return true;
          }
      }
    */
    /*
    [HarmonyPatch(typeof(ResearchstationMenuMapper), nameof(ResearchstationMenuMapper.OnDestroy))]
    [HarmonyPrefix]
    public static bool OnDestroyPrefix()
    {
        var em = Plugin.ClientEntityManager;

        // Query for User entities which have ProgressionMapper
        var userQuery = em.CreateEntityQuery(ComponentType.ReadOnly<ProjectM.Network.User>(), ComponentType.ReadOnly<ProgressionMapper>());
        if (userQuery.IsEmpty) return true;

        var users = userQuery.ToEntityArray(Allocator.Temp);
        foreach (var userEntity in users)
        {
            var query = em.CreateEntityQuery(ComponentType.ReadOnly<UnlockedProgressionElement>());
            if (query.IsEmpty) return true;

            var entities = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                //UnlockedRecipeElement, UnlockedBlueprintElement, UnlockedVBlood, (maybe) UnlockedSpellBookAbility
                var buffer = em.GetBuffer<UnlockedProgressionElement>(entity);
                var unlockedTechHashes = new List<int>();
                unlockedTechHashes.Add(507915220); //Mace is unlocked in crafting
                unlockedTechHashes.Add(-54738837);
                unlockedTechHashes.Add(-2012042353);

                // Sync tech unlocks with recipe unlocks directly on the buffer
                TechToRecipeMapping.SyncUnlockedTechs(buffer, unlockedTechHashes);
                var recipeBuffer = em.GetBuffer<UnlockedRecipeElement>(entity);

                // TODO Read archipelago progression data and sync with game progression


                // Sync tech unlocks with recipe unlocks directly on the buffer
                TechToRecipeMapping.SyncTechRecipes(recipeBuffer, unlockedTechHashes);
            }
            entities.Dispose();
        }
        Plugin.BepinLogger.LogInfo("ResearchStation On Initialize Prefix");
        return true;
    }
    */
    /*
  [HarmonyPatch(typeof(ProgressionUtility), nameof(ProgressionUtility.GatherUnlockedRecipes))]
  [HarmonyPrefix]
  public static bool GatherUnlockedRecipesPrefix(EntityManager entityManager, 
      Entity progressionEntity, 
      NativeParallelHashSet<PrefabGUID> unlockedRecipes)
  {

      foreach (var recipe in entityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity))
      {
          Plugin.BepinLogger.LogInfo($"GatherUnlockedRecipes: {recipe.UnlockedRecipe}");
      }
      for (int i = entityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity).Length - 1; i >= 0; i--)
      {
          if (entityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity)[i].UnlockedPrefab == new PrefabGUID(507915220) || entityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity)[i].UnlockedPrefab == new PrefabGUID(1183771910))
          {
              entityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity).RemoveAt(i);
              break;
          }
      }

      for (int j = entityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity).Length - 1; j >= 0; j--)
      {
          if (entityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity)[j].UnlockedRecipe == new PrefabGUID(-2125590443) || entityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity)[j].UnlockedRecipe == new PrefabGUID(-897446828))
          {
              entityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity).RemoveAt(j);
              break;
          }
      }
      Plugin.BepinLogger.LogInfo("Workstation On Create Prefix");
      return true;
  }
  [HarmonyPatch(typeof(ProgressionUtility), nameof(ProgressionUtility.GatherUnlockedRecipes))]
  [HarmonyPostfix]
  public static void GatherUnlockedRecipesPostfix(EntityManager entityManager,
      Entity progressionEntity,
      NativeParallelHashSet<PrefabGUID> unlockedRecipes)
  {

      entityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity).Add(new UnlockedProgressionElement { UnlockedPrefab = new PrefabGUID(507915220) });
      entityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity).Add(new UnlockedProgressionElement { UnlockedPrefab = new PrefabGUID(1183771910) });
      Plugin.BepinLogger.LogInfo("Workstation On Create Postfix");
  }
  /*
  [HarmonyPatch(typeof(ProgressionUtility), nameof(ProgressionUtility.HasUnlockedProgression), typeof(EntityManager), typeof(Entity), typeof(PrefabGUID))]
  [HarmonyPrefix]
  public static bool HasUnlockedProgressionPrefix(EntityManager entityManager, Entity progressionEntity, PrefabGUID progression)
  {

      foreach (var recipe in entityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity))
      {
          Plugin.BepinLogger.LogInfo($"HasUnlockedProgression: {recipe.UnlockedRecipe}");
      }
      for (int i = entityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity).Length - 1; i >= 0; i--)
      {
          if (entityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity)[i].UnlockedPrefab == new PrefabGUID(507915220) || entityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity)[i].UnlockedPrefab == new PrefabGUID(-54738837) || entityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity)[i].UnlockedPrefab == new PrefabGUID(1183771910))
          {
              entityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity).RemoveAt(i);
              break;
          }
      }

      for (int j = entityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity).Length - 1; j >= 0; j--)
      {
          if (entityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity)[j].UnlockedRecipe == new PrefabGUID(-2125590443) || entityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity)[j].UnlockedRecipe == new PrefabGUID(-897446828))
          {
              entityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity).RemoveAt(j);
              break;
          }
      }
      Plugin.BepinLogger.LogInfo("HasUnlockedProgression On Create Prefix");
      return true;
  }
  [HarmonyPatch(typeof(ProgressionUtility), nameof(ProgressionUtility.HasUnlockedProgression), typeof(EntityManager), typeof(Entity), typeof(PrefabGUID))]
  [HarmonyPostfix]
  public static void HasUnlockedProgressionPostfix(EntityManager entityManager, Entity progressionEntity, PrefabGUID progression)
  {
      foreach (var recipe in entityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity))
      {
          Plugin.BepinLogger.LogInfo($"HasUnlockedProgression: {recipe.UnlockedRecipe}");
      }
      Plugin.BepinLogger.LogInfo("HasUnlockedProgression On Create Postfix");
  }
  /*
  [HarmonyPatch(typeof(StartCraftingSystem), nameof(StartCraftingSystem.OnCreate))]
  [HarmonyPrefix]
  public static bool Prefix(StartCraftingSystem __instance)
  {
      Plugin.BepinLogger.LogInfo("Workstation On Create Prefix");
      return true;
  }
  [HarmonyPatch(typeof(StartCraftingSystem), nameof(StartCraftingSystem.OnCreate))]
  [HarmonyPostfix]
  public static void Postfix(StartCraftingSystem __instance)
  {
      Plugin.BepinLogger.LogInfo("Workstation On Create Postfix");
  }

  [HarmonyPatch(typeof(StartCraftingSystem), nameof(StartCraftingSystem.OnUpdate))]
  [HarmonyPrefix]
  public static bool UpdatePrefix(StartCraftingSystem __instance)
  {

      Plugin.BepinLogger.LogInfo("Workstation On Update Prefix");
      return true;
  }
  [HarmonyPatch(typeof(StartCraftingSystem), nameof(StartCraftingSystem.OnUpdate))]
  [HarmonyPostfix]
  public static void UpdatePostfix(StartCraftingSystem __instance)
  {
      Plugin.BepinLogger.LogInfo("Workstation On Update Postfix");
  }
  /*
  private static ComponentTypeHandle<ResearchStation> testStation;
  private static ComponentTypeHandle<HaveUnlocksInStation> testUnlocks;

  [HarmonyPatch(typeof(ActiveResearchstationSequenceSystem), nameof(ActiveResearchstationSequenceSystem.OnCreate))]
  [HarmonyPrefix]
  public static bool Prefix(ActiveResearchstationSequenceSystem __instance)
  {
      Plugin.BepinLogger.LogInfo("Workstation On Create Prefix");
      return true;
  }
  [HarmonyPatch(typeof(ActiveResearchstationSequenceSystem), nameof(ActiveResearchstationSequenceSystem.OnCreate))]
  [HarmonyPostfix]
  public static void Postfix(ActiveResearchstationSequenceSystem __instance)
  {
      testStation = __instance.__TypeHandle.__ProjectM_ResearchStation_RO_ComponentTypeHandle;
      testUnlocks = __instance.__TypeHandle.__ProjectM_HaveUnlocksInStation_RO_ComponentTypeHandle;

      Plugin.BepinLogger.LogInfo("Workstation On Create Postfix");
  }

  [HarmonyPatch(typeof(ActiveResearchstationSequenceSystem), nameof(ActiveResearchstationSequenceSystem.OnUpdate))]
  [HarmonyPrefix]
  public static bool UpdatePrefix(ActiveResearchstationSequenceSystem __instance)
  {
      testStation = __instance.__TypeHandle.__ProjectM_ResearchStation_RO_ComponentTypeHandle;
      testUnlocks = __instance.__TypeHandle.__ProjectM_HaveUnlocksInStation_RO_ComponentTypeHandle;

      Plugin.BepinLogger.LogInfo("Workstation On Update Prefix");
      return true;
  }
  [HarmonyPatch(typeof(ActiveResearchstationSequenceSystem), nameof(ActiveResearchstationSequenceSystem.OnUpdate))]
  [HarmonyPostfix]
  public static void UpdatePostfix(ActiveResearchstationSequenceSystem __instance)
  {
      testStation = __instance.__TypeHandle.__ProjectM_ResearchStation_RO_ComponentTypeHandle;
      testUnlocks = __instance.__TypeHandle.__ProjectM_HaveUnlocksInStation_RO_ComponentTypeHandle;
      Plugin.BepinLogger.LogInfo("Workstation On Update Postfix");
  }
  /*
  public static NativeParallelHashSet<PrefabGUID> _UnlockedRecipes;
  // majority of this code adapted from VampireCommandFramework @ VCF.Core/Breadstone/ChatHook.cs
  [HarmonyPatch(typeof(WorkstationSubMenuMapper), nameof(WorkstationSubMenuMapper.OnCreate))]
  [HarmonyPrefix]
  public static bool Prefix(WorkstationSubMenuMapper __instance)
  {
      //Plugin.BepinLogger.LogInfo("Workstation On Create Prefix");
      return true;
  }
  [HarmonyPatch(typeof(WorkstationSubMenuMapper), nameof(WorkstationSubMenuMapper.OnCreate))]
  [HarmonyPostfix]
  public static void Postfix(WorkstationSubMenuMapper __instance)
  {
     _UnlockedRecipes = __instance._UnlockedRecipes;
      foreach (var recipe in _UnlockedRecipes)
      {
          Plugin.BepinLogger.LogInfo($"Unlocked recipe: {recipe}");
      }
      // Plugin.BepinLogger.LogInfo("Workstation On Create Postfix");
  }

  [HarmonyPatch(typeof(WorkstationSubMenuMapper), nameof(WorkstationSubMenuMapper.OnUpdate))]
  [HarmonyPrefix]
  public static bool UpdatePrefix(WorkstationSubMenuMapper __instance)
  {
      if (__instance._UnlockedRecipes.Count() != _UnlockedRecipes.Count())
      {
          Plugin.BepinLogger.LogInfo("Workstation On Update Prefix");

          foreach (var recipe in _UnlockedRecipes)
          {
              if (!_UnlockedRecipes.Contains(recipe))
                  Plugin.BepinLogger.LogInfo($"[Update] Unlocked recipe: {recipe}");
          }
          _UnlockedRecipes = __instance._UnlockedRecipes;
      }
      return true;
  }
  [HarmonyPatch(typeof(WorkstationSubMenuMapper), nameof(WorkstationSubMenuMapper.OnUpdate))]
  [HarmonyPostfix]
  public static void UpdatePostfix(WorkstationSubMenuMapper __instance)
  {
      if (__instance._UnlockedRecipes.Count() != _UnlockedRecipes.Count())
      {
          Plugin.BepinLogger.LogInfo("Postfix");

          _UnlockedRecipes = __instance._UnlockedRecipes;
          foreach (var recipe in _UnlockedRecipes)
          {
              Plugin.BepinLogger.LogInfo($"[Update] Unlocked recipe: {recipe}");
          }
      }
      //Plugin.BepinLogger.LogInfo("Workstation On Update Postfix");
  }

  [HarmonyPatch(typeof(WorkstationSubMenuMapper), nameof(WorkstationSubMenuMapper.GetTargetWorkstationEntity))]
  [HarmonyPrefix]
  public static bool GetTargetWorkstationEntityPrefix(WorkstationSubMenuMapper __instance)
  {
      //Plugin.BepinLogger.LogInfo("Workstation On GetTargetWorkstationEntity Prefix");
      return true;
  }
  [HarmonyPatch(typeof(WorkstationSubMenuMapper), nameof(WorkstationSubMenuMapper.GetTargetWorkstationEntity))]
  [HarmonyPostfix]
  public static void GetTargetWorkstationEntityPostfix(WorkstationSubMenuMapper __instance)
  {
      //Plugin.BepinLogger.LogInfo("Workstation On GetTargetWorkstationEntity` Postfix");
  }
  */
}
