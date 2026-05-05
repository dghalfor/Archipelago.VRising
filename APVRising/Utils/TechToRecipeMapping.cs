using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace APVRising.Utils;
public static class TechToRecipeMapping
{
    // Maps tech PrefabGUID hash -> list of recipe PrefabGUID hashes
    // TODO this was AI genned, review
    public static readonly Dictionary<int, List<int>> TechToRecipes = new()
    {
        // Weapons
        { 507915220,  new List<int> { 897446828 } },                    // Tech_Weapon_Mace_T04 -> Recipe_Weapon_Mace_T04_Copper_Reinforced
        { -437562995, new List<int> { -1538728965 } },                  // Tech_Weapon_Mace_T06 -> Recipe_Weapon_Mace_T06_Iron_Reinforced
        { -412324833, new List<int> { -1492594940 } },                  // Tech_Weapon_Mace_T08 -> Recipe_Weapon_Mace_T08_Sanguine
        { -632708133, new List<int> { -411123427 } },                   // Tech_Weapon_Axe_T04 -> Recipe_Weapon_Axe_T04_Copper_Reinforced
        { -2012042353, new List<int> { 690858507 } },                   // Tech_Weapon_Axe_T06 -> Recipe_Weapon_Axe_T06_Iron_Reinforced
        { 1895745785, new List<int> { -67490827 } },                    // Tech_Weapon_Axe_T08 -> Recipe_Weapon_Axe_T08_Sanguine
        { -997169234, new List<int> { -283375796 } },                   // Tech_Weapon_Crossbow_T04 -> Recipe_Weapon_Crossbow_T04_Copper_Reinforced
        { 1000023879, new List<int> { 1341382268 } },                   // Tech_Weapon_Crossbow_T06 -> Recipe_Weapon_Crossbow_T06_Iron_Reinforced
        { -1333600826, new List<int> { -1064000514 } },                 // Tech_Weapon_Crossbow_T08 -> Recipe_Weapon_Crossbow_T08_Sanguine
        { -54738837,  new List<int> { -118222260 } },                   // Tech_Weapon_Spear_T04 -> Recipe_Weapon_Spear_T04_Copper_Reinforced
        { -1396617298, new List<int> { -499925914 } },                  // Tech_Weapon_Spear_T06 -> Recipe_Weapon_Spear_T06_Iron_Reinforced
        { -759663833, new List<int> { -314047482 } },                   // Tech_Weapon_Spear_T08 -> Recipe_Weapon_Spear_T08_Sanguine
        { 1950052883, new List<int> { 774557022 } },                    // Tech_Weapon_Sword_T04 -> Recipe_Weapon_Sword_T04_Copper_Reinforced
        { -1685075160, new List<int> { -1052674868 } },                 // Tech_Weapon_Sword_T06 -> Recipe_Weapon_Sword_T06_Iron_Reinforced
        { 361533671, new List<int> { 895742048 } },                     // Tech_Weapon_Sword_T08 -> Recipe_Weapon_Sword_T08_Sanguine
        { -212104516, new List<int> { 777859879 } },                    // Tech_Weapon_Longbow_T04 -> Recipe_Weapon_Longbow_T04_Copper_Reinforced
        { -1988689265, new List<int> { -149592989 } },                  // Tech_Weapon_Longbow_T06 -> Recipe_Weapon_Longbow_T06_Iron_Reinforced
        { -396858635, new List<int> { -603557479 } },                   // Tech_Weapon_Longbow_T08 -> Recipe_Weapon_Longbow_T08_Sanguine
        { -9976124, new List<int> { 396156173 } },                      // Tech_Weapon_Slashers_T04 -> Recipe_Weapon_Slashers_T04_Copper_Reinforced
        { -2121238754, new List<int> { 1469893872 } },                  // Tech_Weapon_Slashers_T06 -> Recipe_Weapon_Slashers_T06_Iron_Reinforced
        { 575105000, new List<int> { 373339628 } },                     // Tech_Weapon_Slashers_T08 -> Recipe_Weapon_Slashers_T08_Sanguine
        { -1073336085, new List<int> { -681071811 } },                  // Tech_Weapon_Reaper_T04 -> Recipe_Weapon_Reaper_T04_Copper_Reinforced
        { 1184108243, new List<int> { 537685806 } },                    // Tech_Weapon_Reaper_T06 -> Recipe_Weapon_Reaper_T06_Iron_Reinforced
        { -409067814, new List<int> { -1816552963 } },                  // Tech_Weapon_Reaper_T08 -> Recipe_Weapon_Reaper_T08_Sanguine
        { 1738256866, new List<int> { -1690827442 } },                  // Tech_Weapon_Claws_T06 -> Recipe_Weapon_Claws_T06_Iron_Reinforced
        { 574338808, new List<int> { -749910443 } },                    // Tech_Weapon_Claws_T08 -> Recipe_Weapon_Claws_T08_Sanguine
        { -1688466299, new List<int> { -328931595 } },                  // Tech_Weapon_Daggers_T06 -> Recipe_Weapon_Daggers_T06_Iron_Reinforced
        { 867896907, new List<int> { 268825874 } },                     // Tech_Weapon_Daggers_T08 -> Recipe_Weapon_Daggers_T08_Sanguine
        { 175562220, new List<int> { 648459378 } },                     // Tech_Weapon_GreatSword_T06 -> Recipe_Weapon_GreatSword_T06_Iron_Reinforced
        { -976123885, new List<int> { 1944286219 } },                   // Tech_Weapon_GreatSword_T08 -> Recipe_Weapon_GreatSword_T08_Sanguine
        { -1341416577, new List<int> { -1015239074 } },                 // Tech_Weapon_Pistols_T06 -> Recipe_Weapon_Pistols_T06_Iron_Reinforced
        { -1917260012, new List<int> { 1058461467 } },                  // Tech_Weapon_Pistols_T08 -> Recipe_Weapon_Pistols_T08_Sanguine
        { 928267897, new List<int> { 1687058710 } },                    // Tech_Weapon_TwinBlades_T06 -> Recipe_Weapon_TwinBlades_T06_Iron_Reinforced
        { -711409497, new List<int> { 1259720344 } },                   // Tech_Weapon_TwinBlades_T08 -> Recipe_Weapon_TwinBlades_T08_Sanguine
        { 1500666524, new List<int> { 465080212 } },                    // Tech_Weapon_Whip_T06 -> Recipe_Weapon_Whip_T06_Iron_Reinforced
        { 1515808838, new List<int> { -1968497565 } },                  // Tech_Weapon_Whip_T08 -> Recipe_Weapon_Whip_T08_Sanguine

        // Armor - Chest
        { -178432582, new List<int> { -42975513 } },                    // Tech_Armor_Chest_T04_Brute -> Recipe_Armor_Chest_T04_Copper_Brute
        { 1868487918, new List<int> { -235084625 } },                   // Tech_Armor_Chest_T04_Rogue -> Recipe_Armor_Chest_T04_Copper_Rogue
        { 755372402, new List<int> { 1490955797 } },                    // Tech_Armor_Chest_T04_Scholar -> Recipe_Armor_Chest_T04_Copper_Scholar
        { 657926195, new List<int> { -850288860 } },                    // Tech_Armor_Chest_T04_Warrior -> Recipe_Armor_Chest_T04_Copper_Warrior
        { 320958383, new List<int> { 1640689004 } },                    // Tech_Armor_Chest_T06_Brute -> Recipe_Armor_Chest_T06_Iron_Brute
        { -962794065, new List<int> { -921085381 } },                   // Tech_Armor_Chest_T06_Rogue -> Recipe_Armor_Chest_T06_Iron_Rogue
        { 1291904224, new List<int> { 969479018 } },                    // Tech_Armor_Chest_T06_Scholar -> Recipe_Armor_Chest_T06_Iron_Scholar
        { -1191678823, new List<int> { 917114760 } },                   // Tech_Armor_Chest_T06_Warrior -> Recipe_Armor_Chest_T06_Iron_Warrior
        { 750468260, new List<int> { 909405972 } },                     // Tech_Armor_Chest_T08_Brute -> Recipe_Armor_Chest_T08_DarkSilver_Brute
        { -1170753047, new List<int> { 2080647005 } },                  // Tech_Armor_Chest_T08_Rogue -> Recipe_Armor_Chest_T08_DarkSilver_Rogue
        { 1188570352, new List<int> { -246992105 } },                   // Tech_Armor_Chest_T08_Scholar -> Recipe_Armor_Chest_T08_DarkSilver_Scholar
        { -1435202677, new List<int> { 636393327 } },                   // Tech_Armor_Chest_T08_Warrior -> Recipe_Armor_Chest_T08_DarkSilver_Warrior

        // Armor - Legs
        { 352798374, new List<int> { -1149764556 } },                   // Tech_Armor_Legs_T04_Brute -> Recipe_Armor_Legs_T04_Copper_Brute
        { -366570135, new List<int> { -1585906930 } },                  // Tech_Armor_Legs_T04_Rogue -> Recipe_Armor_Legs_T04_Copper_Rogue
        { -1510681319, new List<int> { 1891096609 } },                  // Tech_Armor_Legs_T04_Scholar -> Recipe_Armor_Legs_T04_Copper_Scholar
        { -466068499, new List<int> { -1228356397 } },                  // Tech_Armor_Legs_T04_Warrior -> Recipe_Armor_Legs_T04_Copper_Warrior
        { 754248969, new List<int> { 1446070886 } },                    // Tech_Armor_Legs_T06_Brute -> Recipe_Armor_Legs_T06_Iron_Brute
        { 1811127257, new List<int> { 1989724461 } },                   // Tech_Armor_Legs_T06_Rogue -> Recipe_Armor_Legs_T06_Iron_Rogue
        { -1857837378, new List<int> { 1934342576 } },                  // Tech_Armor_Legs_T06_Scholar -> Recipe_Armor_Legs_T06_Iron_Scholar
        { -1866364260, new List<int> { 1489561003 } },                  // Tech_Armor_Legs_T06_Warrior -> Recipe_Armor_Legs_T06_Iron_Warrior
        { -996734096, new List<int> { 392270656 } },                    // Tech_Armor_Legs_T08_Brute -> Recipe_Armor_Legs_T08_DarkSilver_Brute
        { 419994007, new List<int> { 24363319 } },                      // Tech_Armor_Legs_T08_Rogue -> Recipe_Armor_Legs_T08_DarkSilver_Rogue
        { 1190578873, new List<int> { 1352971933 } },                   // Tech_Armor_Legs_T08_Scholar -> Recipe_Armor_Legs_T08_DarkSilver_Scholar
        { 1738492884, new List<int> { 1912958943 } },                   // Tech_Armor_Legs_T08_Warrior -> Recipe_Armor_Legs_T08_DarkSilver_Warrior

        // Armor - Boots
        { 676266407, new List<int> { -211066785 } },                    // Tech_Armor_Boots_T04_Brute -> Recipe_Armor_Boots_T04_Copper_Brute
        { 2120172621, new List<int> { 30410046 } },                     // Tech_Armor_Boots_T04_Rogue -> Recipe_Armor_Boots_T04_Copper_Rogue
        { 1906599762, new List<int> { 1241016364 } },                   // Tech_Armor_Boots_T04_Scholar -> Recipe_Armor_Boots_T04_Copper_Scholar
        { 1879028083, new List<int> { -1573355501 } },                  // Tech_Armor_Boots_T04_Warrior -> Recipe_Armor_Boots_T04_Copper_Warrior
        { -867568357, new List<int> { 564937663 } },                    // Tech_Armor_Boots_T06_Brute -> Recipe_Armor_Boots_T06_Iron_Brute
        { 399247086, new List<int> { -501436877 } },                    // Tech_Armor_Boots_T06_Rogue -> Recipe_Armor_Boots_T06_Iron_Rogue
        { -2051781325, new List<int> { -1790839980 } },                 // Tech_Armor_Boots_T06_Scholar -> Recipe_Armor_Boots_T06_Iron_Scholar
        { -2023969604, new List<int> { 1598255582 } },                  // Tech_Armor_Boots_T06_Warrior -> Recipe_Armor_Boots_T06_Iron_Warrior
        { 1941997114, new List<int> { 481223129 } },                    // Tech_Armor_Boots_T08_Brute -> Recipe_Armor_Boots_T08_DarkSilver_Brute
        { -592100304, new List<int> { 1020324654 } },                   // Tech_Armor_Boots_T08_Rogue -> Recipe_Armor_Boots_T08_DarkSilver_Rogue
        { -1816818535, new List<int> { 26969974 } },                    // Tech_Armor_Boots_T08_Scholar -> Recipe_Armor_Boots_T08_DarkSilver_Scholar
        { 1673193738, new List<int> { -1671420432 } },                  // Tech_Armor_Boots_T08_Warrior -> Recipe_Armor_Boots_T08_DarkSilver_Warrior

        // Armor - Gloves
        { 935228271, new List<int> { 928177888 } },                     // Tech_Armor_Gloves_T04_Brute -> Recipe_Armor_Gloves_T04_Copper_Brute
        { 1577296935, new List<int> { -1109520881 } },                  // Tech_Armor_Gloves_T04_Rogue -> Recipe_Armor_Gloves_T04_Copper_Rogue
        { -1611749320, new List<int> { 1404998399 } },                  // Tech_Armor_Gloves_T04_Scholar -> Recipe_Armor_Gloves_T04_Copper_Scholar
        { 2125292818, new List<int> { 872441116 } },                    // Tech_Armor_Gloves_T04_Warrior -> Recipe_Armor_Gloves_T04_Copper_Warrior
        { 1232232420, new List<int> { 55560401 } },                     // Tech_Armor_Gloves_T06_Brute -> Recipe_Armor_Gloves_T06_Iron_Brute
        { -1831064302, new List<int> { 550971753 } },                   // Tech_Armor_Gloves_T06_Rogue -> Recipe_Armor_Gloves_T06_Iron_Rogue
        { -2034432336, new List<int> { 2029351741 } },                  // Tech_Armor_Gloves_T06_Scholar -> Recipe_Armor_Gloves_T06_Iron_Scholar
        { -215891793, new List<int> { 1321683558 } },                   // Tech_Armor_Gloves_T06_Warrior -> Recipe_Armor_Gloves_T06_Iron_Warrior
        { -371547835, new List<int> { -693799437 } },                   // Tech_Armor_Gloves_T08_Brute -> Recipe_Armor_Gloves_T08_DarkSilver_Brute
        { 693752504, new List<int> { 894482163 } },                     // Tech_Armor_Gloves_T08_Rogue -> Recipe_Armor_Gloves_T08_DarkSilver_Rogue
        { -2038786647, new List<int> { -494730465 } },                  // Tech_Armor_Gloves_T08_Scholar -> Recipe_Armor_Gloves_T08_DarkSilver_Scholar
        { -1696823248, new List<int> { 1193907705 } },                  // Tech_Armor_Gloves_T08_Warrior -> Recipe_Armor_Gloves_T08_DarkSilver_Warrior

        // Magic Sources
        { 688580986, new List<int> { 606793986 } },     // Tech_MagicSource_General_T04_EmberChain
        { -939673215, new List<int> { -1392969895 } },  // Tech_MagicSource_General_T04_Duskwatcher
        { -987554233, new List<int> { -1643108625 } },  // Tech_MagicSource_General_T04_MistSignet
        { 596593772, new List<int> { 134822591 } },     // Tech_MagicSource_General_T04_KnightRing
        { -1597622418, new List<int> { -1954484110 } }, // Tech_MagicSource_General_T04_SorcererRing
        { -1183635267, new List<int> { -1252143324 } }, // Tech_MagicSource_General_T04_FrozenEye
        { 2120978146, new List<int> { 575942293 } },    // Tech_MagicSource_General_T06_AmethystPendant
        { -2004423580, new List<int> { -1789687685 } }, // Tech_MagicSource_General_T06_EmeraldNecklace
        { -678626936, new List<int> { 2113597811 } },   // Tech_MagicSource_General_T06_MistStoneNecklace
        { 75174829, new List<int> { 1192551289 } },     // Tech_MagicSource_General_T06_RubyPendant
        { 2105959904, new List<int> { 932186802 } },    // Tech_MagicSource_General_T06_SapphirePendant
        { 1266173480, new List<int> { 1272778289 } },   // Tech_MagicSource_General_T06_TopazAmulet
        { 2057306510, new List<int> { -590297568 } },   // Tech_MagicSource_General_T08_Beast
        { -1976191252, new List<int> { -1485680334 } }, // Tech_MagicSource_General_T08_CrimsonSky
        { 1827501900, new List<int> { -321571889 } },   // Tech_MagicSource_General_T08_Delusion
        { -1618357223, new List<int> { -831940419 } },  // Tech_MagicSource_General_T08_FrozenCrypt
        { 438329698, new List<int> { 1926933208 } },    // Tech_MagicSource_General_T08_Madness
        { 1018971284, new List<int> { -715761764 } },   // Tech_MagicSource_General_T08_WickedProphet
    };
    public static bool TryGetRecipes(PrefabGUID techGuid, out List<int> recipeHashes)
    {
        return TechToRecipes.TryGetValue(techGuid.GuidHash, out recipeHashes);
    }

