# Archipelago V Rising Client

## Gameplay

* The first time you start a game you should use .connect to connect to your world
* When you are attempting to do research or unlock spells, you must first run the .startResearch command so your research/spells match what locaitons you've sent.
  * Failure to do this could result in duplicated location checks. 
* Once you are finished researching, use the .stopResearch command. This will set you back to the progression you've been granted.

## YAML Settings
Yaml options are now valid
Make sure to set your goal and the act's included options. So if you have goal of quincey, unselect act 2-4. Goal Ocatvian unselect 3-4. 
death_link is a go

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
* Below are admin commands
  * .unlockTech guid - unlocks the tech associated with a guid (see tech guids here https://wiki.vrisingmods.com/prefabs/Tech.html)
  * .lockTech guid - locks tech associated with a guid (see tech guids here https://wiki.vrisingmods.com/prefabs/Tech.html)

## Known Issues

* There is a big lag spike shortly after launching the server, noticeable often when shapeshifting. (only tested in host+play)
* Some Vbloods will give "cannot send location check" for deprecated techs.
* Gravedigger ring will likely unlock whenever you get an upgraded ring.

## To-do
* Include all progression elements in the current lock/unlock functionality (~~UnlockedShapeshiftElement~~, ~~UnlockedRecipeElement~~, ~~UnlockedBlueprintElement~~, ~~UnlockedSpellBookAbility~~, UnlockedSpellPointPassives)
* Add give item from archipelago
  
## Ideas

* Implement Armipotent's original concept as "Killsanity", killing creatures sends checks
* Early Spells - All spells are added as items, independent of act (requries filler, 34 prog items added to act 1)
* Levelize boss locations - All bosses are levelized to have minimum 3 locations (+11 in act 1)
* FishSanity - 10 locations (4 in act 1)
* JournalSanity - 31 locations (20 in act 1)
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
* Allow multiple players per server - shared archipelago progression, will require saving the record to the server by player.

## Kudos

* ZFolmt from the V Rising Modding discord for techincal help and ideas. He is also the creator of Bloodcraft which helped guide how I approached this
* Odjit's KindredExtract is legitimately the single greatest tool to make these mods. Also for KindredCommands which is both a great debugging tool and way to orchestrate game states.
* Armipotent for the original skeleton of this that I started from
* Phye for the manual mapping of archipelago progress
