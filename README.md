# EasySpawner
## Mod for Valheim using BepInEx
Easy to use Item/NPC/Prefab spawner 

Easy spawner provides a simple and easy to use UI for spawning any Valheim prefab inside the game. 
Use this mod to spawn in items or enemies (of any level!) or just experiment spawning any gameobject possible (includes fx, vfx, and sfx)

While in game press "/" or "Numpad /" to show/hide the menu. Hotkey can be changed in config.

The main feature of the provided menu is a search field that when typed into updates a list of prefabs that contain the search term (non case-sensitive).
The prefabs will contain all manner of gameobjects from items, enemies, world objects, fx, vfx, sfx and anything else in the game.
Anything that can be provided a level such as equipable items and enemies can be spawned at any level up to 999,999,999.
The mod is fully client-side and not required by anyone but the person who wants to use it, this can be used on any server.

Note: The prefab names may differ from the in game name. E.g. The 5th boss Yagluth's prefab is called GoblinKing

*Warning: This mod can massively affect your character and world. Do not use this on a server where you are not given permission by others on it!* 

## Menu UI
<html>
<img src="https://john2143.com/f/AU2E.png" width="450">
</html>

Example. Entering "copper" will provide all prefabs with "copper" contained in the name, here it provide Copper bar, Copper ore, Copper knife and three variants of a copper vein.

<html>
<img src="https://john2143.com/f/UrJq.png" width="450">
</html>

Clicking the spawn button, pressing "=" or "Numpad +" will create the selected prefab at the chosen player.

## Features

* The menu UI can be dragged within the screen with the mouse.

* The search field auto updates the prefab list with every character entered.

* You can choose the player to spawn the prefab at. 
*Note: you cannot place items into others inventory*

* You can choose the amount and level of the item/object you want to spawn using the amount and level field.

* You can create Items/Npcs much higher level than the game normally permits. E.g. a level 999999999 Silver sword.

<html>
<img src="https://john2143.com/f/J2gk.png" width="450">
</html>

* You can tick the "Put in inventory" toggle to immediately place the item in your inventory. 
*Note: this only works for item drops and only works on yourself, if you tick this and try on other players it will just spawn in front of them as normal*

* You can go over the normal stack size limit by ticking the "Ignore stack size" toggle. E.g. a 999 stack of arrows.

<html>
<img src="https://john2143.com/f/lTiY.png" width="450">
</html>

* Spawn something you didnt mean to or didnt realise would be indestructible? You can undo it by pressing the Undo hotkey, defaults to left ctrl + z.

* You can select favourite items that will stay at the top of the list for easier access.

## Config

In your BepInEx/Config there is a config file that can be used to change the hotkeys to open/close the menu, spawn items and undo. Be aware setting this to a letter will cause the menu to close if you type it in the search box!

## Installation

1. [Install BepInEx for Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/)

2. If you have the zip, extract within your BepInEx plugins folder. Otherwise place the .dll in a folder called "EasySpawner" in your BepInEx plugins folder.

## Known Issues

Some times the coroutine checking for changes in the player list is killed unexepectedly.

## Development
Create a file called `Environment.props` inside the project root.
Copy the example and change the Valheim install path to your location.
If you use r2modman you can set the path too, but this is optional.

````
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <PropertyGroup>
        <!-- Needs to be your path to the base Valheim folder -->
        <VALHEIM_INSTALL>E:\Programme\Steam\steamapps\common\Valheim</VALHEIM_INSTALL>
        <!-- Optional, needs to be the path to a r2modmanPlus profile folder -->
        <R2MODMAN_INSTALL>C:\Users\[user]\AppData\Roaming\r2modmanPlus-local\Valheim\profiles\Develop</R2MODMAN_INSTALL>
    </PropertyGroup>
</Project>
````

## Changelog

#### 1.4.2

* Favourite items now stored in same location as config file.

#### 1.4.1

* Asset bundle for UI is now contained within main .dll

#### 1.4.0

* Adds ability to select favourite items that stay at the top of the list. These are saved on file to persist between sessions.
* Player input is more thouroughly blocked during input to fields.

#### 1.3.0

* Changed prefab dropdown to a scrollView. Now all prefabs are shown in the list at all times, so removed the "show > 100 search results" toggle.
* Prefabs from other mods are correctly sorted into the list.

#### 1.2.0

* Added Undo hotkey feature. Can be used to undo last set of spawns, deleting the objects from the game. If items have been destroyed since spawning this does nothing.
* Hotkeys are now shown on UI. Hotkey text will update when hotkeys changed in config.

#### 1.1.4

* Fixed bug with spawning Serpent Trophy as its max stack size is incorrectly set to 0 by the base game.

#### 1.1.3

* Added ability to set hotkey modifiers. E.g. leftAlt + / to open menu

#### 1.1.2

* Fixed conflict with Extended Item data mod

#### 1.1.1

* No longer needs exact file name, should fix issues with mod managers

#### 1.1.0

* Added config file to change hotkeys
