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
            //if (em.HasBuffer<UnlockedProgressionElement>(entity))
            //  Progression[entity] = em.GetBuffer<UnlockedProgressionElement>(entity).ToNativeArray(Allocator.Temp).ToArray().ToList();
            if (em.HasBuffer<UnlockedRecipeElement>(entity))
            {
                var buf = em.GetBuffer<UnlockedRecipeElement>(entity);
                var list = new List<UnlockedRecipeElement>();
                for (int i = 0; i < buf.Length; i++)
                    list.Add(buf[i]);
                Recipe[entity] = list;
                Plugin.BepinLogger.LogInfo($"snapshot recipe length {buf.Length}");
            }

            if (em.HasBuffer<UnlockedBlueprintElement>(entity))
            {
                var buf = em.GetBuffer<UnlockedBlueprintElement>(entity);
                var list = new List<UnlockedBlueprintElement>();
                for (int i = 0; i < buf.Length; i++)
                    list.Add(buf[i]);
                Blueprint[entity] = list;
                Plugin.BepinLogger.LogInfo($"snapshot blueprint length {buf.Length}");
            }

            if (em.HasBuffer<UnlockedShapeshiftElement>(entity))
            {
                var buf = em.GetBuffer<UnlockedShapeshiftElement>(entity);
                var list = new List<UnlockedShapeshiftElement>();
                for (int i = 0; i < buf.Length; i++)
                    list.Add(buf[i]);
                Shapeshift[entity] = list;
                Plugin.BepinLogger.LogInfo($"snapshot shapeshift length {buf.Length}");
            }
        }
        public static void Restore(EntityManager em, Entity entity)
        {
            if (Recipe.TryGetValue(entity, out var recipe))
            {
                var buf = em.GetBuffer<UnlockedRecipeElement>(entity);
                foreach (var e in recipe)
                    if (!BufferContains(buf, e.UnlockedRecipe))
                        buf.Add(e);
                Recipe.Remove(entity);
                Plugin.BepinLogger.LogInfo($"restore recipe length {buf.Length}");

            }

            if (Blueprint.TryGetValue(entity, out var bp))
            {
                var buf = em.GetBuffer<UnlockedBlueprintElement>(entity);
                foreach (var e in bp)
                    if (!BufferContains(buf, e.UnlockedBlueprint))
                        buf.Add(e);
                Blueprint.Remove(entity);
                Plugin.BepinLogger.LogInfo($"restore blueprint length {buf.Length}");

            }

            if (Shapeshift.TryGetValue(entity, out var shift))
            {
                var buf = em.GetBuffer<UnlockedShapeshiftElement>(entity);
                foreach (var e in shift)
                    if (!BufferContains(buf, e.UnlockedShapeshift))
                        buf.Add(e);
                Shapeshift.Remove(entity);
                Plugin.BepinLogger.LogInfo($"restore shapeshift length {buf.Length}");

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
