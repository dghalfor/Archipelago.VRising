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
            if (!ProgressionHandler.IsResearching)
            {
               var message = (FixedString512Bytes)"You just tried to unlock a spell, but are not in Research mode, type '.startResearch' to be able to send spell checks";
                var user = entityManager.GetComponentData<User>(userEntity);
                ServerChatUtils.SendSystemMessageToClient(entityManager, user, ref message);
                return false;
            }
            Plugin.APClient.SendLocationCheck(DebugTool.GetPrefabName(abilityGroup));

            return true;
        }

        [HarmonyPatch(typeof(SpellSchoolProgressionUtility_Server), nameof(SpellSchoolProgressionUtility_Server.TryUnlockAbility))]
        [HarmonyPostfix]
        public static void PostfixServer(EntityManager entityManager, PrefabLookupMap prefabLookupMap, Entity userEntity, PrefabGUID abilityGroup, bool ignorePointCost = false)
        {
            //if not in researching mode, deny this
            //ProgressionHandler.LockSpellAbilityForPlayer(userEntity, abilityGroup);
            //ChatMessage.NotifyClientLock(abilityGroup.GuidHash);
            return;
        }
    }
}
