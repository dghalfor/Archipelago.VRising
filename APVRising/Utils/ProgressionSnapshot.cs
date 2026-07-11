using APVRising;
using APVRising.Archipelago;
using APVRising.Data;
using APVRising.Utils;
using ProjectM;
using Stunlock.Core;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

public static class ProgressionSnapshot
{
    public static Dictionary<Entity, List<UnlockedProgressionElement>> Progression = new();
    public static Dictionary<Entity, List<UnlockedRecipeElement>> Recipe = new();
    public static Dictionary<Entity, List<UnlockedBlueprintElement>> Blueprint = new();
    public static Dictionary<Entity, List<UnlockedShapeshiftElement>> Shapeshift = new();

    // Phase 1: raw baseline capture, no filtering, called once when buffers are ready
    public static void CaptureBaseline(EntityManager em, Entity entity)
    {
        Recipe[entity] = new List<UnlockedRecipeElement>();
        Blueprint[entity] = new List<UnlockedBlueprintElement>();
        Shapeshift[entity] = new List<UnlockedShapeshiftElement>();

        if (em.HasBuffer<UnlockedRecipeElement>(entity))
        {
            var buf = em.GetBuffer<UnlockedRecipeElement>(entity);
            for (int i = 0; i < buf.Length; i++)
                Recipe[entity].Add(buf[i]);
            Plugin.BepinLogger.LogInfo($"[Snapshot] Baseline recipe: {Recipe[entity].Count}");
        }

        if (em.HasBuffer<UnlockedBlueprintElement>(entity))
        {
            var buf = em.GetBuffer<UnlockedBlueprintElement>(entity);
            for (int i = 0; i < buf.Length; i++)
                Blueprint[entity].Add(buf[i]);
            Plugin.BepinLogger.LogInfo($"[Snapshot] Baseline blueprint: {Blueprint[entity].Count}");
        }

        if (em.HasBuffer<UnlockedShapeshiftElement>(entity))
        {
            var buf = em.GetBuffer<UnlockedShapeshiftElement>(entity);
            for (int i = 0; i < buf.Length; i++)
                Shapeshift[entity].Add(buf[i]);
            Plugin.BepinLogger.LogInfo($"[Snapshot] Baseline shapeshift: {Shapeshift[entity].Count}");
        }
    }

