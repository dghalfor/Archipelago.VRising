using HarmonyLib;
using ProjectM;
using ProjectM.Shared;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace APVRising.Patches.Diagnostics
{
    public static class ProgressionBufferDumper
    {
        public static void DumpJobData(string tag, ref ProgressionUtility.UpdateUnlockedJobData jobData)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"[{tag}] UpdateUnlockedJobData fields:");
                var type = jobData.GetType();
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    object val;
                    try { val = field.GetValue(jobData); }
                    catch (Exception ex) { val = $"<err: {ex.Message}>"; }
                    sb.AppendLine($"  {field.FieldType.Name} {field.Name} = {val}");
                }
                Plugin.BepinLogger.LogWarning(sb.ToString());
            }
            catch (Exception ex)
            {
                Plugin.BepinLogger.LogError($"[{tag}] Failed to dump jobData: {ex}");
            }
        }

        public static void DumpBuffer(string tag, Entity progressionEntity, DynamicBuffer<UnlockedProgressionElement> buffer)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine($"[{tag}] progressionEntity={progressionEntity.Index}:{progressionEntity.Version} bufferLength={buffer.Length}");
                for (int i = 0; i < buffer.Length; i++)
                {
                    var elem = buffer[i];
                    sb.AppendLine($"  [{i}] {elem}");
                }
                Plugin.BepinLogger.LogWarning(sb.ToString());
            }
            catch (Exception ex)
            {
                Plugin.BepinLogger.LogError($"[{tag}] Failed to dump buffer: {ex}");
            }
        }

        public static void DumpCallContext(string tag)
        {
            try
            {
                var frameCount = Time.frameCount;
                var trace = new System.Diagnostics.StackTrace(1, false);
                var frames = trace.GetFrames();
                string frameSummary = frames != null
                    ? string.Join(" <- ", frames.Take(6).Select(f => f.GetMethod()?.DeclaringType?.Name + "." + f.GetMethod()?.Name))
                    : "<no managed frames>";
                Plugin.BepinLogger.LogWarning($"[{tag}] frame={frameCount} managedStack={frameSummary}");
            }
            catch (Exception ex)
            {
                Plugin.BepinLogger.LogError($"[{tag}] Failed to dump call context: {ex}");
            }
        }
    }
    // --- 3-arg overload ---
    [HarmonyPatch(typeof(ProgressionUtility), nameof(ProgressionUtility.UpdateUnlockedBuffers),
        new Type[] {
            typeof(Entity),
            typeof(DynamicBuffer<UnlockedProgressionElement>),
            typeof(ProgressionUtility.UpdateUnlockedJobData) // plain typeof, no MakeByRefType
        },
        new ArgumentType[] {
            ArgumentType.Normal,
            ArgumentType.Normal,
            ArgumentType.Ref // marks the 3rd param as ref
        })]
    public static class UpdateUnlockedBuffers3_Diagnostic
    {
        [HarmonyPrefix]
        public static bool Prefix(
            Entity progressionEntity,
            DynamicBuffer<UnlockedProgressionElement> unlockedProgressionElements,
            ref ProgressionUtility.UpdateUnlockedJobData jobData)
        {
            ProgressionBufferDumper.DumpCallContext("Buffers3.Prefix");
            ProgressionBufferDumper.DumpJobData("Buffers3.Prefix", ref jobData);
            ProgressionBufferDumper.DumpBuffer("Buffers3.Prefix.Before", progressionEntity, unlockedProgressionElements);
            return true;
        }
    }

    // --- 4-arg overload ---
    [HarmonyPatch(typeof(ProgressionUtility), nameof(ProgressionUtility.UpdateUnlockedBuffers),
        new Type[] {
        typeof(Entity),
        typeof(DynamicBuffer<UnlockedProgressionElement>),
        typeof(UserContentFlags),
        typeof(ProgressionUtility.UpdateUnlockedJobData)
        },
        new ArgumentType[] {
        ArgumentType.Normal,
        ArgumentType.Normal,
        ArgumentType.Normal,
        ArgumentType.Ref
        })]
    public static class UpdateUnlockedBuffers4_Diagnostic
    {
        [HarmonyPrefix]
        public static bool Prefix(
            Entity progressionEntity,
            DynamicBuffer<UnlockedProgressionElement> unlockedProgressionElements,
            UserContentFlags userContentFlags,
            ref ProgressionUtility.UpdateUnlockedJobData jobData)
        {
            ProgressionBufferDumper.DumpCallContext("Buffers4.Prefix");
            ProgressionBufferDumper.DumpJobData("Buffers4.Prefix", ref jobData);
            ProgressionBufferDumper.DumpBuffer("Buffers4.Prefix.Snapshot", progressionEntity, unlockedProgressionElements);

            return true; // skip the crashy original entirely for now
        }
    }
}