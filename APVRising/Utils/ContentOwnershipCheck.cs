using APVRising;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Stunlock.Core;
using System.Collections.Generic;
using Unity.Entities;

public static class ContentOwnershipCheck
{
    public static bool UserOwnsRequiredContentFor(
        PrefabGUID prefabGuid,
        Entity userEntity,
        EntityManager entityManager,
        PrefabLookupMap prefabLookup)
    {
        if (!entityManager.HasComponent<User>(userEntity))
        {
            Plugin.BepinLogger.LogWarning($"[ContentCheck] Entity {userEntity} has no User component");
            return false; 
        }

        var user = entityManager.GetComponentData<User>(userEntity);
        return UserOwnsRequiredContentFor(prefabGuid, user.UserContent, entityManager, prefabLookup);
    }

    public static bool UserOwnsRequiredContentFor(
        PrefabGUID prefabGuid,
        UserContentFlags userOwnedFlags,
        EntityManager entityManager,
        PrefabLookupMap prefabLookup)
    {
        if (!prefabLookup.TryGetValue(prefabGuid, out Entity prefabEntity))
        {
            Plugin.BepinLogger.LogWarning($"[ContentCheck] Prefab not found for {prefabGuid}");
            return false; 
        }

        if (!entityManager.HasComponent<ProgressionUserContentDependency>(prefabEntity))
            return true; 

        var dependency = entityManager.GetComponentData<ProgressionUserContentDependency>(prefabEntity);

        // Bitflag containment — user must own the required flag
        return (userOwnedFlags & dependency.Value) == dependency.Value;
    }

    public static List<PrefabGUID> GetOwnedDependencyVariants(
        PrefabGUID baseGroupGuid,
        Entity userEntity,
        EntityManager entityManager,
        PrefabLookupMap prefabLookup)
    {
        var owned = new List<PrefabGUID>();

        var user = entityManager.GetComponentData<User>(userEntity);
        var userOwnedFlags = user.UserContent;

        if (!prefabLookup.TryGetValue(baseGroupGuid, out Entity baseEntity))
            return owned;

        if (!entityManager.HasBuffer<ProgressionDependencyElement>(baseEntity))
        {
            owned.Add(baseGroupGuid); // no variants, base is standalone
            return owned;
        }

        var dependencies = entityManager.GetBuffer<ProgressionDependencyElement>(baseEntity);
        foreach (var dep in dependencies)
        {
            if (UserOwnsRequiredContentFor(dep.PrefabGuid, userOwnedFlags, entityManager, prefabLookup))
                owned.Add(dep.PrefabGuid);
            else
                Plugin.BepinLogger.LogInfo($"[ContentCheck] Skipping {dep.PrefabGuid} — user {userEntity} lacks required DLC");
        }

        return owned;
    }
}