    // Phase 2: called on AP connect, filters snapshot against ReceivedChecks
    public static void ReconcileWithAP(EntityManager em, Entity entity)
    {
        Plugin.BepinLogger.LogInfo($"[Snapshot] ReconcileWithAP entity: {entity.Index}:{entity.Version}");
        Plugin.BepinLogger.LogInfo($"[Reconcile] ReceivedChecks={ArchipelagoData.ReceivedChecks.Count}, ConfiguredLocations={ArchipelagoData.ConfiguredLocations.Count}");

        PrefabCollectionSystem prefabCollectionSystem;
        if (Plugin.IsServer)
        {
            prefabCollectionSystem = Plugin.PrefabCollectionSystem;
        }
        else
        {
            prefabCollectionSystem = Plugin.ClientCollectionSystem;
        }
        if (!Recipe.ContainsKey(entity)) Recipe[entity] = new List<UnlockedRecipeElement>();
        if (!Blueprint.ContainsKey(entity)) Blueprint[entity] = new List<UnlockedBlueprintElement>();
        if (!Shapeshift.ContainsKey(entity)) Shapeshift[entity] = new List<UnlockedShapeshiftElement>();

        // Build sets of what AP says the player should have
        var allowedRecipes = new HashSet<PrefabGUID>();
        var allowedBlueprints = new HashSet<PrefabGUID>();
        var allowedShapeshifts = new HashSet<PrefabGUID>();

        foreach (var kvp in DataDicts.TechToPrefab)
        {
            var techPrefab = kvp.Value;
            if (!ArchipelagoData.ReceivedChecks.Contains(techPrefab._Value))
                continue;
            CollectTechEntries(em, prefabCollectionSystem, techPrefab, allowedRecipes, allowedBlueprints, allowedShapeshifts);
        }

        // Build sets of what non-AP progression currently grants
        // (vanilla unlocks still in UnlockedProgressionElement that aren't AP-managed)
        if (em.HasBuffer<UnlockedProgressionElement>(entity))
        {
            var progBuf = em.GetBuffer<UnlockedProgressionElement>(entity);
            for (int i = 0; i < progBuf.Length; i++)
            {
                var techPrefab = progBuf[i].UnlockedPrefab;
                var prefabName = DebugTool.GetPrefabName(techPrefab);

                if (string.IsNullOrEmpty(prefabName))
                {
                    Plugin.BepinLogger.LogWarning($"[Snapshot] ReconcileWithAP: could not resolve name for prefab {techPrefab}, skipping in vanilla-merge pass");
                    continue;
                }

                if (DataDicts.EntityNameToAPLocation.ContainsKey(prefabName))
                    continue; // AP-managed, handled above
                CollectTechEntries(em, prefabCollectionSystem, techPrefab, allowedRecipes, allowedBlueprints, allowedShapeshifts);
            }
        }

        // Filter snapshot: remove entries the player isn't entitled to
        Recipe[entity].RemoveAll(e => !allowedRecipes.Contains(e.UnlockedRecipe));
        Blueprint[entity].RemoveAll(e => !allowedBlueprints.Contains(e.UnlockedBlueprint));
        Shapeshift[entity].RemoveAll(e => !allowedShapeshifts.Contains(e.UnlockedShapeshift));

        // Add entries the player should have but are missing from snapshot
        foreach (var guid in allowedRecipes)
        {
            if (!Recipe[entity].Any(e => e.UnlockedRecipe == guid))
                Recipe[entity].Add(new UnlockedRecipeElement { UnlockedRecipe = guid });
        }
        foreach (var guid in allowedBlueprints)
        {
            if (!Blueprint[entity].Any(e => e.UnlockedBlueprint == guid))
                Blueprint[entity].Add(new UnlockedBlueprintElement { UnlockedBlueprint = guid });
        }
        foreach (var guid in allowedShapeshifts)
        {
            if (!Shapeshift[entity].Any(e => e.UnlockedShapeshift == guid))
                Shapeshift[entity].Add(new UnlockedShapeshiftElement { UnlockedShapeshift = guid });
        }

        Plugin.BepinLogger.LogInfo($"[Snapshot] Reconciled: recipe={Recipe[entity].Count}, blueprint={Blueprint[entity].Count}, shapeshift={Shapeshift[entity].Count}");
    }

