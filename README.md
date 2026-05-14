# Archipelago V Rising Client

## Gameplay

* Every time you start your game you must use the .connect command before doing anything else.
* When you are attempting to do research or unlock spells, you must first run the .startResearch command so your research/spells match what locaitons you've sent.
  * Failure to do this could result in duplicated location checks. 
* Once you are finished researching, use the .stopResearch command. This will set you back to the progression you've been granted.

## YAML Settings
Please use the provided yaml at https://github.com/dghalfor/V-Rising-APWorld/releases/tag/0.0.2 for the time being

## Connecting to an Archipelago Server

* .connect [slotName] [ip:port] - this connects you to the archipelago. I highly recommend starting with this when you load your game.

## Starting the game
* Install BepInEx, which is required for modding VRising. Follow the manual instructions provided at BepInEx Installation Guide to set it up correctly in your VRising game directory. https://thunderstore.io/c/v-rising/p/BepInEx/BepInExPack_V_Rising/
* Run the game once, fully launching a new world. This lets BepinEx build its library
* After downloading, open plugins.zip which contains APVrising.dll and its dependencies. Move or copy these .dll files into the BepInEx\Plugins directory within your VRising installation folder.
* After that, launch v rising and confirm you do not see any major "red errors" in the bepinex window.
* Launch your world again, it may give you a connection error the first time you do this, just reattempt to start the world.
* If you cannot get it to launch check Steam\steamapps\common\VRising\BepInEx and BepInEx_Server for the errorlogs and see if anything was logged in them. (edited)Tuesday, May 12, 2026 3:41 PM


## Changelog

## Current commands

* .connect [slotName] [ip:port] - this connects you to the archipelago. I highly recommend starting with this when you load your game.
* .startResearch - switches research progression buffers to allow for the collection of checks in your research desks (see todo)
* .stopResearch - switches progression buffers to the elements that have been unlocked by the archipelago (see todo)
* .unlockTech guid - unlocks the tech associated with a guid (see tech guids here https://wiki.vrisingmods.com/prefabs/Tech.html)
* .lockTech guid - locks tech associated with a guid (see tech guids here https://wiki.vrisingmods.com/prefabs/Tech.html)

## Known Issues

* There is a big lag spike shortly after launching the server. (only tested in host+play)
* Client entity manager getting hit on server logs during locks post sync
* DLC variants of shapeshifts bypass the archipelago logic and will remain attached to the player
* Killing V bloods will result in a "could not send locaiton check" error. This is intermittent and still needs to be diagnosed.
* You can drag spells you've unlocked while in research mode to your hotbar and they will not be removed. 
* Until you use .connect you will be able to use techs that should be locked.

## To-do
* Elements removed from research at runtime are re-added after saving and closing the game.
  * ~~Function to re-sync to the archipelago in case of crash or other unrecoverable state~~ - resync command
  * Locally save the progression buffers and reload upon server start

* Journal progress being blocked like the other progression.
* Include all progression elements in the current lock/unlock functionality (~~UnlockedShapeshiftElement~~, ~~UnlockedRecipeElement~~, ~~UnlockedBlueprintElement~~, ~~UnlockedSpellBookAbility~~, UnlockedSpellPointPassives)
* Add give item from archipelago


## Done
* ~~Archipelago categorization of progression, spheres, etc.~~ -Phye's manual (https://github.com/PhyeBloodrose/V-Rising-Manual) is being adapted for this use case
* ~~Connect to the archipelago~~
* ~~Change logs for AP progression to send to the archipelago (~~bosses~~, research, discover)~~
* ~~Make unlocks from the archipelago call~~ (working pending mapping)
* ~~DiscoverResearch needs to work like unlockProgression and remove the result~~
* ~~Refinement Station progress being blocked, there is some nuance here of refinement being locked/unlocked in station or even hidden in station.~~
* ~~UnlockedVBlood likely should stay separate if possible, that would let players actually see what they've killed in the vblood menu~~
* ~~Add dictionary between the nicely formatted names and the Tech_collection entity names~~
* ~~Make the progression buffers saved and written by startresearch/stopresearch actually connected to the archipelago's progression, not just variables in code~~
* ~~Add give item functions to support filler, should be straightforward~~
* ~~Block spell assignment during research.~~
* ~~LockResearch needs to be aware if the player has actually earned this in the archipelago or not.~~
  
## Ideas

* Implement Armipotent's original concept as "Killsanity", killing creatures sends checks
* FishSanity?
* Add filler table 
  * Paper/techbooks/coins are very powerful and should be limited in this pool. 
  * Raw resources
  * consumables
  * high percentage blood potions work great for this
* Progressive crafting cost reduction should be possible in runtime (difficult for unfriendly multiplayer)
* Progressive damage decrease (reducing how much damage vbloods do to make it easier to finish the game)
* Daywalker

## To-do multiplayer

* Neutral research desks to prevent sharing of research
* Change user queries to be based on specific users rather than all users 

## Kudos

* ZFolmt from the V Rising Modding discord for techincal help and ideas. He is also the creator of Bloodcraft which helped guide how I approached this
* Odjit's KindredExtract is legitimately the single greatest tool to make these mods. Also for KindredCommands which is both a great debugging tool and way to orchestrate game states.
* Armipotent for the original skeleton of this that I started from
* Phye for the manual mapping of archipelago progress
