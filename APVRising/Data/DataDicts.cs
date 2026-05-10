using Stunlock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProjectM.CastleBuilding.GenerateCastleSystem.LayoutRoom;
using static UnityEngine.InputSystem.Layouts.InputDeviceBuilder;

namespace APVRising.Data
{
        public static class DataDicts
        {
        public static readonly Dictionary<string, string> EntityNameToAPLocation = new Dictionary<string, string>
        {
            // --- ABILITIES ---
            {"Tech_Ability_CommandingForm", "Quincey the Bandit King (Smithy)"}, // [?] Commanding Form is gained from Quincey
            {"Tech_Ability_PsychicForm", "Jade the Vampire Hunter (Pistols)"}, // [?] Psychic/Rat Form from Nibbles — but Commanding Form fits Quincey better; see note







            // --- COLLECTIONS: BONE / ACT 1 STRUCTURE ---
            { "Tech_Collection_Armor_T01_BoneAll", "Errol the Stonebreaker (Material and Gem Storage)"},   // [?] Bone armor often tied to Stonebreaker or early crafting
            { "Tech_Collection_Armor_T01_BoneLower", "Errol the Stonebreaker (Material and Gem Storage)"}, // [?]
            { "Tech_Collection_Armor_T01_BoneUpper_Salve", "Errol the Stonebreaker (Material and Gem Storage)"}, // [?]

            // --- COLLECTIONS: CRAFTING / STATION ---
            { "Tech_Collection_ArtisansCorner", "Rufus the Foreman (Simple Furniture)"},
            { "Tech_Collection_BloodTracking", "Rufus the Foreman (Woodworking Bench)"}, // [?] Blood tracking feels like an early Rufus unlock
            { "Tech_Collection_Brazier_01", "Errol the Stonebreaker (Copper Torch & Brazier)"},
            { "Tech_Collection_Braziers02", "Clive the Firestarter (Alchemy Table)"}, // [?] tier 2 braziers
            { "Tech_Collection_Braziers03", "Grethel the Glassblower (Wall Hanging Mirrors)"}, // [?] tier 3 braziers

            // --- COLLECTIONS: DECORATIVE ---
            { "Tech_Collection_BustStatues_Vampire", "Beatrice the Tailor (Loom)"}, // [?]
            { "Tech_Collection_CandleStands_T02", "Christina the Sun Priestess (Candles)"},
            { "Tech_Collection_Carpet_T01", "Grayson the Armourer (Workshop Flooring)"},
            { "Tech_Collection_Carpet_T02", "Quincey the Bandit King (Ebonite Stairs & Doors)"}, // [?]
            { "Tech_Collection_Carpet_T03", "Beatrice the Tailor (Assortment of Curtains)"}, // [?]
            { "Tech_Collection_Castle_Paintings", "Maja the Dark Savant (Study)"},
            { "Tech_Collection_Castle_Windows_T01", "Quincey the Bandit King (Ebonite Stairs & Doors)"}, // [?]

            

            // --- COLLECTIONS: DRACULA ARMOR ---
            { "Tech_Collection_Dracula_Armor_Boots", "Dracula (Armor)"},
            { "Tech_Collection_Dracula_Armor_Chest", "Dracula (Armor)"},
            { "Tech_Collection_Dracula_Armor_Gloves", "Dracula (Armor)"},
            { "Tech_Collection_Dracula_Armor_Legs", "Dracula (Armor)"},

            // --- COLLECTIONS: FENCING / OUTDOOR ---
            
            { "Tech_Collection_FenceFountain_Noble_T02", "Octavian the Militia Commander (Wide Gate)"}, // [?]
            { "Tech_Collection_Fireplaces", "Clive the Firestarter (Alchemy Table)"}, // [?]
            { "Tech_Collection_FlyingCandles_T03", "Nicholaus the Fallen (Assortment of Simple Candle Stands)"}, // [?]

            // --- COLLECTIONS: FOUNTAINS ---
            { "Tech_Collection_Fountain_T03", "Polora the Feywalker (Garden Foundations)"}, // [?]

            // --- COLLECTIONS: FRAMEWORK / CASTLE BUILDING ---
            { "Tech_Collection_Framework_CastleHeart", "Rufus the Foreman (Woodworking Bench)"}, // Castle Heart is very early
            { "Tech_Collection_Framework_Gargoyles_and_DLC", "Dracula (Castle)"}, // [?]
            { "Tech_Collection_Framework_T01_Wood_Foundation", "Rufus the Foreman (Woodworking Bench)"},
            { "Tech_Collection_Framework_T01_Wood_Structures", "Rufus the Foreman (Simple Furniture)"},
            { "Tech_Collection_Framework_T02_Stone", "Errol the Stonebreaker (Material and Gem Storage)"},
            { "Tech_Collection_Framework_T02_Stone_DLC_Dracula", "Dracula (Castle)"}, // [?]
            { "Tech_Collection_Framework_T02_Stone_DLC_Gloomrot", "Ziva the Engineer (Gloomrot)"}, // [?] Gloomrot DLC
            { "Tech_Collection_Framework_T02_Stone_DLC_ProjectK", "Quincey the Bandit King (Ebonite Stairs & Doors)"}, // [?]
            { "Tech_Collection_Framework_T02_Stone_DLC_Strongblade", "Tristan the Vampire Hunter (Greatsword)"}, // [?]
            { "Tech_Collection_Framework_T02_Stone_Halloween2022", "Nicholaus the Fallen (Frayed Rugs, Banners & Curtains)"}, // [?]

            // --- COLLECTIONS: MISC DECOR ---
            { "Tech_Collection_Furniture_Chairs_T02", "Rufus the Foreman (Simple Furniture)"},   // [?]
            { "Tech_Collection_Furniture_Desk_Chairs_T02", "Maja the Dark Savant (Study)"},
            { "Tech_Collection_Furniture_Desks_T02", "Maja the Dark Savant (Study)"},
            { "Tech_Collection_Furniture_DressingTables_T03", "Beatrice the Tailor (Loom)"}, // [?]
            
            { "Tech_Collection_Furniture_Sofas_T03", "Maja the Dark Savant (Study)"}, // [?]
            { "Tech_Collection_Furniture_Tables_T02", "Rufus the Foreman (Simple Furniture)"}, // [?]

            // --- COLLECTIONS: GARDEN ---
            { "Tech_Collection_Garden_Furniture_Lanterns_T02", "Polora the Feywalker (Garden Foundations)"},
            { "Tech_Collection_Garden_PlantersDecor_T01", "Polora the Feywalker (Large Growing Plots)"},
            { "Tech_Collection_Garden_PlantersDecor_T02", "Polora the Feywalker (Growing Plot Collection)"},
            { "Tech_Collection_Garden_PlantersDecor_T03", "Polora the Feywalker (Growing Plot Collection)"}, // [?]

            // --- COLLECTIONS: LIGHTING ---
            { "Tech_Collection_HangingLanterns_T02", "Clive the Firestarter (Minor Explosive Box)"}, // [?]
            { "Tech_Collection_Light_GardenLampPosts_T03", "Polora the Feywalker (Garden Foundations)"}, // [?]

            // --- COLLECTIONS: MIRRORS ---
            { "Tech_Collection_Mirrors", "Grethel the Glassblower (Wall Hanging Mirrors)"},
            { "Tech_Collection_Mirrors_Halloween2022", "Grethel the Glassblower (Wall Hanging Mirrors)"}, // [?]

            // --- COLLECTIONS: ORNAMENTS / BANNERS ---
            { "Tech_Collection_Ornaments_Banners_T02", "Rufus the Foreman (Dueling Banner)"},
            { "Tech_Collection_Ornaments_Stone_T01", "Errol the Stonebreaker (Copper Torch & Brazier)"},

            // --- COLLECTIONS: OUTDOOR FLOORS / PAVEMENT ---
            { "Tech_Collection_Outdoor_Floors", "Grayson the Armourer (Workshop Flooring)"},
            { "Tech_Collection_Pavement_Cobblestone", "Errol the Stonebreaker (Material and Gem Storage)"}, // [?]
            { "Tech_Collection_Pavement_Dirt", "Grayson the Armourer (Workshop Flooring)"}, // [?]

            // --- COLLECTIONS: SALVE ---
            { "Tech_Collection_Salve", "Keely the Frost Archer (Traveller's Wrap)"}, // [?] Salve relates to bone/early crafting

            // --- COLLECTIONS: STABLES / STATUES ---
            { "Tech_Collection_Stables_Furnishing", "Sir Erwin the Gallant Cavalier (Stables Furnishing Decor)"},
            { "Tech_Collection_Statues_Vampire", "Tristan the Vampire Hunter (Blood Hunter)"}, // [?]
            { "Tech_Collection_Structures_T01", "Rufus the Foreman (Woodworking Bench)"},
            { "Tech_Collection_Vases01", "Grethel the Glassblower (Glass)"},

            // --- COLLECTIONS: VBLOOD ARENA / MISC REWARDS ---
            { "Tech_Collection_VBlood_ArenaDecorations", "Voltatia the Power Master (Arena Champion)"}, // [?]
            { "Tech_Collection_VBlood_CrystalLamp", "Grethel the Glassblower (Glass)"}, // [?]
            

  



            







            // --- COLLECTIONS: WALLPAPERS / SHELVES / MIRRORS ---
            
            
            { "Tech_Collection_Wallpapers_Bricks01", "Errol the Stonebreaker (Material and Gem Storage)"}, // [?]
            { "Tech_Collection_Wallpapers_Classical01", "Quincey the Bandit King (Ebonite Stairs & Doors)"}, // [?]
            { "Tech_Collection_Wallpapers_Cordial01", "Beatrice the Tailor (Loom)"}, // [?]
            { "Tech_Collection_Wallpapers_Imperious01", "Octavian the Militia Commander (Iron Weapons)"}, // [?]
            { "Tech_Collection_Wallpapers_Prison01", "Vincent the Frostbringer (Prison Framework)"},
            { "Tech_Collection_Wallpapers_Stone01", "Errol the Stonebreaker (Material and Gem Storage)"},
            { "Tech_Collection_Wallpapers_WoodPanel01", "Rufus the Foreman (Woodworking Bench)"},

            // --- COLLECTIONS: WAYPOINT / WORKSHOP ---
            { "Tech_Collection_Waypoint", "Rufus the Foreman (Woodworking Bench)"}, // [?] Waygates are early unlocks
            { "Tech_Collection_Workshop_Decoration", "Grayson the Armourer (Workshop Flooring)"},

            

            // --- CURTAINS ---
            { "Tech_Curtains_T03_Royal", "Beatrice the Tailor (Assortment of Curtains)"},

            

            

            // --- SPELL PASSIVES: BLOOD ---
            { "Tech_SpellPassive_Blood_T01_BloodSpray", "Rufus the Foreman (Blood Tier 1)"}, // [?] Blood passives assigned to appropriate blood-tier bosses
            { "Tech_SpellPassive_Blood_T02_BloodTypeEfficiency", "Tristan the Vampire Hunter (Blood Tier 3)"},
            { "Tech_SpellPassive_Blood_T03_VBloodSlayer", "Tristan the Vampire Hunter (Greater Blood Essence)"},
            { "Tech_SpellPassive_Blood_T04_Rampage", "General Valencia the Depraved (Blood Knight)"}, // [?]
            // CHAOS
            { "Tech_SpellPassive_Chaos_T01_ChaosKindling", "Errol the Stonebreaker (Chaos Tier 1)"},
            { "Tech_SpellPassive_Chaos_T02_RenewingFlames", "Lidia the Chaos Archer (Chaos Tier 1)"},
            { "Tech_SpellPassive_Chaos_T03_Overpower", "Clive the Firestarter (Chaos Tier 2)"},
            { "Tech_SpellPassive_Chaos_T04_RavenousStrikes", "Quincey the Bandit King (Chaos Tier 3)"},
            // FROST
            { "Tech_SpellPassive_Frost_T01_ColdSoul", "Keely the Frost Archer (Frost Tier 1)"},
            { "Tech_SpellPassive_Frost_T02_ChillWeave", "Finn the Fisherman (Frost Tier 1)"},
            { "Tech_SpellPassive_Frost_T03_Bastion", "Vincent the Frostbringer (Veil of Frost)"},
            { "Tech_SpellPassive_Frost_T04_DarkEnchantment", "Terrorclaw the Ogre (Yeti)"}, // [?]
            // ILLUSION
            { "Tech_SpellPassive_Illusion_T01_SpiritualInfusion", "Grayson the Armourer (Illusion Tier 1)"},
            { "Tech_SpellPassive_Illusion_T02_FlowingSorcery", "Polora the Feywalker (Illusion Tier 1)"},
            { "Tech_SpellPassive_Illusion_T03_FeralHaste", "Maja the Dark Savant (Illusion Tier 1)"}, // [?]
            { "Tech_SpellPassive_Illusion_T04_WickedPower", "Mairwyn the Elementalist (Cursed Wanderer)"}, // [?]
            // STORM
            { "Tech_SpellPassive_Storm_T01_LightningFastStrikes", "Sir Erwin the Gallant Cavalier (Storm Tier 1)"},
            { "Tech_SpellPassive_Storm_T02_EnhancedConductivity", "Grethel the Glassblower (Storm Tier 1)"},
            { "Tech_SpellPassive_Storm_T03_HungerForPower", "Voltatia the Power Master (Voltage)"}, // [?]
            { "Tech_SpellPassive_Storm_T04_TurbulentVelocity", "Ziva the Engineer (Archmage)"}, // [?]
            // UNHOLY
            { "Tech_SpellPassive_Unholy_T01_ArcaneAnimator", "Goreswine the Ravager (Unholy Tier 1)"},
            { "Tech_SpellPassive_Unholy_T02_SoulDrinker", "Kriig the Undead General (Unholy Tier 2)"},
            { "Tech_SpellPassive_Unholy_T03_LethalStrikes", "Nicholaus the Fallen (Unholy Tier 3)"},
            { "Tech_SpellPassive_Unholy_T04_EmbraceMayhem", "Cyril the Cursed Smith (Cursed Smith)"}, // [?]

            // --- STORAGE ---
            { "Tech_Storage_Alchemy_T02", "Clive the Firestarter (Alchemy Table)"},
            { "Tech_Storage_Alchemy_T03", "Ziva the Engineer (Archmage)"}, // [?]
            { "Tech_Storage_Armor_T02", "Quincey the Bandit King (Tailoring Bench)"},
            { "Tech_Storage_Blood_T02", "Rufus the Foreman (Woodworking Bench)"},
            { "Tech_Storage_Blood_T03", "Tristan the Vampire Hunter (Greater Blood Essence)"},
            { "Tech_Storage_Coins_T02", "The Duke of Balaton (Coins)"},
            { "Tech_Storage_Consumable_T02", "Grethel the Glassblower (Blood Rose Potion)"},
            { "Tech_Storage_Consumable_T03", "Ziva the Engineer (Elixirs)"}, // [?]
            { "Tech_Storage_Fish_T02", "Finn the Fisherman (Fishing Pole)"},
            { "Tech_Storage_Gems_T01", "Errol the Stonebreaker (Material and Gem Storage)"},
            { "Tech_Storage_Gems_T02", "Ungora the Spider Queen (Spider Queen)"}, // [?]
            { "Tech_Storage_Herbs_T01", "Polora the Feywalker (Large Growing Plots)"},
            { "Tech_Storage_Herbs_T02", "Polora the Feywalker (Growing Plot Collection)"},
            { "Tech_Storage_Jewels_T02", "Domina the Blade Dancer (Jewels)"}, // [?]
            { "Tech_Storage_Knowledge_T02", "Maja the Dark Savant (Study)"},
            { "Tech_Storage_Knowledge_T03", "Raziel the Shepherd (Magic Source)"}, // [?]
            { "Tech_Storage_Minerals_T01", "Errol the Stonebreaker (Material and Gem Storage)"},
            { "Tech_Storage_Minerals_T02", "Quincey the Bandit King (Iron Ingot)"},
            { "Tech_Storage_Pack_T01_A", "Rufus the Foreman (Woodworking Bench)"},
            { "Tech_Storage_Pack_T01_B", "Rufus the Foreman (Simple Furniture)"},
            { "Tech_Storage_T01", "Rufus the Foreman (Woodworking Bench)"},
            { "Tech_Storage_T02", "Quincey the Bandit King (Smithy)"},
            { "Tech_Storage_T03", "Raziel the Shepherd (Raziel)"}, // [?]
            { "Tech_Storage_Tailoring_T02", "Beatrice the Tailor (Loom)"},
            { "Tech_Storage_Tailoring_T03", "Octavian the Militia Commander (Iron Weapons)"}, // [?] Silk tailoring
            { "Tech_Storage_Weapons_T02", "Quincey the Bandit King (Smithy)"},
            { "Tech_Storage_Woodworking_T01", "Rufus the Foreman (Woodworking Bench)"},
            { "Tech_Storage_Woodworking_T02", "Quincey the Bandit King (Smithy)"},


            //-------------------------- BELOW THIS LINE IS Analyzed-------------------------

            // --- COLLECTIONS: ACT 1 VBLOOD ---
            { "Tech_Collection_VBlood_T02_AlphaWolf", "Alpha the White Wolf (Wolf Form)"},
            { "Tech_Collection_VBlood_T02_Keely_Armor", "Keely the Frost Archer (Leather)"},
            { "Tech_Collection_VBlood_T02_KeelyFrostArrow", "Keely the Frost Archer (Tannery)"},
            { "Tech_Collection_VBlood_T02_KeelyFrostArrow_Canteen", "Keely the Frost Archer (Empty Waterskin)"},
            { "Tech_Collection_VBlood_T02_LidiaChaosArrow", "Lidia the Chaos Archer (Leatherworking Station)"},
            { "Tech_Collection_VBlood_T02_LidiaChaosArrow_Longbow", "Lidia the Chaos Archer (Longbow)"},
            { "Tech_Collection_VBlood_T02_RufusForeman", "Rufus the Foreman (Woodworking Bench)"},
            { "Tech_Collection_Furniture_Simple_T01", "Rufus the Foreman (Simple Furniture)"},
            { "Tech_Collection_VBlood_T02_RufusForeman_Crossbow", "Rufus the Foreman (Copper Crossbow)"},
            { "Tech_Collection_VBlood_T02_StoneBreaker", "Errol the Stonebreaker (Material and Gem Storage)"},
            { "Tech_Collection_VBlood_T02_StoneBreaker_CopperWeapons", "Errol the Stonebreaker (Copper Torch & Brazier)"}, // [?] Copper Weapons not a listed location — closest match
            { "Tech_Collection_VBlood_T03_Fisherman", "Finn the Fisherman (Fishing Pole)"},
            { "Tech_Collection_VBlood_T03_Goreswine", "Goreswine the Ravager (Tomb)"},
            { "Tech_Collection_VBlood_T03_Goreswine_MagicSource", "Goreswine the Ravager (Gravedigger Ring)"},
            { "Tech_Collection_VBlood_T03_Goreswine_Units", "Goreswine the Ravager (Raise Skeleton & Ghouls)"},
            { "Tech_Collection_VBlood_T03_GoreswineFence", "Goreswine the Ravager (Graveyard decor)"},
            { "Tech_Collection_VBlood_T03_Grayson", "Grayson the Armourer (Whetstone)"},
            { "Tech_Collection_VBlood_T03_Grayson_Floors", "Grayson the Armourer (Workshop Flooring)"},
            { "Tech_Collection_VBlood_T03_Grayson_TargetDummies", "Grayson the Armourer (Target Dummies)"},
            { "Tech_Collection_VBlood_T03_PutridRat", "Nibbles the Putrid Rat (Rat Form)"},

            // --- COLLECTIONS: ACT 2 VBLOOD ---
            { "Tech_Collection_VBlood_T04_Ball_Cosmetic", "Maja the Dark Savant (Midnight Ball Gown)"},
            { "Tech_Collection_VBlood_T04_CliveTheFirestarter", "Clive the Firestarter (Alchemy Table)"},
            { "Tech_Collection_VBlood_T04_FrostGuard", "Vincent the Frostbringer (Prison Cell)"},
            { "Tech_Collection_VBlood_T04_FrostGuard_PrisonFramework", "Vincent the Frostbringer (Prison Framework)"},
            { "Tech_Collection_VBlood_T04_NicholausTheFallen", "Nicholaus the Fallen (Paper Press)"},
            //{ "Tech_Collection_VBlood_T04_NicholausTheFallen_MagicSource", "Nicholaus the Fallen (Treasury Flooring)"}, // [?] MagicSource = ring/amulet; no longer has it?
            { "Tech_Collection_CandleStands_T01", "Nicholaus the Fallen (Assortment of Simple Candle Stands)"},
            { "Tech_Collection_FrayedDecor", "Nicholaus the Fallen (Frayed Rugs, Banners & Curtains)"},
            { "Tech_Floor_Treasury", "Nicholaus the Fallen (Treasury Flooring)"},
            { "Tech_Collection_VBlood_T04_Poloma", "Polora the Feywalker (Minor Garlic Resistance Brew)"},
            { "Tech_Collection_VBlood_T04_PolomaGardenFloors", "Polora the Feywalker (Garden Foundations)"},
            { "Tech_Collection_VBlood_T04_PolomaPlantersExterior", "Polora the Feywalker (Growing Plot Collection)"},
            { "Tech_Collection_VBlood_T04_PolomaPlantersInterior", "Polora the Feywalker (Large Growing Plots)"},
            { "Tech_Collection_Garden_Hedges_T02", "Polora the Feywalker (Garden Hedges)"},
            { "Tech_Collection_VBlood_T04_Quincey", "Quincey the Bandit King (Smithy)"},
            { "Tech_Collection_VBlood_T04_Quincey_CottonArmor", "Quincey the Bandit King (Hollowfang Battlegear)"},
            { "Tech_Collection_VBlood_T04_Quincey_Decoration", "Quincey the Bandit King (Ebonite Stairs & Doors)"},
            //{ "Tech_Collection_VBlood_T04_Quincey_IronWeapons", "Quincey the Bandit King (Iron Ingot)"}, -doesn't unlock anything
            { "Tech_Collection_VBlood_T04_Tailor", "Beatrice the Tailor (Loom)"},
            { "Tech_Collection_VBlood_T04_Tailor_Curtains", "Beatrice the Tailor (Assortment of Curtains)"},
            { "Tech_Collection_VBlood_T04_VampireHunter", "Tristan the Vampire Hunter (Blood Hunter)"},
            { "Tech_Collection_VBlood_T04_VampireHunter_GreatSword", "Tristan the Vampire Hunter (Greatsword)"},
            { "Tech_Collection_VBlood_T04_Wendigo", "Frostmaw the Mountain Terror (Thick Leather)"},
            { "Tech_Collection_VBlood_T04_Wendigo_Claws", "Frostmaw the Mountain Terror (Claws)"}, 

            // --- COLLECTIONS: ACT 2-3 VBLOOD ---
            { "Tech_Collection_VBlood_T05_ArenaChampion", "Gaius the Cursed Champion (Arena Station)"},
            { "Tech_Collection_VBlood_T05_ArenaChampion_TwinBlades", "Gaius the Cursed Champion (Twinblades)"},
            { "Tech_Collection_VBlood_T05_BishopOfShadow", "Leandra the Shadow Priestess (Scourgestone Pendant)"},
            { "Tech_Collection_VBlood_T05_Fabian", "Sir Erwin the Gallant Cavalier (Stables)"},
            { "Tech_Collection_VBlood_T05_FerociousBear", "Kodia the Ferocious Bear (Bear Form)"},
            { "Tech_Collection_VBlood_T05_FerociousBear_Rugs", "Kodia the Ferocious Bear (Fur Rugs)"},
            { "Tech_Collection_VBlood_T05_GlassBlower", "Grethel the Glassblower (Glass)"},
            { "Tech_Collection_WallHangingMirrors_T02", "Grethel the Glassblower (Wall Hanging Mirrors)"},
            { "Tech_Collection_VBlood_T05_Golem", "Terah the Geomancer (Gem Cutting Table)"},
            { "Tech_Collection_VBlood_T05_GolemGems", "Terah the Geomancer (Regular Gems)"},
            { "Tech_Collection_VBlood_T05_GolemObsidian", "Terah the Geomancer (Obsidian)"},
            { "Tech_Collection_VBlood_T05_HolyNun", "Christina the Sun Priestess (Wool Thread)"},
            { "Tech_Collection_Candles_T02", "Christina the Sun Priestess (Candles)"},
            { "Tech_Collection_VBlood_T05_IceRanger", "General Elena the Hollow (Altar of Stygian Awakening)"},
            { "Tech_Collection_Carpet_Stately", "General Elena the Hollow (Stately Carpets)"},
            { "Tech_Collection_VBlood_T05_Infiltrator", "Bane the Shadowblade (Human Form)"},
            { "Tech_Collection_VBlood_T05_Infiltrator_Daggers", "Bane the Shadowblade (Daggers)"},
            { "Tech_Collection_VBlood_T05_Meredith", "Meredith the Bright Archer (Holy Resistance Potion)"},
            { "Tech_Collection_VBlood_T05_Scribe", "Maja the Dark Savant (Study)"},
            { "Tech_Collection_Fence_Verdant", "Maja the Dark Savant (Verdant Garden Fencing)"},
            { "Tech_Collection_WallHangingShelves_T02", "Maja the Dark Savant (Wall Hanging Shelves)"},
            { "Tech_Collection_VBlood_T05_UndeadLeader", "Kriig the Undead General (Skeleton Priest)"},
            { "Tech_Collection_VBlood_T05_UndeadLeader_Bells", "Kriig the Undead General (Castle Door Bells)"},
            { "Tech_Collection_VBlood_T05_UndeadLeader_Reaper", "Kriig the Undead General (Reaper)"},

            // --- COLLECTIONS: ACT 3 VBLOOD (T06) ---
            //{ "Tech_Collection_VBlood_T06_Castleman", "Simon Belmont the Vampire Hunter (Castleman)"}, // Dark silver?
            { "Tech_Collection_VBlood_T06_Castleman_SanguineWhip", "Simon Belmont the Vampire Hunter (Sanguine Whip)"}, // [?]
            { "Tech_Collection_VBlood_T06_CursedWanderer", "Ben the Old Wanderer (Pristine Leather)"}, 
            //{ "Tech_Collection_VBlood_T06_HeadlessHorseman", "Ben the Old Wanderer (Headless Horseman)"}, // Nothing
            { "Tech_Collection_VBlood_T06_HighLord", "General Cassius the Betrayer (Stygian Summoning Circle)"},
            { "Tech_Collection_VBlood_T06_Iva", "Ziva the Engineer (Fabricator)"},
            { "Tech_Collection_VBlood_T06_Jade", "Jade the Vampire Hunter (Primal Blood Essence)"},
            { "Tech_Collection_VBlood_T06_Jade_Pistols", "Jade the Vampire Hunter (Pistols)"},
            { "Tech_Collection_VBlood_T06_MilitiaCommander", "Octavian the Militia Captain (Ancestral Forge)"},
            { "Tech_Collection_VBlood_T06_MilitiaCommander_WideGate", "Octavian the Militia Commander (Wide Gate)"},
            { "Tech_Storage_Pack_Equipment_T02", "Octavian the Militia Captain (Equipment Storage)"},
            { "Tech_Collection_VBlood_T06_Purifier", "Angram the Purifier (Mutated Rat)"},
            { "Tech_Collection_VBlood_T06_Purifier_Gruel", "Angram the Purifier (Irradiant Gruel)"},
            { "Tech_Collection_VBlood_T06_Armor_Silk", "Angram the Purifier (Dawnthorn Regalia)"},
            { "Tech_Collection_VBlood_T06_Raziel", "Raziel the Shepherd (Jewelcrafting Table)"},
            { "Tech_Collection_VBlood_T06_Raziel_Decoration", "(Cordial Stairs & Doors)"},
            { "Tech_Collection_VBlood_T06_SpiderQueen", "Ungora the Spider Queen (Silk)"},
            { "Tech_Collection_VBlood_T06_ToadKing", "Albert the Duke of Balaton (Toad Form)"},
            { "Tech_Collection_VBlood_T06_ToadKing_Coins", "Albert the Duke of Balaton (Coining)"},
            { "Tech_Collection_VBlood_T06_Voltage", "Domina the Blade Dancer (Advanced Grinder)"},
            { "Tech_Collection_VBlood_T06_Whip", "Domina the Blade Dancer (Iron Whip)"},
            { "Tech_Collection_VBlood_T06_Werewolf", "Willfred the Werewolf Chief (Pristine Leather Bag)"},
            { "Tech_Collection_Clocks", "Willfred the Werewolf Chief (Longcase Clocks)"},
            { "Tech_Collection_VBlood_T06_Yeti", "Terrorclaw the Ogre (Advanced Tannery)"},

            // --- COLLECTIONS: ACT 3-4 VBLOOD (T07) ---
            { "Tech_Collection_VBlood_T07_Archmage", "Mairwyn the Elementalist (Holy Resistance Flask)"},
            { "Tech_Collection_VBlood_T07_Archmage_JewelsT03", "Mairwyn the Elementalist (Greater Jewels)"},
            { "Tech_Collection_VBlood_T07_CardinalPriest", "Azariel the Sunbringer (Gold Ingot)"},
            { "Tech_Collection_Carpet_Ostenstatious", "Azariel the Sunbringer (Ostentatious Carpets)"},
            { "Tech_Collection_VBlood_T07_Carver", "Stavros the Carver (Advanced Sawmill)"},
            { "Tech_Collection_VBlood_T07_Carver_Coating", "Stavros the Carver (Weapon Coatings)"},
            { "Tech_Collection_VBlood_T07_CursedSmith", "Cyril the Cursed Smith (Dark Silver Ingot)"},
            { "Tech_Collection_VBlood_T07_HarpyGems", "Morian the Stormwing Matriarch (Flawless Gems)"},
            { "Tech_Collection_VBlood_T07_Livith", "Jakira the Shadow Huntress (Elixir of the Twisted)"},
            { "Tech_Collection_VBlood_T07_Livith_Slashers", "Jakira the Shadow Huntress (Slashers)"},
            { "Tech_Collection_VBlood_T07_Lucie", "Lucile the Venom Alchemist (Blood Homogenizer)"}, 
            //{ "Tech_Collection_VBlood_T07_Lucie_Elixirs", "Lucie the Iteration 3 (Elixirs)"},
            { "Tech_Collection_VBlood_T07_Overseer", "Sir Magnus the Overseer (Phantom's Veil)"},
            { "Tech_Storage_Pack_T02", "Sir Magnus the Overseer (Assortment of Wide Storage Shelves)"},
            { "Tech_Collection_VBlood_T07_Professor", "Henry Blackbrew the Doctor (Athenaeum)"},
            { "Tech_Collection_VBlood_T07_RailgunSergeant", "Voltatia the Power Master (Power Core)"},
            { "Tech_Collection_VBlood_T07_Sommelier", "Baron du Bouchon the Sommelier (Barrel Disguise)"},
            { "Tech_Collection_VBlood_T07_Sommelier_Bloodwine", "Baron du Bouchon the Sommelier (Blood Merlot)"},
            { "Tech_Collection_VBlood_T07_Sommelier_MagicSource", "Baron du Bouchon the Sommelier (Blood Merlot Amulet)"},
            { "Tech_Collection_Fence_Rural", "Baron du Bouchon the Sommelier (Rural Garden Fencing)"},
            { "Tech_Collection_VBlood_T07_Valyr", "Dantos the Forgebinder (Fusion Forge)" },
            { "Tech_Collection_VBlood_T07_Witch", "Matka the Curse Weaver (Advanced Loom)"},
            { "Tech_Collection_VBlood_T07_ZealousCultist", "Foulrot the Soultaker (Spectral Dust)"}, 

            // --- COLLECTIONS: ACT 4 VBLOOD (T08) ---
            { "Tech_Collection_VBlood_T08_BatVampire", "Lord Styx the Night Champion (Bat Form)"},
            { "Tech_Collection_VBlood_T08_Behemoth", "Gorecrusher the Behemoth (Bat Leather)"},
            { "Tech_Collection_VBlood_T08_BloodKnight", "General Valencia the Depraved (Shadow Weave)"},
            { "Tech_Collection_VBlood_JewelsT04", "General Valencia the Depraved (Primal Jewels)"},
            { "Tech_Collection_CoatOfArms", "General Valencia the Depraved (Coat of Arms)"},
            //{ "Tech_Collection_VBlood_T08_Dracula", "Dracula (Dracula)"},
            { "Tech_Collection_VBlood_T08_Manticore", "Solarus the Immaculate (Pedestal of Solarus)"},
            { "Tech_Collection_VBlood_T08_Monster", "Adam the Firstborn (Pedestal of the Monster)"},
            { "Tech_Collection_VBlood_T08_Morgana", "Megara the Serpent Queen (Pedestal of the Serpent)"},
            { "Tech_Collection_VBlood_T08_Paladin", "Solarus the Immaculate (Pedestal of Solarus)"},

            // --- T04 ARMOR (Merciless / Hollowfang tier) ---
            // Quincey unlocks Hollowfang Battlegear (Cotton armor set)
            { "Tech_Armor_Boots_T04_Brute", "Research Desk - Learn Marauder Boots"},
            { "Tech_Armor_Boots_T04_Rogue", "Research Desk - Learn Shadewalker Boots"},
            { "Tech_Armor_Boots_T04_Scholar", "Research Desk - Learn Warlock Boots"},
            { "Tech_Armor_Boots_T04_Warrior", "Research Desk - Learn Grim Ranger Boots"},
            { "Tech_Armor_Chest_T04_Brute", "Research Desk - Learn Marauder Vest"},
            { "Tech_Armor_Chest_T04_Rogue", "Research Desk - Learn Shadewalker Vest"},
            { "Tech_Armor_Chest_T04_Scholar", "Research Desk - Learn Warlock Vest"},
            { "Tech_Armor_Chest_T04_Warrior", "Research Desk - Learn Grim Ranger Vest"},
            { "Tech_Armor_Gloves_T04_Brute", "Research Desk - Learn Marauder Gloves"},
            { "Tech_Armor_Gloves_T04_Rogue", "Research Desk - Learn Shadewalker Gloves"},
            { "Tech_Armor_Gloves_T04_Scholar", "Research Desk - Learn Warlock Gloves"},
            { "Tech_Armor_Gloves_T04_Warrior", "Research Desk - Learn Grim Ranger Gloves"},
            { "Tech_Armor_Legs_T04_Brute", "Research Desk - Learn Marauder Leggings"},
            { "Tech_Armor_Legs_T04_Rogue", "Research Desk - Learn Shadewalker Leggings"},
            { "Tech_Armor_Legs_T04_Scholar", "Research Desk - Learn Warlock Leggings"},
            { "Tech_Armor_Legs_T04_Warrior", "Research Desk - Learn Grim Ranger Leggings"},

            // --- T06 ARMOR (Dawnthorn / Silk tier) ---
            { "Tech_Armor_Boots_T06_Brute", "Study - Learn Crimson Templar Boots"},
            { "Tech_Armor_Boots_T06_Rogue", "Study - Learn Duskwatcher Boots"},
            { "Tech_Armor_Boots_T06_Scholar", "Study - Learn Dark Magus Boots"},
            { "Tech_Armor_Boots_T06_Warrior", "Study - Learn Blood Hunter Boots"},
            { "Tech_Armor_Chest_T06_Brute", "Study - Learn Crimson Templar Chestguard"},
            { "Tech_Armor_Chest_T06_Rogue", "Study - Learn Duskwatcher Chestguard"},
            { "Tech_Armor_Chest_T06_Scholar", "Study - Learn Dark Magus Chestguard"},
            { "Tech_Armor_Chest_T06_Warrior", "Study - Learn Blood Hunter Chestguard"},
            { "Tech_Armor_Gloves_T06_Brute", "Study - Learn Crimson Templar Gloves"},
            { "Tech_Armor_Gloves_T06_Rogue", "Study - Learn Duskwatcher Gloves"},
            { "Tech_Armor_Gloves_T06_Scholar", "Study - Learn Dark Magus Gloves"},
            { "Tech_Armor_Gloves_T06_Warrior", "Study - Learn Blood Hunter Gloves"},
            { "Tech_Armor_Legs_T06_Brute", "Study - Learn Crimson Templar Leggings"},
            { "Tech_Armor_Legs_T06_Rogue", "Study - Learn Duskwatcher Leggings"},
            { "Tech_Armor_Legs_T06_Scholar", "Study - Learn Dark Magus Leggings"},
            { "Tech_Armor_Legs_T06_Warrior", "Study - Learn Blood Hunter Leggings"},

            // --- T08 ARMOR (Bloodmoon / Dark Silver tier) ---
            { "Tech_Armor_Boots_T08_Brute", "Athenaeum - Learn Grim Knight Boots"},
            { "Tech_Armor_Boots_T08_Rogue", "Athenaeum - Learn Shadowmoon Boots"},
            { "Tech_Armor_Boots_T08_Scholar", "Athenaeum - Learn Maleficer Scholar Boots"},
            { "Tech_Armor_Boots_T08_Warrior", "Athenaeum - Learn Dread Plate Boots"},
            { "Tech_Armor_Chest_T08_Brute", "Athenaeum - Learn Grim Knight Chestguard"},
            { "Tech_Armor_Chest_T08_Rogue", "Athenaeum - Learn Shadowmoon Chestguard"},
            { "Tech_Armor_Chest_T08_Scholar", "Athenaeum - Learn Maleficer Scholar Chestguard"},
            { "Tech_Armor_Chest_T08_Warrior", "Athenaeum - Learn Dread Plate Chestguard"},
            { "Tech_Armor_Gloves_T08_Brute", "Athenaeum - Learn Grim Knight Gloves"},
            { "Tech_Armor_Gloves_T08_Rogue", "Athenaeum - Learn Shadowmoon Gloves"},
            { "Tech_Armor_Gloves_T08_Scholar", "Athenaeum - Learn Maleficer Scholar Gloves"},
            { "Tech_Armor_Gloves_T08_Warrior", "Athenaeum - Learn Dread Plate Gloves"},
            { "Tech_Armor_Legs_T08_Brute", "Athenaeum - Learn Grim Knight Leggings"},
            { "Tech_Armor_Legs_T08_Rogue", "Athenaeum - Learn Shadowmoon Leggings"},
            { "Tech_Armor_Legs_T08_Scholar", "Athenaeum - Learn Maleficer Scholar Leggings"},
            { "Tech_Armor_Legs_T08_Warrior", "Athenaeum - Learn Dread Plate Leggings"},

            // --- CONSUMABLES ---
            { "Tech_Consumable_FireResistance_Canteen_T01", "Study - Learn Fire Resistance Brew"},
            { "Tech_Consumable_GarlicResistance_GlassBottle_T02", "Study - Learn Garlic Resistance Potion"},
            { "Tech_Consumable_PhysicalBrew_Canteen_T01", "Research Desk - Learn Brew of Ferocity"},
            { "Tech_Consumable_PhysicalBrew_Potion_T03", "Athenaeum - Learn Potion of Rage"},
            { "Tech_Consumable_RoseHealing_Canteen_T01", "Research Desk - Learn Blood Rose Brew"},
            { "Tech_Consumable_SilverBrew_Canteen_T01", "Research Desk - Learn Silver Resistance Brew"},
            { "Tech_Consumable_SpellBrew_Canteen_T01", "Research Desk - Learn Enchanted Brew"},
            { "Tech_Consumable_SpellBrew_Potion_T03", "Athenaeum - Learn Witch Potion" },
            { "Tech_Consumable_SunResistance_Canteen_T01", "Research Desk - Learn Minor Sun Resistance Brew"},
            { "Tech_Consumable_WranglersTea_GlassBottle_T01", "Study - Learn Wrangler's Potion"},

            // --- FLOOR TILES ---
            { "Tech_Floor_AlchemyLab", "Research Desk - Learn Alchemy Lab Flooring"},
            { "Tech_Floor_Crypt", "Goreswine the Ravager (Tomb)"}, // [?]
            { "Tech_Floor_Forge", "Research Desk - Learn Forge Flooring"},
            { "Tech_Floor_Jewelcrafting", "Errol the Stonebreaker (Material and Gem Storage)"}, // [?]
            { "Tech_Floor_Library", "Study - Learn Library Flooring"},
            { "Tech_Floor_Prison", "Vincent the Frostbringer (Prison Framework)"},// [?]
            { "Tech_Floor_Tailor", "Study - Learn Tailor's Flooring"},

            // --- LIQUID STATION ---
            { "Tech_LiquidStatiom_Water_Well01", "Research Desk - Learn Water Well"}, // [?]
            // --- MAGIC SOURCES (RINGS / AMULETS) ---
            // T01
            //{ "Tech_MagicSource_General_T01_BoneRing", "Errol the Stonebreaker (Material and Gem Storage)"},
            // T04 rings
            { "Tech_MagicSource_General_T04_Duskwatcher", "Research Desk - Learn Ring of the Duskwatcher"},
            { "Tech_MagicSource_General_T04_EmberChain", "Research Desk - Learn Ring of the Dawnrunner"},
            { "Tech_MagicSource_General_T04_FrozenEye", "Research Desk - Learn Ring of the Warrior"},
            { "Tech_MagicSource_General_T04_KnightRing", "Research Desk - Learn Ring of the Warrior"},
            { "Tech_MagicSource_General_T04_MistSignet", "Research Desk - Learn Ring of the Spellweaver"},
            { "Tech_MagicSource_General_T04_SorcererRing", "Research Desk - Learn Ring of the Sorcerer"}, 
            // T06 amulets/necklaces
            { "Tech_MagicSource_General_T06_AmethystPendant", "Study - Learn Pendant of the Sorcerer"},
            { "Tech_MagicSource_General_T06_EmeraldNecklace", "Study - Learn Pendant of the Dawnrunner"},
            { "Tech_MagicSource_General_T06_MistStoneNecklace", "Study - Learn Pendant of the Spellweaver"},
            { "Tech_MagicSource_General_T06_RubyPendant", "Study - Learn Pendant of the Warrior"},
            { "Tech_MagicSource_General_T06_SapphirePendant", "Study - Learn Pendant of the Warlock"},
            { "Tech_MagicSource_General_T06_TopazAmulet", "Study - Learn Pendant of the Duskwatcher"}, 
            // T08 relics - this whole section was reworked, I'm gambling that these match
            { "Tech_MagicSource_General_T08_Beast", "Athenaeum - Learn Amulet of the Blademaster"},
            { "Tech_MagicSource_General_T08_CrimsonSky", "Athenaeum - Learn Amulet of the Crimson Commander"},
            { "Tech_MagicSource_General_T08_Delusion", "Athenaeum - Learn Amulet of the Master Spellweaver"},
            { "Tech_MagicSource_General_T08_FrozenCrypt", "Athenaeum - Learn Amulet of the Arch-Warlock"},
            { "Tech_MagicSource_General_T08_Madness", "Athenaeum - Learn Amulet of the Unyielding Charger"},
            { "Tech_MagicSource_General_T08_WickedProphet", "Athenaeum - Learn Amulet of the Wicked Prophet"},
            // --- WEAPONS ---
            // T04 = Merciless Copper
            { "Tech_Weapon_Axe_T04", "Research Desk - Learn Merciless Copper Axes"}, 
            { "Tech_Weapon_Crossbow_T04", "Research Desk - Learn Merciless Copper Crossbow"},
            { "Tech_Weapon_Longbow_T04", "Research Desk - Learn Merciless Copper Longbow"},
            { "Tech_Weapon_Mace_T04", "Research Desk - Learn Merciless Copper Mace"},       
            { "Tech_Weapon_Reaper_T04", ""}, // Don't think this exists
            { "Tech_Weapon_Slashers_T04", ""},// Don't think this exists
            { "Tech_Weapon_Spear_T04", "Research Desk - Learn Merciless Copper Spear"},
            { "Tech_Weapon_Sword_T04", "Research Desk - Learn Merciless Copper Sword"},

            // T06 = Merciless Iron
            { "Tech_Weapon_Axe_T06", "Study - Learn Merciless Iron Axes"},
            { "Tech_Weapon_Claws_T06", "Study - Learn Merciless Iron Claws"},
            { "Tech_Weapon_Crossbow_T06", "Study - Learn Merciless Iron Crossbow"},
            { "Tech_Weapon_Daggers_T06", "Study - Learn Merciless Iron Daggers"},
            { "Tech_Weapon_GreatSword_T06", "Study - Learn Merciless Iron Greatsword"},
            { "Tech_Weapon_Longbow_T06", "Study - Learn Merciless Iron Longbow"},
            { "Tech_Weapon_Mace_T06", "Study - Learn Merciless Iron Mace"},
            { "Tech_Weapon_Pistols_T06", "Study - Learn Merciless Iron Pistols"},
            { "Tech_Weapon_Reaper_T06", "Study - Learn Merciless Iron Reaper"},
            { "Tech_Weapon_Slashers_T06", "Study - Learn Merciless Iron Slashers"},
            { "Tech_Weapon_Spear_T06", "Study - Learn Merciless Iron Spear"},
            { "Tech_Weapon_Sword_T06", "Study - Learn Merciless Iron Sword"},
            { "Tech_Weapon_TwinBlades_T06", "Study - Learn Merciless Iron Twinblade"},
            { "Tech_Weapon_Whip_T06", "Study - Learn Merciless Iron Whip"},

            // T08 = Dark Silver
            { "Tech_Weapon_Axe_T08", "Athenaeum - Learn Sanguine Axes"},
            { "Tech_Weapon_Claws_T08", "Athenaeum - Learn Sanguine Claws"},
            { "Tech_Weapon_Crossbow_T08", "Athenaeum - Learn Sanguine Crossbow"}, 
            { "Tech_Weapon_Daggers_T08", "Athenaeum - Learn Sanguine Daggers"},   
            { "Tech_Weapon_GreatSword_T08", "Athenaeum - Learn Sanguine Greatsword"},  
            { "Tech_Weapon_Longbow_T08", "Athenaeum - Learn Sanguine Longbow"}, 
            { "Tech_Weapon_Mace_T08", "Athenaeum - Learn Sanguine Mace"},     
            { "Tech_Weapon_Pistols_T08", "Athenaeum - Learn Sanguine Pistols"},    
            { "Tech_Weapon_Reaper_T08", "Athenaeum - Learn Sanguine Reaper"},        
            { "Tech_Weapon_Slashers_T08", "Athenaeum - Learn Sanguine Slashers"},
            { "Tech_Weapon_Spear_T08", "Athenaeum - Learn Sanguine Spear"},              
            { "Tech_Weapon_Sword_T08", "Athenaeum - Learn Sanguine Sword"}, 
            { "Tech_Weapon_TwinBlades_T08", "Athenaeum - Learn Sanguine Twinblade"},
            { "Tech_Weapon_Whip_T08", ""},// (I think this is just from the T06 whip tech, but confirm if there's a unique unlock for the T08 whip)
        };