    // Phase 3/4: event-driven update, called after item received or tech locked (via delay)
    public static void Capture(EntityManager em, Entity entity)
    {
        PrefabCollectionSystem prefabCollectionSystem;
        if (Plugin.IsServer) {
            prefabCollectionSystem = Plugin.PrefabCollectionSystem;
        } else
        {
            prefabCollectionSystem = Plugin.ClientCollectionSystem;
        }
        if (!Recipe.ContainsKey(entity)) Recipe[entity] = new List<UnlockedRecipeElement>();
        if (!Blueprint.ContainsKey(entity)) Blueprint[entity] = new List<UnlockedBlueprintElement>();
        if (!Shapeshift.ContainsKey(entity)) Shapeshift[entity] = new List<UnlockedShapeshiftElement>();

        // Merge new non-AP vanilla entries from live buffers into snapshot
        if (em.HasBuffer<UnlockedProgressionElement>(entity))
        {
            var progBuf = em.GetBuffer<UnlockedProgressionElement>(entity);
            for (int i = 0; i < progBuf.Length; i++)
            {
                var techPrefab = progBuf[i].UnlockedPrefab;
                var prefabName = DebugTool.GetPrefabName(techPrefab);

                if (string.IsNullOrEmpty(prefabName))
                {
                    Plugin.BepinLogger.LogWarning($"[Snapshot] Capture: could not resolve name for prefab {techPrefab}, skipping in vanilla-merge pass");
                    continue;
                }

                if (DataDicts.EntityNameToAPLocation.ContainsKey(prefabName))
                    continue; // AP-managed, handled separately

                if (!prefabCollectionSystem._PrefabLookupMap.TryGetValue(techPrefab, out Entity techEntity))
                    continue;

                if (em.HasBuffer<TechUnlockRecipeBuffer>(techEntity))
                {
                    var buf = em.GetBuffer<TechUnlockRecipeBuffer>(techEntity);
                    for (int j = 0; j < buf.Length; j++)
                    {
                        var guid = buf[j].Guid;
                        if (!Recipe[entity].Any(e => e.UnlockedRecipe == guid))
                            Recipe[entity].Add(new UnlockedRecipeElement { UnlockedRecipe = guid });
                    }
                }

                if (em.HasBuffer<TechUnlockBlueprintBuffer>(techEntity))
                {
                    var buf = em.GetBuffer<TechUnlockBlueprintBuffer>(techEntity);
                    for (int j = 0; j < buf.Length; j++)
                    {
                        var guid = buf[j].Guid;
                        if (!Blueprint[entity].Any(e => e.UnlockedBlueprint == guid))
                            Blueprint[entity].Add(new UnlockedBlueprintElement { UnlockedBlueprint = guid });
                    }
                }

                if (em.HasBuffer<ProgressionBookShapeshiftElement>(techEntity))
                {
                    var buf = em.GetBuffer<ProgressionBookShapeshiftElement>(techEntity);
                    for (int j = 0; j < buf.Length; j++)
                    {
                        var guid = buf[j].Shapeshift;
                        if (!Shapeshift[entity].Any(e => e.UnlockedShapeshift == guid))
                            Shapeshift[entity].Add(new UnlockedShapeshiftElement { UnlockedShapeshift = guid });
                    }
                }
            }
        }

        // Sync AP entries: add newly received, remove revoked
        var allowedAPRecipes = new HashSet<PrefabGUID>();
        var allowedAPBlueprints = new HashSet<PrefabGUID>();
        var allowedAPShapeshifts = new HashSet<PrefabGUID>();

        foreach (var kvp in DataDicts.TechToPrefab)
        {
            var techPrefab = kvp.Value;
            if (!ArchipelagoData.ReceivedChecks.Contains(techPrefab._Value))
                continue;
            CollectTechEntries(em, prefabCollectionSystem, techPrefab, allowedAPRecipes, allowedAPBlueprints, allowedAPShapeshifts);
        }

        // Remove AP entries no longer in ReceivedChecks
        var allAPRecipes = new HashSet<PrefabGUID>();
        var allAPBlueprints = new HashSet<PrefabGUID>();
        var allAPShapeshifts = new HashSet<PrefabGUID>();
        foreach (var kvp in DataDicts.TechToPrefab)
            CollectTechEntries(em, prefabCollectionSystem, kvp.Value, allAPRecipes, allAPBlueprints, allAPShapeshifts);

        Recipe[entity].RemoveAll(e => allAPRecipes.Contains(e.UnlockedRecipe) && !allowedAPRecipes.Contains(e.UnlockedRecipe));
        Blueprint[entity].RemoveAll(e => allAPBlueprints.Contains(e.UnlockedBlueprint) && !allowedAPBlueprints.Contains(e.UnlockedBlueprint));
        Shapeshift[entity].RemoveAll(e => allAPShapeshifts.Contains(e.UnlockedShapeshift) && !allowedAPShapeshifts.Contains(e.UnlockedShapeshift));

        // Add newly received AP entries
        foreach (var guid in allowedAPRecipes)
            if (!Recipe[entity].Any(e => e.UnlockedRecipe == guid))
                Recipe[entity].Add(new UnlockedRecipeElement { UnlockedRecipe = guid });
        foreach (var guid in allowedAPBlueprints)
            if (!Blueprint[entity].Any(e => e.UnlockedBlueprint == guid))
                Blueprint[entity].Add(new UnlockedBlueprintElement { UnlockedBlueprint = guid });
        foreach (var guid in allowedAPShapeshifts)
            if (!Shapeshift[entity].Any(e => e.UnlockedShapeshift == guid))
                Shapeshift[entity].Add(new UnlockedShapeshiftElement { UnlockedShapeshift = guid });

        Plugin.BepinLogger.LogInfo($"[Snapshot] Capture: recipe={Recipe[entity].Count}, blueprint={Blueprint[entity].Count}, shapeshift={Shapeshift[entity].Count}");
    }