    /// <summary>
    /// Checks if a recipe hash is associated with any tech in the mapping.
    /// </summary>
    /// <param name="recipeHash">The recipe hash to check</param>
    /// <returns>True if the recipe is in the mapping, false otherwise</returns>
    private static bool IsRecipeMapped(int recipeHash)
    {
        foreach (var recipeList in TechToRecipes.Values)
        {
            if (recipeList.Contains(recipeHash))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Synchronizes tech unlocks with recipe unlocks in a DynamicBuffer.
    /// If a tech exists in the mapping and is unlocked, its associated recipes will be added to the buffer.
    /// If a tech exists in the mapping and is locked (not in unlockedTech), its associated recipes will be removed from the buffer.
    /// Only recipes that exist in the mapping will be affected. Unmapped recipes are left untouched.
    /// </summary>
    /// <param name="recipeBuffer">The DynamicBuffer of UnlockedRecipeElement to modify in-place</param>
    /// <param name="unlockedTech">The list of currently unlocked tech hashes</param>
    public static void SyncTechRecipes(DynamicBuffer<UnlockedRecipeElement> recipeBuffer, List<int> unlockedTech)
    {
        if (unlockedTech == null)
        {
            return;
        }

        foreach (var techHash in TechToRecipes.Keys)
        {
            if (TechToRecipes.TryGetValue(techHash, out var recipeHashes))
            {
                bool techIsUnlocked = unlockedTech.Contains(techHash);

                foreach (var recipeHash in recipeHashes)
                {
                    PrefabGUID recipePrefab = new PrefabGUID(recipeHash);

                    if (techIsUnlocked)
                    {
                        // Tech is unlocked, ensure recipe is in the buffer
                        bool recipeExists = false;
                        for (int i = 0; i < recipeBuffer.Length; i++)
                        {
                            if (recipeBuffer[i].UnlockedRecipe == recipePrefab)
                            {
                                recipeExists = true;
                                break;
                            }
                        }

                        if (!recipeExists)
                        {
                            recipeBuffer.Add(new UnlockedRecipeElement { UnlockedRecipe = recipePrefab });
                        }
                    }
                    else
                    {
                        // Tech is locked, only remove if the recipe is mapped
                        if (IsRecipeMapped(recipeHash))
                        {
                            for (int i = recipeBuffer.Length - 1; i >= 0; i--)
                            {
                                if (recipeBuffer[i].UnlockedRecipe == recipePrefab)
                                {
                                    recipeBuffer.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Synchronizes tech unlocks in a DynamicBuffer based on the mapping.
    /// Only adds/removes techs that exist in the TechToRecipeMapping.
    /// Unmapped techs are left untouched.
    /// </summary>
    /// <param name="techBuffer">The DynamicBuffer of UnlockedProgressionElement containing techs to modify in-place</param>
    /// <param name="unlockedTech">The list of tech hashes that should be unlocked</param>
    public static void SyncUnlockedTechs(DynamicBuffer<UnlockedProgressionElement> techBuffer, List<int> unlockedTech)
    {
        if (unlockedTech == null)
        {
            return;
        }

        foreach (var techHash in TechToRecipes.Keys)
        {
            PrefabGUID techPrefab = new PrefabGUID(techHash);
            bool techShouldBeUnlocked = unlockedTech.Contains(techHash);

            if (techShouldBeUnlocked)
            {
                // Tech should be unlocked, ensure it's in the buffer
                bool techExists = false;
                for (int i = 0; i < techBuffer.Length; i++)
                {
                    if (techBuffer[i].UnlockedPrefab == techPrefab)
                    {
                        Plugin.BepinLogger.LogInfo($"Tech {techPrefab} already exists in buffer, skipping add");
                        techExists = true;
                        break;
                    }
                }

                if (!techExists)
                {
                    Plugin.BepinLogger.LogInfo($"Tech {techPrefab} should be unlocked but is not in buffer, adding it");
                    techBuffer.Add(new UnlockedProgressionElement { UnlockedPrefab = techPrefab });
                }
            }
            else
            {
                // Tech should be locked, remove it if it exists
                for (int i = techBuffer.Length - 1; i >= 0; i--)
                {
                    if (techBuffer[i].UnlockedPrefab == techPrefab)
                    {
                        Plugin.BepinLogger.LogInfo($"Tech {techPrefab} should be locked but is in buffer, removing it");
                        techBuffer.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
    public static void SyncResearchStation(DynamicBuffer<ResearchBuffer> techBuffer, List<int> unlockedTech)
    {
        if (unlockedTech == null)
        {
            return;
        }

        foreach (var techHash in TechToRecipes.Keys)
        {
            PrefabGUID techPrefab = new PrefabGUID(techHash);
            bool techShouldBeUnlocked = unlockedTech.Contains(techHash);

            if (techShouldBeUnlocked)
            {
                // Tech should be unlocked, ensure it's in the buffer
                bool techExists = false;
                for (int i = 0; i < techBuffer.Length; i++)
                {
                    if (techBuffer[i].ResearchGuid == techPrefab)
                    {
                        var tech = techBuffer[i];
                        tech.IsResearchByStation = true;
                        techBuffer[i] = tech;
                        break;
                    }
                }

            }
            else
            {
                // Tech should be locked, remove it if it exists
                for (int j = techBuffer.Length - 1; j >= 0; j--)
                {
                    if (techBuffer[j].ResearchGuid == techPrefab)
                    {
                        var tech = techBuffer[j];
                        tech.IsResearchByStation = false;
                        techBuffer[j] = tech;
                        break;
                    }
                }
            }
        }
    }
    public static void SyncResearchSnapshot(DynamicBuffer<Snapshot_ResearchBuffer> techBuffer, List<int> unlockedTech)
    {


        if (unlockedTech == null) return;

        if (!Snapshot_ResearchBuffer.TryGetSerializedSnapshot(techBuffer, readOnly: false, out Snapshot_ResearchBuffer.BufferSnapshotPtr snapshotPtr))
        {
            Plugin.BepinLogger.LogWarning("SyncResearchSnapshot: TryGetSerializedSnapshot failed");
            return;
        }
        unsafe
        {
            if (snapshotPtr.Elements == null || snapshotPtr.Length == 0) return;
            for (int i = 0; i < snapshotPtr.Length; i++)
            {
                ref Snapshot_ResearchBuffer_Data data = ref snapshotPtr.Elements[i];
                int hash = data.ResearchGuid.GetHashCode();

                if (!TechToRecipes.ContainsKey(hash)) continue;

                bool shouldBeUnlocked = unlockedTech.Contains(hash);

                if (data.IsResearchByStation != shouldBeUnlocked)
                {
                    Plugin.BepinLogger.LogInfo($"Snapshot sync: {data.ResearchGuid} {data.IsResearchByStation} -> {shouldBeUnlocked}");
                    data.IsResearchByStation = shouldBeUnlocked;
                    Plugin.BepinLogger.LogInfo($"Verify: {data.ResearchGuid} = {data.IsResearchByStation}");
                }
            }
        }
      
    }
}
    
