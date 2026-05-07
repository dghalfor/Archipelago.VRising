# Archipelago V Rising Client

HEAVY WIP. Will need documentation. Yes, the skeleton of this readme is stolen from [Archipelago.RiskOfRain2](https://github.com/Ijwu/Archipelago.RiskOfRain2/).

## Gameplay

## YAML Settings

An example YAML would look like this:
```yaml
description: Armi-VRising
name: Armi

game: V Rising
V Rising:
  
```

| Name | Description | Allowed Values |
| ---- | ----------- | -------------- |
| | |

## Connecting to an Archipelago Server

## Changelog

## Current commands

* .startResearch - switches research progression buffers to allow for the collection of checks in your research desks (see todo)
* .stopResearch - switches progression buffers to the elements that have been unlocked by the archipelago (see todo)
* .unlockTech guid - unlocks the tech associated with a guid (see tech guids here https://wiki.vrisingmods.com/prefabs/Tech.html)
* .lockTech guid - locks tech associated with a guid (see tech guids here https://wiki.vrisingmods.com/prefabs/Tech.html)

## Known Issues

* Elements removed from research at runtime are re-added after saving and closing the game.

## To-do

* ~~DiscoverResearch needs to work like unlockProgression and remove the result~~
* Make the progression buffers saved and written by startresearch/stopresearch actually connected to the archipelago's progression, not just variables in code
* Refinement Station progress being blocked, there is some nuance here of refinement being locked/unlocked in station or even hidden in station.
* Journal progress being blocked like the other progression.
* Connect to the archipelago
* Change logs for AP progression to send to the archipelago
* Make unlocks from the archipelago call
* Include all progression elements in the current lock/unlock functionality (UnlockedShapeshiftElement, UnlockedRecipeElement(working), UnlockedBlueprintElement, UnlockedSpellBookAbility, UnlockedSpellPointPassives)
* UnlockedVBlood likely should stay separate if possible, that would let players actually see what they've killed in the vblood menu
* Locally save the progression buffers and reload upon server start
* Function to re-sync to the archipelago in case of crash or other unrecoverable state
* Add give item functions to support filler, should be straightforward
* Archipelago categorization of progression, spheres, etc.
* LockResearch needs to be aware if the player has actually earned this in the archipelago or not.

## Ideas

* Implement Armipotent's original concept as "Killsanity", killing creatures sends checks
* FishSanity?
* Add filler table - Paper/techbooks/coins are very powerful and should be limited in this pool. Raw resources, consumables, and high percentage blood potions work great for this
* Progressive crafting cost reduction should be possible in runtime
* Progressive damage decrease (reducing how much damage vbloods do to make it easier to finish the game)

## To-do multiplayer

* Neutral research desks to prevent sharing of research
* Change user queries to be based on specific users rather than all users 

## Setup

* Add archipelago.multiclient.net.dll to the plugins folder
* Add ServerLaunchFix.dll to the plugins

## Kudos

* ZFolmt from the V Rising Modding discord for techincal help and ideas. He is also the creator of Bloodcraft which helped guide how I approached this
* Odjit's KindredExtract is legitimately the single greatest tool to make these mods. Also for KindredCommands which is both a great debugging tool and way to orchestrate game states.
* Armipotent for the original skeleton of this that I started from