    // Restore: push snapshot into live buffers, no filtering needed since snapshot is authoritative
    public static void Restore(EntityManager em, Entity entity)
    {
        Plugin.BepinLogger.LogInfo($"[Snapshot] Restore entity: {entity.Index}:{entity.Version}");
        Plugin.BepinLogger.LogInfo($"[Snapshot] Restore: ConfiguredLocations={ArchipelagoData.ConfiguredLocations.Count}, TechToPrefab={DataDicts.TechToPrefab.Count}");

        PrefabCollectionSystem prefabCollectionSystem;
        if (Plugin.IsServer)
        {
            prefabCollectionSystem = Plugin.PrefabCollectionSystem;
        }
        else
        {
            prefabCollectionSystem = Plugin.ClientCollectionSystem;
        }
        // Build set of all AP-managed recipe/blueprint/shapeshift GUIDs
        // so we only enforce locking on those, leaving vanilla entries alone
        var allAPRecipes = new HashSet<PrefabGUID>();
        var allAPBlueprints = new HashSet<PrefabGUID>();
        var allAPShapeshifts = new HashSet<PrefabGUID>();

        if (Plugin.IsServer) {
            foreach (var kvp in DataDicts.TechToPrefab)
            {
                // Only treat as AP-managed if it's actually configured for this session
                if (!DataDicts.EntityNameToAPLocation.TryGetValue(kvp.Key, out var locationName))
                    continue;
                if (!Plugin.APClient.IsConfiguredLocation(locationName))
                    continue;

                CollectTechEntries(em, prefabCollectionSystem, kvp.Value, allAPRecipes, allAPBlueprints, allAPShapeshifts);
            }
        } else
        {
            foreach (var kvp in DataDicts.TechToPrefab)
            {
                if (!ArchipelagoData.ConfiguredLocations.Contains(kvp.Value._Value))
                    continue;
                CollectTechEntries(em, prefabCollectionSystem, kvp.Value, allAPRecipes, allAPBlueprints, allAPShapeshifts);
            }  
        }

        if (Recipe.TryGetValue(entity, out var recipe))
        {
            var buf = em.GetBuffer<UnlockedRecipeElement>(entity);
            var allowed = new HashSet<PrefabGUID>(recipe.Select(e => e.UnlockedRecipe));

            // Only remove entries that are AP-managed and not in snapshot
            for (int i = buf.Length - 1; i >= 0; i--)
            {
                var guid = buf[i].UnlockedRecipe;
                if (allAPRecipes.Contains(guid) && !allowed.Contains(guid))
                    buf.RemoveAt(i);
            }

            int added = 0;
            foreach (var e in recipe)
                if (!BufferContains(buf, e.UnlockedRecipe))
                {
                    buf.Add(e);
                    added++;
                }

            Plugin.BepinLogger.LogInfo($"[Snapshot] Restore recipe: buffer={buf.Length}, added={added}");
        }

        if (Blueprint.TryGetValue(entity, out var bp))
        {
            var buf = em.GetBuffer<UnlockedBlueprintElement>(entity);
            var allowed = new HashSet<PrefabGUID>(bp.Select(e => e.UnlockedBlueprint));

            for (int i = buf.Length - 1; i >= 0; i--)
            {
                var guid = buf[i].UnlockedBlueprint;
                if (allAPBlueprints.Contains(guid) && !allowed.Contains(guid))
                    buf.RemoveAt(i);
            }

            int added = 0;
            foreach (var e in bp)
                if (!BufferContains(buf, e.UnlockedBlueprint))
                {
                    buf.Add(e);
                    added++;
                }

            Plugin.BepinLogger.LogInfo($"[Snapshot] Restore blueprint: buffer={buf.Length}, added={added}");
        }

        if (Shapeshift.TryGetValue(entity, out var shift))
        {
            var buf = em.GetBuffer<UnlockedShapeshiftElement>(entity);
            var allowed = new HashSet<PrefabGUID>(shift.Select(e => e.UnlockedShapeshift));

            for (int i = buf.Length - 1; i >= 0; i--)
            {
                var guid = buf[i].UnlockedShapeshift;
                if (allAPShapeshifts.Contains(guid) && !allowed.Contains(guid))
                    buf.RemoveAt(i);
            }

            int added = 0;
            foreach (var e in shift)
                if (!BufferContains(buf, e.UnlockedShapeshift))
                {
                    buf.Add(e);
                    added++;
                }

            Plugin.BepinLogger.LogInfo($"[Snapshot] Restore shapeshift: buffer={buf.Length}, added={added}");
        }
    }

