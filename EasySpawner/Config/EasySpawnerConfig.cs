using BepInEx.Configuration;
using UnityEngine;

namespace EasySpawner.Config
{
    public class EasySpawnerConfig
    {
        public ConfigFile ConfigFile { get; set; }

        public ConfigEntry<string> FirstOpenHotkey;
        public ConfigEntry<string> FirstOpenHotkeyModifier;
        public ConfigEntry<string> SecondOpenHotkey;
        public ConfigEntry<string> FirstSpawnHotkey;
        public ConfigEntry<string> FirstSpawnHotkeyModifier;
        public ConfigEntry<string> SecondSpawnHotkey;
        public ConfigEntry<string> UndoHotkey;
        public ConfigEntry<string> UndoHotkeyModifier;

        public bool OpenHotkeyModifierSet;
        public bool SpawnHotkeyModifierSet;
        public bool UndoHotkeyModifierSet;

        public void SetupConfig()
        {
            FirstOpenHotkey = ConfigFile.Bind("Hotkeys", "firstOpenHotkey", "/", "Main hotkey to show/hide the menu. To find appropriate hotkeys use https://answers.unity.com/questions/762073/c-list-of-string-name-for-inputgetkeystring-name.html");
            FirstOpenHotkeyModifier = ConfigFile.Bind("Hotkeys", "firstOpenHotkeyModifier", "", "Optional Modifier to the firstOpenHotkey. Setting this will mean you have to press firstOpenHotkey + firstOpenHotkeyModifier to open/hide the menu. E.g. set this to left alt");
            SecondOpenHotkey = ConfigFile.Bind("Hotkeys", "secondOpenHotkey", "[/]", "Secondary hotkey to show/hide the menu");
            
            FirstSpawnHotkey = ConfigFile.Bind("Hotkeys", "firstSpawnHotkey", "=", "Main hotkey to spawn selected prefab");
            FirstSpawnHotkeyModifier = ConfigFile.Bind("Hotkeys", "firstSpawnHotkeyModifier", "", "Optional Modifier to the firstSpawnHotkey. Setting this will mean you have to press firstSpawnHotkey + firstSpawnHotkeyModifier to spawn selected prefab. E.g. set this to left alt");
            SecondSpawnHotkey = ConfigFile.Bind("Hotkeys", "secondSpawnHotkey", "[+]", "Secondary hotkey to spawn selected prefab");

            UndoHotkey = ConfigFile.Bind("Hotkeys", "UndoHotkey", "z", "Hotkey to Undo last spawn");
            UndoHotkeyModifier = ConfigFile.Bind("Hotkeys", "UndoHotkeyModifier", "left ctrl", "Modifier to Undo hotkey");

            FirstOpenHotkey.Value = FirstOpenHotkey.Value.ToLower();
            FirstOpenHotkeyModifier.Value = FirstOpenHotkeyModifier.Value.ToLower();
            SecondOpenHotkey.Value = SecondOpenHotkey.Value.ToLower();
            FirstSpawnHotkey.Value = FirstSpawnHotkey.Value.ToLower();
            FirstSpawnHotkeyModifier.Value = FirstSpawnHotkeyModifier.Value.ToLower();
            SecondSpawnHotkey.Value = SecondSpawnHotkey.Value.ToLower();
            UndoHotkey.Value = UndoHotkey.Value.ToLower();
            UndoHotkeyModifier.Value = UndoHotkeyModifier.Value.ToLower();

            OpenHotkeyModifierSet = !string.IsNullOrWhiteSpace(FirstOpenHotkeyModifier.Value);
            SpawnHotkeyModifierSet = !string.IsNullOrWhiteSpace(FirstSpawnHotkeyModifier.Value);
            UndoHotkeyModifierSet = !string.IsNullOrWhiteSpace(UndoHotkeyModifier.Value);
        }

        public bool IfUndoHotkeyPressed()
        {
            if (!UndoHotkeyModifierSet)
                return Input.GetKeyDown(UndoHotkey.Value);
            else
                return (Input.GetKey(UndoHotkeyModifier.Value) && Input.GetKeyDown(UndoHotkey.Value));
        }

        public bool IfMenuHotkeyPressed()
        {
            if (!OpenHotkeyModifierSet)
                return Input.GetKeyDown(FirstOpenHotkey.Value) || Input.GetKeyDown(SecondOpenHotkey.Value);
            else
                return (Input.GetKey(FirstOpenHotkeyModifier.Value) && Input.GetKeyDown(FirstOpenHotkey.Value)) || Input.GetKeyDown(SecondOpenHotkey.Value);
        }

        public bool IfSpawnHotkeyPressed()
        {
            if (!SpawnHotkeyModifierSet)
                return Input.GetKeyDown(FirstSpawnHotkey.Value) || Input.GetKeyDown(SecondSpawnHotkey.Value);
            else
                return (Input.GetKey(FirstSpawnHotkeyModifier.Value) && Input.GetKeyDown(FirstSpawnHotkey.Value)) || Input.GetKeyDown(SecondSpawnHotkey.Value);
        }
    }
}
