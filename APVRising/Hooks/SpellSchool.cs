using APVRising.Archipelago;
using APVRising.Utils;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace APVRising.Hooks
{
    [HarmonyPatch]
    public static class SpellSchoolHandler
    {
        // unwrite unlocked buffer
        [HarmonyPatch(typeof(SpellSchoolProgressionUtility_Server), nameof(SpellSchoolProgressionUtility_Server.TryUnlockAbility))]
        [HarmonyPrefix]
        public static bool PrefixServer(EntityManager entityManager, PrefabLookupMap prefabLookupMap, Entity userEntity, PrefabGUID abilityGroup, bool ignorePointCost = false)
        {
            var message = (FixedString512Bytes)"Spells in Archipelago are not unlocked through this menu.";
            var user = entityManager.GetComponentData<User>(userEntity);
            ServerChatUtils.SendSystemMessageToClient(entityManager, user, ref message);
            return false;
        }
    }
}
