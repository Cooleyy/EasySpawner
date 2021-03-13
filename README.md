# EasySpawner
## Mod for Valheim using BepInEx
Easy to use Item/NPC/Prefab spawner 

Easy spawner provides a simple and easy to use UI for spawning any Valheim prefab inside the game. 
Use this mod to spawn in items or enemies (of any level!) or just experiment spawning any gameobject possible (includes fx, vfx, and sfx)

While in game press "/" or "Numpad /" to show/hide the menu. Hotkey can be changed in config.

The main feature of the provided menu is a search field that when typed into updates a dropdown of prefabs that contain the search term (non case-sensitive).
The prefabs will contain all manner of gameobjects from items, enemies, world objects, fx, vfx, sfx and anything else in the game.
Anything that can be provided a level such as equipable items and enemies can be spawned at any level up to 999,999,999.
The mod is fully client-side and not required by anyone but the person who wants to use it, this can be used on any server.

Note: The prefab names may differ from the in game name. E.g. The 5th boss Yagluth's prefab is called GoblinKing

*Warning: This mod can massively affect your character and world. Do not use this on a server where you are not given permission by others on it!* 

## Menu UI
<html>
<img src="https://john2143.com/f/oTub.png" width="450">
</html>

Example. Entering "copper" will provide all prefabs with "copper" contained in the name, here it provide Copper bar, Copper ore, Copper knife and three variants of a copper vein.

<html>
<img src="https://john2143.com/f/cFY4.png" width="450">
</html>

Clicking the spawn button, pressing "=" or "Numpad +" will create the selected prefab at the chosen player.

## Features

* The menu UI can be dragged within the screen with the mouse.

* The search field auto updates the prefab dropdown with every character entered

* You can choose the player to spawn the prefab at. 
*Note: you cannot place items into others inventory*

* You can choose the amount and level of the item/object you want to spawn using the amount and level field

* You can create Items/Npcs much higher level than the game normally permits. E.g. a level 999999999 Silver sword 

<html>
<img src="https://john2143.com/f/w2rG.png" width="450">
</html>

* You can tick the "Put in inventory" toggle to immediately place the item in your inventory. 
*Note: this only works for item drops and only works on yourself, if you tick this and try on other players it will just spawn in front of them as normal*

* You can go over the normal stack size limit by ticking the "Ignore stack size" toggle. E.g. a 999 stack of arrows

<html>
<img src="https://john2143.com/f/UYIf.png" width="450">
</html>

* By default the dropdown only fills with up to 100 search results as large dropdowns can cause a bit of lag when opened, check the "Show > 100 search results" toggle to fill the dropdown with every result found. (Dropdown with every single prefab can lag the game for a few seconds)*

## Config

In your BepInEx/Config there is a config file that can be used to change the hotkeys to open the menu or spawn items. Be aware setting this to a letter will cause the menu to close if you type it in the search box!

## Installation

1. [Install BepInEx for Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/)

2. If you have the zip, extract within your BepInEx plugins folder. Otherwise place the .dll and Assetbundle file in a folder called "EasySpawner" in your BepInEx plugins folder.

## Known Issues

Some times the coroutine checking for changes in the player list is killed unexepectedly.

## Changelog

#### 1.1.4

* Fixed bug with spawn Serpent Trophy as its max stack size is incorrectly set to 0 by the base game.

#### 1.1.3

* Added ability to set hotkey modifiers. E.g. leftAlt + / to open menu

#### 1.1.2

* Fixed conflict with Extended Item data mod

#### 1.1.1

* No longer needs exact file name, should fix issues with mod managers

#### 1.1.0

* Added config file to change hotkeys