        public static readonly Dictionary<string, string> APLocationToEntityName = EntityNameToAPLocation.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public static readonly Dictionary<string, PrefabGUID> TechToPrefab = new Dictionary<string, PrefabGUID>
            {
                { "Tech_Ability_CommandingForm", new PrefabGUID(-1301155150) },
                { "Tech_Ability_PsychicForm", new PrefabGUID(-647200166) },
                { "Tech_Armor_Boots_T04_Brute", new PrefabGUID(676266407) },
                { "Tech_Armor_Boots_T04_Rogue", new PrefabGUID(2120172621) },
                { "Tech_Armor_Boots_T04_Scholar", new PrefabGUID(1906599762) },
                { "Tech_Armor_Boots_T04_Warrior", new PrefabGUID(1879028083) },
                { "Tech_Armor_Boots_T06_Brute", new PrefabGUID(-867568357) },
                { "Tech_Armor_Boots_T06_Rogue", new PrefabGUID(399247086) },
                { "Tech_Armor_Boots_T06_Scholar", new PrefabGUID(-2051781325) },
                { "Tech_Armor_Boots_T06_Warrior", new PrefabGUID(-2023969604) },
                { "Tech_Armor_Boots_T08_Brute", new PrefabGUID(1941997114) },
                { "Tech_Armor_Boots_T08_Rogue", new PrefabGUID(-592100304) },
                { "Tech_Armor_Boots_T08_Scholar", new PrefabGUID(-1816818535) },
                { "Tech_Armor_Boots_T08_Warrior", new PrefabGUID(1673193738) },
                { "Tech_Armor_Chest_T04_Brute", new PrefabGUID(-178432582) },
                { "Tech_Armor_Chest_T04_Rogue", new PrefabGUID(1868487918) },
                { "Tech_Armor_Chest_T04_Scholar", new PrefabGUID(755372402) },
                { "Tech_Armor_Chest_T04_Warrior", new PrefabGUID(657926195) },
                { "Tech_Armor_Chest_T06_Brute", new PrefabGUID(320958383) },
                { "Tech_Armor_Chest_T06_Rogue", new PrefabGUID(-962794065) },
                { "Tech_Armor_Chest_T06_Scholar", new PrefabGUID(1291904224) },
                { "Tech_Armor_Chest_T06_Warrior", new PrefabGUID(-1191678823) },
                { "Tech_Armor_Chest_T08_Brute", new PrefabGUID(750468260) },
                { "Tech_Armor_Chest_T08_Rogue", new PrefabGUID(-1170753047) },
                { "Tech_Armor_Chest_T08_Scholar", new PrefabGUID(1188570352) },
                { "Tech_Armor_Chest_T08_Warrior", new PrefabGUID(-1435202677) },
                { "Tech_Armor_Gloves_T04_Brute", new PrefabGUID(935228271) },
                { "Tech_Armor_Gloves_T04_Rogue", new PrefabGUID(1577296935) },
                { "Tech_Armor_Gloves_T04_Scholar", new PrefabGUID(-1611749320) },
                { "Tech_Armor_Gloves_T04_Warrior", new PrefabGUID(2125292818) },
                { "Tech_Armor_Gloves_T06_Brute", new PrefabGUID(1232232420) },
                { "Tech_Armor_Gloves_T06_Rogue", new PrefabGUID(-1831064302) },
                { "Tech_Armor_Gloves_T06_Scholar", new PrefabGUID(-2034432336) },
                { "Tech_Armor_Gloves_T06_Warrior", new PrefabGUID(-215891793) },
                { "Tech_Armor_Gloves_T08_Brute", new PrefabGUID(-371547835) },
                { "Tech_Armor_Gloves_T08_Rogue", new PrefabGUID(693752504) },
                { "Tech_Armor_Gloves_T08_Scholar", new PrefabGUID(-2038786647) },
                { "Tech_Armor_Gloves_T08_Warrior", new PrefabGUID(-1696823248) },
                { "Tech_Armor_Legs_T04_Brute", new PrefabGUID(352798374) },
                { "Tech_Armor_Legs_T04_Rogue", new PrefabGUID(-366570135) },
                { "Tech_Armor_Legs_T04_Scholar", new PrefabGUID(-1510681319) },
                { "Tech_Armor_Legs_T04_Warrior", new PrefabGUID(-466068499) },
                { "Tech_Armor_Legs_T06_Brute", new PrefabGUID(754248969) },
                { "Tech_Armor_Legs_T06_Rogue", new PrefabGUID(1811127257) },
                { "Tech_Armor_Legs_T06_Scholar", new PrefabGUID(-1857837378) },
                { "Tech_Armor_Legs_T06_Warrior", new PrefabGUID(-1866364260) },
                { "Tech_Armor_Legs_T08_Brute", new PrefabGUID(-996734096) },
                { "Tech_Armor_Legs_T08_Rogue", new PrefabGUID(419994007) },
                { "Tech_Armor_Legs_T08_Scholar", new PrefabGUID(1190578873) },
                { "Tech_Armor_Legs_T08_Warrior", new PrefabGUID(1738492884) },
                { "Tech_Collection_Armor_T01_BoneAll", new PrefabGUID(-347160774) },
                { "Tech_Collection_Armor_T01_BoneLower", new PrefabGUID(376701344) },
                { "Tech_Collection_Armor_T01_BoneUpper_Salve", new PrefabGUID(-1657036518) },
                { "Tech_Collection_ArtisansCorner", new PrefabGUID(2022418671) },
                { "Tech_Collection_BloodTracking", new PrefabGUID(1396665832) },
                { "Tech_Collection_Brazier_01", new PrefabGUID(1789166952) },
                { "Tech_Collection_Braziers02", new PrefabGUID(1928348865) },
                { "Tech_Collection_Braziers03", new PrefabGUID(-1841292034) },
                { "Tech_Collection_BustStatues_Vampire", new PrefabGUID(-1045980026) },
                { "Tech_Collection_Candles_T02", new PrefabGUID(1982396745) },
                { "Tech_Collection_CandleStands_T01", new PrefabGUID(-1032220214) },
                { "Tech_Collection_CandleStands_T02", new PrefabGUID(1186146964) },
                { "Tech_Collection_Carpet_Ostenstatious", new PrefabGUID(377882346) },
                { "Tech_Collection_Carpet_Stately", new PrefabGUID(-1363487077) },
                { "Tech_Collection_Carpet_T01", new PrefabGUID(971588976) },
                { "Tech_Collection_Carpet_T02", new PrefabGUID(-59044434) },
                { "Tech_Collection_Carpet_T03", new PrefabGUID(-166104992) },
                { "Tech_Collection_Castle_Paintings", new PrefabGUID(698575581) },
                { "Tech_Collection_Castle_Windows_T01", new PrefabGUID(-1270150309) },
                { "Tech_Collection_Clocks", new PrefabGUID(-1847627135) },
                { "Tech_Collection_CoatOfArms", new PrefabGUID(-1637580865) },
                { "Tech_Collection_Dracula_Armor_Boots", new PrefabGUID(-1000381969) },
                { "Tech_Collection_Dracula_Armor_Chest", new PrefabGUID(-784761022) },
                { "Tech_Collection_Dracula_Armor_Gloves", new PrefabGUID(1518908715) },
                { "Tech_Collection_Dracula_Armor_Legs", new PrefabGUID(-997181076) },
                { "Tech_Collection_Fence_Rural", new PrefabGUID(1194397556) },
                { "Tech_Collection_Fence_Verdant", new PrefabGUID(-4950312) },
                { "Tech_Collection_FenceFountain_Noble_T02", new PrefabGUID(-1776617088) },
                { "Tech_Collection_Fireplaces", new PrefabGUID(-820554605) },
                { "Tech_Collection_FlyingCandles_T03", new PrefabGUID(1863270378) },
                { "Tech_Collection_Fountain_T03", new PrefabGUID(-1296064423) },
                { "Tech_Collection_Framework_CastleHeart", new PrefabGUID(-1377692981) },
                { "Tech_Collection_Framework_Gargoyles_and_DLC", new PrefabGUID(-2118176472) },
                { "Tech_Collection_Framework_T01_Wood_Foundation", new PrefabGUID(-219528846) },
                { "Tech_Collection_Framework_T01_Wood_Structures", new PrefabGUID(-1010843571) },
                { "Tech_Collection_Framework_T02_Stone", new PrefabGUID(1908774048) },
                { "Tech_Collection_Framework_T02_Stone_DLC_Dracula", new PrefabGUID(-447888984) },
                { "Tech_Collection_Framework_T02_Stone_DLC_Gloomrot", new PrefabGUID(-1485413430) },
                { "Tech_Collection_Framework_T02_Stone_DLC_ProjectK", new PrefabGUID(763319653) },
                { "Tech_Collection_Framework_T02_Stone_DLC_Strongblade", new PrefabGUID(-2146477131) },
                { "Tech_Collection_Framework_T02_Stone_Halloween2022", new PrefabGUID(-729522822) },
                { "Tech_Collection_FrayedDecor", new PrefabGUID(524373519) },
                { "Tech_Collection_Furniture_Chairs_T02", new PrefabGUID(-1393827852) },
                { "Tech_Collection_Furniture_Desk_Chairs_T02", new PrefabGUID(-398123938) },
                { "Tech_Collection_Furniture_Desks_T02", new PrefabGUID(-1949258784) },
                { "Tech_Collection_Furniture_DressingTables_T03", new PrefabGUID(-1708158238) },
                { "Tech_Collection_Furniture_Simple_T01", new PrefabGUID(1549360318) },
                { "Tech_Collection_Furniture_Sofas_T03", new PrefabGUID(1333706297) },
                { "Tech_Collection_Furniture_Tables_T02", new PrefabGUID(1970685025) },
                { "Tech_Collection_Garden_Furniture_Lanterns_T02", new PrefabGUID(-547241268) },
                { "Tech_Collection_Garden_Hedges_T02", new PrefabGUID(1158190765) },
                { "Tech_Collection_Garden_PlantersDecor_T01", new PrefabGUID(-3886426) },
                { "Tech_Collection_Garden_PlantersDecor_T02", new PrefabGUID(1643488994) },
                { "Tech_Collection_Garden_PlantersDecor_T03", new PrefabGUID(303243522) },
                { "Tech_Collection_HangingLanterns_T02", new PrefabGUID(-986883360) },
                { "Tech_Collection_Light_GardenLampPosts_T03", new PrefabGUID(-732367543) },
                { "Tech_Collection_Mirrors", new PrefabGUID(437493175) },
                { "Tech_Collection_Mirrors_Halloween2022", new PrefabGUID(153177445) },
                { "Tech_Collection_Ornaments_Banners_T02", new PrefabGUID(2134771043) },
                { "Tech_Collection_Ornaments_Stone_T01", new PrefabGUID(498212120) },
                { "Tech_Collection_Outdoor_Floors", new PrefabGUID(1426681031) },
                { "Tech_Collection_Pavement_Cobblestone", new PrefabGUID(1029462093) },
                { "Tech_Collection_Pavement_Dirt", new PrefabGUID(2080238877) },
                { "Tech_Collection_Salve", new PrefabGUID(-1279942095) },
                { "Tech_Collection_Stables_Furnishing", new PrefabGUID(2128476081) },
                { "Tech_Collection_Statues_Vampire", new PrefabGUID(192335890) },
                { "Tech_Collection_Structures_T01", new PrefabGUID(-998624122) },
                { "Tech_Collection_Vases01", new PrefabGUID(1449355366) },
                { "Tech_Collection_VBlood_ArenaDecorations", new PrefabGUID(959981888) },
                { "Tech_Collection_VBlood_CrystalLamp", new PrefabGUID(1120119195) },
                { "Tech_Collection_VBlood_JewelsT04", new PrefabGUID(514472877) },
                { "Tech_Collection_VBlood_T02_AlphaWolf", new PrefabGUID(-1031733757) },
                { "Tech_Collection_VBlood_T02_Keely_Armor", new PrefabGUID(537293692) },
                { "Tech_Collection_VBlood_T02_KeelyFrostArrow", new PrefabGUID(792292662) },
                { "Tech_Collection_VBlood_T02_KeelyFrostArrow_Canteen", new PrefabGUID(1500994622) },
                { "Tech_Collection_VBlood_T02_LidiaChaosArrow", new PrefabGUID(873587326) },
                { "Tech_Collection_VBlood_T02_LidiaChaosArrow_Longbow", new PrefabGUID(-272307342) },
                { "Tech_Collection_VBlood_T02_RufusForeman", new PrefabGUID(-1375294270) },
                { "Tech_Collection_VBlood_T02_RufusForeman_Crossbow", new PrefabGUID(1080245850) },
                { "Tech_Collection_VBlood_T02_StoneBreaker", new PrefabGUID(-1230635663) },
                { "Tech_Collection_VBlood_T02_StoneBreaker_CopperWeapons", new PrefabGUID(-681604451) },
                { "Tech_Collection_VBlood_T03_Fisherman", new PrefabGUID(-1955769660) },
                { "Tech_Collection_VBlood_T03_Goreswine", new PrefabGUID(503108272) },
                { "Tech_Collection_VBlood_T03_Goreswine_MagicSource", new PrefabGUID(-934771037) },
                { "Tech_Collection_VBlood_T03_Goreswine_Units", new PrefabGUID(1226331752) },
                { "Tech_Collection_VBlood_T03_GoreswineFence", new PrefabGUID(-1852229711) },
                { "Tech_Collection_VBlood_T03_Grayson", new PrefabGUID(-819263071) },
                { "Tech_Collection_VBlood_T03_Grayson_Floors", new PrefabGUID(613260763) },
                { "Tech_Collection_VBlood_T03_Grayson_TargetDummies", new PrefabGUID(-1484059874) },
                { "Tech_Collection_VBlood_T03_PutridRat", new PrefabGUID(-482440851) },
                { "Tech_Collection_VBlood_T04_Ball_Cosmetic", new PrefabGUID(-1140095741) },
                { "Tech_Collection_VBlood_T04_CliveTheFirestarter", new PrefabGUID(1165734787) },
                { "Tech_Collection_VBlood_T04_FrostGuard", new PrefabGUID(627388312) },
                { "Tech_Collection_VBlood_T04_FrostGuard_PrisonFramework", new PrefabGUID(1286356808) },
                { "Tech_Collection_VBlood_T04_NicholausTheFallen", new PrefabGUID(656825307) },
                { "Tech_Collection_VBlood_T04_NicholausTheFallen_MagicSource", new PrefabGUID(189289255) },
                { "Tech_Collection_VBlood_T04_Poloma", new PrefabGUID(-1138769053) },
                { "Tech_Collection_VBlood_T04_PolomaGardenFloors", new PrefabGUID(196684992) },
                { "Tech_Collection_VBlood_T04_PolomaPlantersExterior", new PrefabGUID(35536792) },
                { "Tech_Collection_VBlood_T04_PolomaPlantersInterior", new PrefabGUID(6233001) },
                { "Tech_Collection_VBlood_T04_Quincey", new PrefabGUID(798280158) },
                { "Tech_Collection_VBlood_T04_Quincey_CottonArmor", new PrefabGUID(981172472) },
                { "Tech_Collection_VBlood_T04_Quincey_Decoration", new PrefabGUID(-1204804925) },
                { "Tech_Collection_VBlood_T04_Quincey_IronWeapons", new PrefabGUID(1452185822) },
                { "Tech_Collection_VBlood_T04_Tailor", new PrefabGUID(1748243393) },
                { "Tech_Collection_VBlood_T04_Tailor_Curtains", new PrefabGUID(-1291119657) },
                { "Tech_Collection_VBlood_T04_VampireHunter", new PrefabGUID(1812666586) },
                { "Tech_Collection_VBlood_T04_VampireHunter_GreatSword", new PrefabGUID(2076894288) },
                { "Tech_Collection_VBlood_T04_Wendigo", new PrefabGUID(43505638) },
                { "Tech_Collection_VBlood_T04_Wendigo_Claws", new PrefabGUID(803940486) },
                { "Tech_Collection_VBlood_T05_ArenaChampion", new PrefabGUID(-2144827499) },
                { "Tech_Collection_VBlood_T05_ArenaChampion_TwinBlades", new PrefabGUID(1469248266) },
                { "Tech_Collection_VBlood_T05_BishopOfShadow", new PrefabGUID(1795449585) },
                { "Tech_Collection_VBlood_T05_Fabian", new PrefabGUID(-1659396939) },
                { "Tech_Collection_VBlood_T05_FerociousBear", new PrefabGUID(1481816999) },
                { "Tech_Collection_VBlood_T05_FerociousBear_Rugs", new PrefabGUID(-979296135) },
                { "Tech_Collection_VBlood_T05_GlassBlower", new PrefabGUID(836932829) },
                { "Tech_Collection_VBlood_T05_Golem", new PrefabGUID(292267578) },
                { "Tech_Collection_VBlood_T05_GolemGems", new PrefabGUID(-1074691879) },
                { "Tech_Collection_VBlood_T05_GolemObsidian", new PrefabGUID(-318857592) },
                { "Tech_Collection_VBlood_T05_HolyNun", new PrefabGUID(1509833074) },
                { "Tech_Collection_VBlood_T05_IceRanger", new PrefabGUID(-1286478056) },
                { "Tech_Collection_VBlood_T05_Infiltrator", new PrefabGUID(1499385606) },
                { "Tech_Collection_VBlood_T05_Infiltrator_Daggers", new PrefabGUID(1521034384) },
                { "Tech_Collection_VBlood_T05_Meredith", new PrefabGUID(-124171135) },
                { "Tech_Collection_VBlood_T05_Scribe", new PrefabGUID(1096972300) },
                { "Tech_Collection_VBlood_T05_UndeadLeader", new PrefabGUID(684401761) },
                { "Tech_Collection_VBlood_T05_UndeadLeader_Bells", new PrefabGUID(-1062774694) },
                { "Tech_Collection_VBlood_T05_UndeadLeader_Reaper", new PrefabGUID(-1631949089) },
                { "Tech_Collection_VBlood_T06_Armor_Silk", new PrefabGUID(574648849) },
                { "Tech_Collection_VBlood_T06_Castleman", new PrefabGUID(963388509) },
                { "Tech_Collection_VBlood_T06_Castleman_SanguineWhip", new PrefabGUID(499979034) },
                { "Tech_Collection_VBlood_T06_CursedWanderer", new PrefabGUID(-608704270) },
                { "Tech_Collection_VBlood_T06_HeadlessHorseman", new PrefabGUID(1993827563) },
                { "Tech_Collection_VBlood_T06_HighLord", new PrefabGUID(2003830913) },
                { "Tech_Collection_VBlood_T06_Iva", new PrefabGUID(-1168862239) },
                { "Tech_Collection_VBlood_T06_Jade", new PrefabGUID(-1809787416) },
                { "Tech_Collection_VBlood_T06_Jade_Pistols", new PrefabGUID(-1329550886) },
                { "Tech_Collection_VBlood_T06_MilitiaCommander", new PrefabGUID(-1747434949) },
                { "Tech_Collection_VBlood_T06_MilitiaCommander_GreatSword", new PrefabGUID(1430937326) },
                { "Tech_Collection_VBlood_T06_MilitiaCommander_Weapons", new PrefabGUID(252901819) },
                { "Tech_Collection_VBlood_T06_MilitiaCommander_WideGate", new PrefabGUID(2053355293) },
                { "Tech_Collection_VBlood_T06_Purifier", new PrefabGUID(-794945828) },
                { "Tech_Collection_VBlood_T06_Purifier_Gruel", new PrefabGUID(-513542593) },
                { "Tech_Collection_VBlood_T06_Raziel", new PrefabGUID(177893377) },
                { "Tech_Collection_VBlood_T06_Raziel_Armor", new PrefabGUID(1383473060) },
                { "Tech_Collection_VBlood_T06_Raziel_Decoration", new PrefabGUID(961368319) },
                { "Tech_Collection_VBlood_T06_Raziel_MagicSource", new PrefabGUID(1008496220) },
                { "Tech_Collection_VBlood_T06_SpiderQueen", new PrefabGUID(693361325) },
                { "Tech_Collection_VBlood_T06_ToadKing", new PrefabGUID(1846992402) },
                { "Tech_Collection_VBlood_T06_ToadKing_Coins", new PrefabGUID(-837551957) },
                { "Tech_Collection_VBlood_T06_Voltage", new PrefabGUID(228678537) },
                { "Tech_Collection_VBlood_T06_Voltage_Teleporters", new PrefabGUID(902743573) },
                { "Tech_Collection_VBlood_T06_Werewolf", new PrefabGUID(-330347291) },
                { "Tech_Collection_VBlood_T06_Whip", new PrefabGUID(142799020) },
                { "Tech_Collection_VBlood_T06_Yeti", new PrefabGUID(-1435750586) },
                { "Tech_Collection_VBlood_T07_Archmage", new PrefabGUID(26064539) },
                { "Tech_Collection_VBlood_T07_Archmage_JewelsT03", new PrefabGUID(-942401074) },
                { "Tech_Collection_VBlood_T07_CardinalPriest", new PrefabGUID(1270259509) },
                { "Tech_Collection_VBlood_T07_Carver", new PrefabGUID(1092543000) },
                { "Tech_Collection_VBlood_T07_Carver_Coating", new PrefabGUID(-1318816407) },
                { "Tech_Collection_VBlood_T07_CursedSmith", new PrefabGUID(-1633289177) },
                { "Tech_Collection_VBlood_T07_CursedSmith_MagicSource", new PrefabGUID(-778842690) },
                { "Tech_Collection_VBlood_T07_Gerard", new PrefabGUID(701618642) },
                { "Tech_Collection_VBlood_T07_Harpy", new PrefabGUID(56158045) },
                { "Tech_Collection_VBlood_T07_HarpyGems", new PrefabGUID(-477566763) },
                { "Tech_Collection_VBlood_T07_Livith", new PrefabGUID(1851932061) },
                { "Tech_Collection_VBlood_T07_Livith_Slashers", new PrefabGUID(1262310143) },
                { "Tech_Collection_VBlood_T07_Lucie", new PrefabGUID(-1642797098) },
                { "Tech_Collection_VBlood_T07_Lucie_Elixirs", new PrefabGUID(-367655643) },
                { "Tech_Collection_VBlood_T07_Overseer", new PrefabGUID(-1652178969) },
                { "Tech_Collection_VBlood_T07_Professor", new PrefabGUID(-1199699442) },
                { "Tech_Collection_VBlood_T07_RailgunSergeant", new PrefabGUID(-1166562965) },
                { "Tech_Collection_VBlood_T07_Sommelier", new PrefabGUID(2086705669) },
                { "Tech_Collection_VBlood_T07_Sommelier_Bloodwine", new PrefabGUID(-797717364) },
                { "Tech_Collection_VBlood_T07_Sommelier_MagicSource", new PrefabGUID(134700304) },
                { "Tech_Collection_VBlood_T07_Valyr", new PrefabGUID(-1224541374) },
                { "Tech_Collection_VBlood_T07_Witch", new PrefabGUID(739063686) },
                { "Tech_Collection_VBlood_T07_ZealousCultist", new PrefabGUID(-242619336) },
                { "Tech_Collection_VBlood_T08_BatVampire", new PrefabGUID(644170529) },
                { "Tech_Collection_VBlood_T08_Behemoth", new PrefabGUID(-2059047777) },
                { "Tech_Collection_VBlood_T08_BloodKnight", new PrefabGUID(90328407) },
                { "Tech_Collection_VBlood_T08_Dracula", new PrefabGUID(956657660) },
                { "Tech_Collection_VBlood_T08_Manticore", new PrefabGUID(28013331) },
                { "Tech_Collection_VBlood_T08_Monster", new PrefabGUID(-1579828449) },
                { "Tech_Collection_VBlood_T08_Morgana", new PrefabGUID(-1902462638) },
                { "Tech_Collection_VBlood_T08_Paladin", new PrefabGUID(-1627795309) },
                { "Tech_Collection_WallHangingMirrors_T02", new PrefabGUID(-505308287) },
                { "Tech_Collection_WallHangingShelves_T02", new PrefabGUID(1738542265) },
                { "Tech_Collection_Wallpapers_Bricks01", new PrefabGUID(-1471943197) },
                { "Tech_Collection_Wallpapers_Classical01", new PrefabGUID(1405536938) },
                { "Tech_Collection_Wallpapers_Cordial01", new PrefabGUID(-1047045523) },
                { "Tech_Collection_Wallpapers_Imperious01", new PrefabGUID(302835542) },
                { "Tech_Collection_Wallpapers_Prison01", new PrefabGUID(1345992385) },
                { "Tech_Collection_Wallpapers_Stone01", new PrefabGUID(2087478902) },
                { "Tech_Collection_Wallpapers_WoodPanel01", new PrefabGUID(649753246) },
                { "Tech_Collection_Waypoint", new PrefabGUID(-1380817858) },
                { "Tech_Collection_Workshop_Decoration", new PrefabGUID(-1212695091) },
                { "Tech_Consumable_FireResistance_Canteen_T01", new PrefabGUID(844019492) },
                { "Tech_Consumable_GarlicResistance_GlassBottle_T02", new PrefabGUID(1329925559) },
                { "Tech_Consumable_PhysicalBrew_Canteen_T01", new PrefabGUID(-537813334) },
                { "Tech_Consumable_PhysicalBrew_Potion_T03", new PrefabGUID(-11827994) },
                { "Tech_Consumable_RoseHealing_Canteen_T01", new PrefabGUID(70753269) },
                { "Tech_Consumable_SilverBrew_Canteen_T01", new PrefabGUID(-845792723) },
                { "Tech_Consumable_SpellBrew_Canteen_T01", new PrefabGUID(1934754319) },
                { "Tech_Consumable_SpellBrew_Potion_T03", new PrefabGUID(-446090633) },
                { "Tech_Consumable_SunResistance_Canteen_T01", new PrefabGUID(-680166074) },
                { "Tech_Consumable_WranglersTea_GlassBottle_T01", new PrefabGUID(-1805160530) },
                { "Tech_Curtains_T03_Royal", new PrefabGUID(638346506) },
                { "Tech_Floor_AlchemyLab", new PrefabGUID(-55882446) },
                { "Tech_Floor_Crypt", new PrefabGUID(308890669) },
                { "Tech_Floor_Forge", new PrefabGUID(-508159781) },
                { "Tech_Floor_Jewelcrafting", new PrefabGUID(414215710) },
                { "Tech_Floor_Library", new PrefabGUID(-1851035383) },
                { "Tech_Floor_Prison", new PrefabGUID(-876687572) },
                { "Tech_Floor_Tailor", new PrefabGUID(-49029138) },
                { "Tech_Floor_Treasury", new PrefabGUID(833988507) },
                { "Tech_LiquidStatiom_Water_Well01", new PrefabGUID(-554785122) },
                { "Tech_MagicSource_General_T01_BoneRing", new PrefabGUID(-1261543603) },
                { "Tech_MagicSource_General_T04_Duskwatcher", new PrefabGUID(-939673215) },
                { "Tech_MagicSource_General_T04_EmberChain", new PrefabGUID(688580986) },
                { "Tech_MagicSource_General_T04_FrozenEye", new PrefabGUID(-1183635267) },
                { "Tech_MagicSource_General_T04_KnightRing", new PrefabGUID(596593772) },
                { "Tech_MagicSource_General_T04_MistSignet", new PrefabGUID(-987554233) },
                { "Tech_MagicSource_General_T04_SorcererRing", new PrefabGUID(-1597622418) },
                { "Tech_MagicSource_General_T06_AmethystPendant", new PrefabGUID(2120978146) },
                { "Tech_MagicSource_General_T06_EmeraldNecklace", new PrefabGUID(-2004423580) },
                { "Tech_MagicSource_General_T06_MistStoneNecklace", new PrefabGUID(-678626936) },
                { "Tech_MagicSource_General_T06_RubyPendant", new PrefabGUID(75174829) },
                { "Tech_MagicSource_General_T06_SapphirePendant", new PrefabGUID(2105959904) },
                { "Tech_MagicSource_General_T06_TopazAmulet", new PrefabGUID(1266173480) },
                { "Tech_MagicSource_General_T08_Beast", new PrefabGUID(2057306510) },
                { "Tech_MagicSource_General_T08_CrimsonSky", new PrefabGUID(-1976191252) },
                { "Tech_MagicSource_General_T08_Delusion", new PrefabGUID(1827501900) },
                { "Tech_MagicSource_General_T08_FrozenCrypt", new PrefabGUID(-1618357223) },
                { "Tech_MagicSource_General_T08_Madness", new PrefabGUID(438329698) },
                { "Tech_MagicSource_General_T08_WickedProphet", new PrefabGUID(1018971284) },
                { "Tech_SpellPassive_Blood_T01_BloodSpray", new PrefabGUID(-1435275335) },
                { "Tech_SpellPassive_Blood_T02_BloodTypeEfficiency", new PrefabGUID(-1672153699) },
                { "Tech_SpellPassive_Blood_T03_VBloodSlayer", new PrefabGUID(-798456253) },
                { "Tech_SpellPassive_Blood_T04_Rampage", new PrefabGUID(1425732083) },
                { "Tech_SpellPassive_Chaos_T01_ChaosKindling", new PrefabGUID(1843485926) },
                { "Tech_SpellPassive_Chaos_T02_RenewingFlames", new PrefabGUID(-1267731653) },
                { "Tech_SpellPassive_Chaos_T03_Overpower", new PrefabGUID(2009766737) },
                { "Tech_SpellPassive_Chaos_T04_RavenousStrikes", new PrefabGUID(1027528474) },
                { "Tech_SpellPassive_Frost_T01_ColdSoul", new PrefabGUID(-1243503477) },
                { "Tech_SpellPassive_Frost_T02_ChillWeave", new PrefabGUID(-634698501) },
                { "Tech_SpellPassive_Frost_T03_Bastion", new PrefabGUID(167793585) },
                { "Tech_SpellPassive_Frost_T04_DarkEnchantment", new PrefabGUID(-476299746) },
                { "Tech_SpellPassive_Illusion_T01_SpiritualInfusion", new PrefabGUID(443034615) },
                { "Tech_SpellPassive_Illusion_T02_FlowingSorcery", new PrefabGUID(1166977128) },
                { "Tech_SpellPassive_Illusion_T03_FeralHaste", new PrefabGUID(87181825) },
                { "Tech_SpellPassive_Illusion_T04_WickedPower", new PrefabGUID(839015128) },
                { "Tech_SpellPassive_Storm_T01_LightningFastStrikes", new PrefabGUID(216926005) },
                { "Tech_SpellPassive_Storm_T02_EnhancedConductivity", new PrefabGUID(-421268939) },
                { "Tech_SpellPassive_Storm_T03_HungerForPower", new PrefabGUID(416186705) },
                { "Tech_SpellPassive_Storm_T04_TurbulentVelocity", new PrefabGUID(-6320984) },
                { "Tech_SpellPassive_Unholy_T01_ArcaneAnimator", new PrefabGUID(-759918389) },
                { "Tech_SpellPassive_Unholy_T02_SoulDrinker", new PrefabGUID(1841694629) },
                { "Tech_SpellPassive_Unholy_T03_LethalStrikes", new PrefabGUID(1930313893) },
                { "Tech_SpellPassive_Unholy_T04_EmbraceMayhem", new PrefabGUID(550246163) },
                { "Tech_Storage_Alchemy_T02", new PrefabGUID(1517382260) },
                { "Tech_Storage_Alchemy_T03", new PrefabGUID(1625843968) },
                { "Tech_Storage_Armor_T02", new PrefabGUID(1081189242) },
                { "Tech_Storage_Blood_T02", new PrefabGUID(977509050) },
                { "Tech_Storage_Blood_T03", new PrefabGUID(1047288877) },
                { "Tech_Storage_Coins_T02", new PrefabGUID(1415500586) },
                { "Tech_Storage_Consumable_T02", new PrefabGUID(-799314882) },
                { "Tech_Storage_Consumable_T03", new PrefabGUID(-895778945) },
                { "Tech_Storage_Fish_T02", new PrefabGUID(1464078715) },
                { "Tech_Storage_Gems_T01", new PrefabGUID(283418827) },
                { "Tech_Storage_Gems_T02", new PrefabGUID(1546021583) },
                { "Tech_Storage_Herbs_T01", new PrefabGUID(1919678927) },
                { "Tech_Storage_Herbs_T02", new PrefabGUID(-1430391301) },
                { "Tech_Storage_Jewels_T02", new PrefabGUID(1839251968) },
                { "Tech_Storage_Knowledge_T02", new PrefabGUID(-1233545962) },
                { "Tech_Storage_Knowledge_T03", new PrefabGUID(-1309377338) },
                { "Tech_Storage_Minerals_T01", new PrefabGUID(503660474) },
                { "Tech_Storage_Minerals_T02", new PrefabGUID(659306848) },
                { "Tech_Storage_Pack_Equipment_T02", new PrefabGUID(-1605857820) },
                { "Tech_Storage_Pack_T01_A", new PrefabGUID(134968992) },
                { "Tech_Storage_Pack_T01_B", new PrefabGUID(-1700135018) },
                { "Tech_Storage_Pack_T02", new PrefabGUID(-612322185) },
                { "Tech_Storage_T01", new PrefabGUID(-1065914013) },
                { "Tech_Storage_T02", new PrefabGUID(2081297006) },
                { "Tech_Storage_T03", new PrefabGUID(-527623056) },
                { "Tech_Storage_Tailoring_T02", new PrefabGUID(-1208628879) },
                { "Tech_Storage_Tailoring_T03", new PrefabGUID(1648033500) },
                { "Tech_Storage_Weapons_T02", new PrefabGUID(-917955046) },
                { "Tech_Storage_Woodworking_T01", new PrefabGUID(-1972974567) },
                { "Tech_Storage_Woodworking_T02", new PrefabGUID(1090765974) },
                { "Tech_Weapon_Axe_T04", new PrefabGUID(-632708133) },
                { "Tech_Weapon_Axe_T06", new PrefabGUID(-2012042353) },
                { "Tech_Weapon_Axe_T08", new PrefabGUID(1895745785) },
                { "Tech_Weapon_Claws_T06", new PrefabGUID(1738256866) },
                { "Tech_Weapon_Claws_T08", new PrefabGUID(574338808) },
                { "Tech_Weapon_Crossbow_T04", new PrefabGUID(-997169234) },
                { "Tech_Weapon_Crossbow_T06", new PrefabGUID(1000023879) },
                { "Tech_Weapon_Crossbow_T08", new PrefabGUID(-1333600826) },
                { "Tech_Weapon_Daggers_T06", new PrefabGUID(-1688466299) },
                { "Tech_Weapon_Daggers_T08", new PrefabGUID(867896907) },
                { "Tech_Weapon_GreatSword_T06", new PrefabGUID(175562220) },
                { "Tech_Weapon_GreatSword_T08", new PrefabGUID(-976123885) },
                { "Tech_Weapon_Longbow_T04", new PrefabGUID(-212104516) },
                { "Tech_Weapon_Longbow_T06", new PrefabGUID(-1988689265) },
                { "Tech_Weapon_Longbow_T08", new PrefabGUID(-396858635) },
                { "Tech_Weapon_Mace_T04", new PrefabGUID(507915220) },
                { "Tech_Weapon_Mace_T06", new PrefabGUID(-437562995) },
                { "Tech_Weapon_Mace_T08", new PrefabGUID(-412324833) },
                { "Tech_Weapon_Pistols_T06", new PrefabGUID(-1341416577) },
                { "Tech_Weapon_Pistols_T08", new PrefabGUID(-1917260012) },
                { "Tech_Weapon_Reaper_T04", new PrefabGUID(-1073336085) },
                { "Tech_Weapon_Reaper_T06", new PrefabGUID(1184108243) },
                { "Tech_Weapon_Reaper_T08", new PrefabGUID(-409067814) },
                { "Tech_Weapon_Slashers_T04", new PrefabGUID(-9976124) },
                { "Tech_Weapon_Slashers_T06", new PrefabGUID(-2121238754) },
                { "Tech_Weapon_Slashers_T08", new PrefabGUID(575105000) },
                { "Tech_Weapon_Spear_T04", new PrefabGUID(-54738837) },
                { "Tech_Weapon_Spear_T06", new PrefabGUID(-1396617298) },
                { "Tech_Weapon_Spear_T08", new PrefabGUID(-759663833) },
                { "Tech_Weapon_Sword_T04", new PrefabGUID(1950052883) },
                { "Tech_Weapon_Sword_T06", new PrefabGUID(-1685075160) },
                { "Tech_Weapon_Sword_T08", new PrefabGUID(361533671) },
                { "Tech_Weapon_TwinBlades_T06", new PrefabGUID(928267897) },
                { "Tech_Weapon_TwinBlades_T08", new PrefabGUID(-711409497) },
                { "Tech_Weapon_Whip_T06", new PrefabGUID(1500666524) },
                { "Tech_Weapon_Whip_T08", new PrefabGUID(1515808838) },
            };
        public static readonly Dictionary<PrefabGUID, string> PrefabToTech = TechToPrefab.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    }

}