    // Shared helper: collects all recipes/blueprints/shapeshifts a given tech grants
    private static void CollectTechEntries(
        EntityManager em,
        PrefabCollectionSystem prefabCollectionSystem,
        PrefabGUID techPrefab,
        HashSet<PrefabGUID> recipes,
        HashSet<PrefabGUID> blueprints,
        HashSet<PrefabGUID> shapeshifts)
    {
        if (!prefabCollectionSystem._PrefabLookupMap.TryGetValue(techPrefab, out Entity techEntity))
            return;

        if (em.HasBuffer<TechUnlockRecipeBuffer>(techEntity))
        {
            var buf = em.GetBuffer<TechUnlockRecipeBuffer>(techEntity);
            for (int i = 0; i < buf.Length; i++)
                recipes.Add(buf[i].Guid);
        }

        if (em.HasBuffer<TechUnlockBlueprintBuffer>(techEntity))
        {
            var buf = em.GetBuffer<TechUnlockBlueprintBuffer>(techEntity);
            for (int i = 0; i < buf.Length; i++)
                blueprints.Add(buf[i].Guid);
        }

        if (em.HasBuffer<ProgressionBookShapeshiftElement>(techEntity))
        {
            var buf = em.GetBuffer<ProgressionBookShapeshiftElement>(techEntity);
            for (int i = 0; i < buf.Length; i++)
                shapeshifts.Add(buf[i].Shapeshift);
        }
    }

    private static bool BufferContains(DynamicBuffer<UnlockedRecipeElement> buf, PrefabGUID guid)
    {
        for (int i = 0; i < buf.Length; i++)
            if (buf[i].UnlockedRecipe == guid) return true;
        return false;
    }

    private static bool BufferContains(DynamicBuffer<UnlockedBlueprintElement> buf, PrefabGUID guid)
    {
        for (int i = 0; i < buf.Length; i++)
            if (buf[i].UnlockedBlueprint == guid) return true;
        return false;
    }

    private static bool BufferContains(DynamicBuffer<UnlockedShapeshiftElement> buf, PrefabGUID guid)
    {
        for (int i = 0; i < buf.Length; i++)
            if (buf[i].UnlockedShapeshift == guid) return true;
        return false;
    }
}