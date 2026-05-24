using ProjectM;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace APVRising.Utils
{
    public static class ProgressionSnapshot
    {
        public static Dictionary<Entity, List<UnlockedProgressionElement>> Progression = new();
        public static Dictionary<Entity, List<UnlockedRecipeElement>> Recipe = new();
        public static Dictionary<Entity, List<UnlockedBlueprintElement>> Blueprint = new();
        public static Dictionary<Entity, List<UnlockedShapeshiftElement>> Shapeshift = new();

        public static void Capture(EntityManager em, Entity entity)
        {
            if (em.HasBuffer<UnlockedRecipeElement>(entity))
            {
                var buf = em.GetBuffer<UnlockedRecipeElement>(entity);
                if (!Recipe.TryGetValue(entity, out var existing))
                {
                    existing = new List<UnlockedRecipeElement>();
                    Recipe[entity] = existing;
                }
                int added = 0;
                for (int i = 0; i < buf.Length; i++)
                {
                    var item = buf[i];
                    if (!existing.Any(e => e.UnlockedRecipe == item.UnlockedRecipe))
                    {
                        existing.Add(item);
                        added++;
                    }
                }
                Plugin.BepinLogger.LogInfo($"snapshot recipe: buffer={buf.Length}, snapshot={existing.Count}, added={added}");
            }

            if (em.HasBuffer<UnlockedBlueprintElement>(entity))
            {
                var buf = em.GetBuffer<UnlockedBlueprintElement>(entity);
                if (!Blueprint.TryGetValue(entity, out var existing))
                {
                    existing = new List<UnlockedBlueprintElement>();
                    Blueprint[entity] = existing;
                }
                int added = 0;
                for (int i = 0; i < buf.Length; i++)
                {
                    var item = buf[i];
                    if (!existing.Any(e => e.UnlockedBlueprint == item.UnlockedBlueprint))
                    {
                        existing.Add(item);
                        added++;
                    }
                }
                Plugin.BepinLogger.LogInfo($"snapshot blueprint: buffer={buf.Length}, snapshot={existing.Count}, added={added}");
            }

            if (em.HasBuffer<UnlockedShapeshiftElement>(entity))
            {
                var buf = em.GetBuffer<UnlockedShapeshiftElement>(entity);
                if (!Shapeshift.TryGetValue(entity, out var existing))
                {
                    existing = new List<UnlockedShapeshiftElement>();
                    Shapeshift[entity] = existing;
                }
                int added = 0;
                for (int i = 0; i < buf.Length; i++)
                {
                    var item = buf[i];
                    if (!existing.Any(e => e.UnlockedShapeshift == item.UnlockedShapeshift))
                    {
                        existing.Add(item);
                        added++;
                    }
                }
                Plugin.BepinLogger.LogInfo($"snapshot shapeshift: buffer={buf.Length}, snapshot={existing.Count}, added={added}");
            }
        }

        public static void Restore(EntityManager em, Entity entity)
        {
            if (Recipe.TryGetValue(entity, out var recipe))
            {
                var buf = em.GetBuffer<UnlockedRecipeElement>(entity);
                int added = 0;
                foreach (var e in recipe)
                    if (!BufferContains(buf, e.UnlockedRecipe))
                    {
                        buf.Add(e);
                        added++;
                    }
                // snapshot intentionally retained
                Plugin.BepinLogger.LogInfo($"restore recipe: buffer={buf.Length}, added={added}");
            }

            if (Blueprint.TryGetValue(entity, out var bp))
            {
                var buf = em.GetBuffer<UnlockedBlueprintElement>(entity);
                int added = 0;
                foreach (var e in bp)
                    if (!BufferContains(buf, e.UnlockedBlueprint))
                    {
                        buf.Add(e);
                        added++;
                    }
                // snapshot intentionally retained
                Plugin.BepinLogger.LogInfo($"restore blueprint: buffer={buf.Length}, added={added}");
            }

            if (Shapeshift.TryGetValue(entity, out var shift))
            {
                var buf = em.GetBuffer<UnlockedShapeshiftElement>(entity);
                int added = 0;
                foreach (var e in shift)
                    if (!BufferContains(buf, e.UnlockedShapeshift))
                    {
                        buf.Add(e);
                        added++;
                    }
                // snapshot intentionally retained
                Plugin.BepinLogger.LogInfo($"restore shapeshift: buffer={buf.Length}, added={added}");
            }
        }

        // One helper per buffer type since there's no shared interface for the GUID field
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
}
