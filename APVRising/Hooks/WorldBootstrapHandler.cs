using APVRising.Systems;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Reflection;
using ProjectM;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace APVRising.Hooks
{
    [HarmonyPatch]
    public static class WorldBootstrapPatch
    {
        static readonly System.Reflection.MethodInfo _getOrCreate = typeof(World)
            .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .First(m =>
                m.Name == nameof(World.GetOrCreateSystemManaged) &&
                m.IsGenericMethodDefinition &&
                m.GetParameters().Length == 0
            );

        [HarmonyPatch(typeof(WorldBootstrapUtilities), nameof(WorldBootstrapUtilities.AddSystemsToWorld))]
        [HarmonyPrefix]
        public static void Prefix(World world, WorldBootstrap worldConfig, WorldSystemConfig worldSystemConfig)
        {
            try
            {
                if (world.Name.Equals("Server"))
                {
                    RegisterArchipelagoSystem(world);
                }
            }
            catch (Exception e)
            {
                Plugin.BepinLogger.LogError($"[WorldBootstrapPatch] Exception: {e}");
            }
        }

        static void RegisterArchipelagoSystem(World world)
        {
            var updateGroup = world.GetOrCreateSystemManaged<UpdateGroup>();

            ClassInjector.RegisterTypeInIl2Cpp(typeof(ArchipelagoItemSystem));

            var getOrCreate = _getOrCreate.MakeGenericMethod(typeof(ArchipelagoItemSystem));
            var instance = getOrCreate.Invoke(world, null)
                ?? throw new InvalidOperationException("Failed to create ArchipelagoItemSystem.");

            var systemInstance = (ComponentSystemBase)instance;
            updateGroup.AddSystemToUpdateList(systemInstance);
            updateGroup.SortSystems();

            Plugin.BepinLogger.LogInfo("[AP] ArchipelagoItemSystem registered in Server world");
        }
    }
}
