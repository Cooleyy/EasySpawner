using System;
using UnityEngine;
using UnityEngine.UI;
using EasySpawner.Config;

namespace EasySpawner.UI
{
    public class EasySpawnerMenu
    {
        public Dropdown PrefabDropdown;
        public Dropdown PlayerDropdown;
        public Text PrefabDropdownLabel;
        public InputField SearchField;
        public InputField AmountField;
        public InputField LevelField;
        public Toggle PutInInventoryToggle;
        public Toggle IgnoreStackSizeToggle;
        public Toggle SearchSizeToggle;
        public Button SpawnButton;
        public Text SpawnText;
        public Text UndoText;
        public Text CloseText;

        private static readonly string PlaceholderOptionText = "Choose object to spawn";

        public void CreateMenu(GameObject menuGameObject)
        {
            PrefabDropdown = menuGameObject.transform.Find("PrefabDropdown").GetComponent<Dropdown>();
            PrefabDropdownLabel = menuGameObject.transform.Find("PrefabDropdown").Find("Label").GetComponent<Text>();

            PlayerDropdown = menuGameObject.transform.Find("PlayerDropdown").GetComponent<Dropdown>();

            SearchField = menuGameObject.transform.Find("SearchInputField").GetComponent<InputField>();
            SearchField.onValueChanged.AddListener(delegate { RebuildPrefabDropdown(); });

            AmountField = menuGameObject.transform.Find("AmountInputField").GetComponent<InputField>();
            LevelField = menuGameObject.transform.Find("LevelInputField").GetComponent<InputField>();

            PutInInventoryToggle = menuGameObject.transform.Find("PutInInventoryToggle").GetComponent<Toggle>();
            IgnoreStackSizeToggle = menuGameObject.transform.Find("IgnoreStackSizeToggle").GetComponent<Toggle>();
            SearchSizeToggle = menuGameObject.transform.Find("SearchSizeToggle").GetComponent<Toggle>();
            SearchSizeToggle.onValueChanged.AddListener(delegate { RebuildPrefabDropdown(); });//Rebuild prefabdropdown when toggled

            SpawnButton = menuGameObject.transform.Find("SpawnButton").GetComponent<Button>();
            SpawnButton.onClick.AddListener(SpawnButtonPress);

            Transform hotkeyTexts = menuGameObject.transform.Find("HotkeyText");
            SpawnText = hotkeyTexts.Find("SpawnText").GetComponent<Text>();
            UndoText = hotkeyTexts.Find("UndoText").GetComponent<Text>();
            CloseText = hotkeyTexts.Find("CloseText").GetComponent<Text>();

            //Set hotkey texts
            EasySpawnerConfig config = EasySpawnerPlugin.config;
            if (config.SpawnHotkeyModifierSet)
                SpawnText.text = "Spawn: " + config.FirstSpawnHotkeyModifier.Value + " + " +config.FirstSpawnHotkey.Value;
            else
                SpawnText.text = "Spawn: " + config.FirstSpawnHotkey.Value;

            if (config.UndoHotkeyModifierSet)
                UndoText.text = "Undo: " + config.UndoHotkeyModifier.Value + " + " + config.UndoHotkey.Value;
            else
                UndoText.text = "Undo: " + config.UndoHotkey.Value;

            if (config.OpenHotkeyModifierSet)
                CloseText.text = "Open: " + config.FirstOpenHotkeyModifier.Value + " + " + config.FirstOpenHotkey.Value;
            else
                CloseText.text = "Open: " + config.FirstOpenHotkey.Value;

            //Initial player dropdown
            PlayerDropdown.ClearOptions();
            RebuildPlayerDropdown();

            //Create prefab dropdown initial options
            PrefabDropdown.ClearOptions();
            PrefabDropdown.options.Add(new Dropdown.OptionData(PlaceholderOptionText));//Add placeholder option
            foreach (string prefabName in EasySpawnerPlugin.prefabNames)
            {
                if (!SearchSizeToggle.isOn && PrefabDropdown.options.Count >= 100)
                    break;
                PrefabDropdown.options.Add(new Dropdown.OptionData(prefabName));
            }
            PrefabDropdownLabel.text = PlaceholderOptionText;
        }

        public void RebuildPlayerDropdown()
        {
            if (PlayerDropdown && ZNet.instance && Player.m_localPlayer)
            {
                PlayerDropdown.ClearOptions();
                PlayerDropdown.options.Add(new Dropdown.OptionData(Player.m_localPlayer.GetPlayerName()));

                foreach (ZNet.PlayerInfo player in ZNet.instance.GetPlayerList())
                {
                    if (player.m_name != Player.m_localPlayer.GetPlayerName())
                        PlayerDropdown.options.Add(new Dropdown.OptionData(player.m_name));
                }
            }
            else
            {
                Debug.Log("EasySpawner: Cannot rebuild player dropdown");
            }
        }

        private void SpawnButtonPress()
        {
            if (PrefabDropdown.options.Count == 0 || PrefabDropdown.options[PrefabDropdown.value].text == PlaceholderOptionText)
                return;
            string prefabName = PrefabDropdown.options[PrefabDropdown.value].text;
            bool pickup = PutInInventoryToggle.isOn;
            bool ignoreStackSize = IgnoreStackSizeToggle.isOn;
            Player player = Player.m_localPlayer;

            if (!Int32.TryParse(AmountField.text, out int amount) || amount < 1)
                amount = 1;
            if (!Int32.TryParse(LevelField.text, out int level) || level < 1)
                level = 1;

            //If not local player selected in player dropdown then get the Player object for selected player, if player not found then player will stay as local player
            if (!PlayerDropdown.options[PlayerDropdown.value].text.Equals(player.GetPlayerName()))
            {
                pickup = false;//Cannot place items in other players inventories

                foreach (Player playerCheck in Player.GetAllPlayers())
                {
                    if (playerCheck.GetPlayerName().Equals(PlayerDropdown.options[PlayerDropdown.value].text))
                    {
                        player = playerCheck;
                        break;
                    }
                }
            }

            EasySpawnerPlugin.SpawnPrefab(prefabName, player, amount, level, pickup, ignoreStackSize);
        }

        //Update dropdown options using new search parameters
        public void RebuildPrefabDropdown()
        {
            if (PrefabDropdown)
            {
                PrefabDropdown.ClearOptions();
                foreach (string prefabName in EasySpawnerPlugin.prefabNames)
                {
                    if (!SearchSizeToggle.isOn && PrefabDropdown.options.Count >= 100)
                        break;
                    if (prefabName.IndexOf(SearchField.text, StringComparison.OrdinalIgnoreCase) >= 0)
                        PrefabDropdown.options.Add(new Dropdown.OptionData(prefabName));
                }
                PrefabDropdownLabel.text = PlaceholderOptionText;
            }
            else
            {
                Debug.Log("EasySpawner: Cannot rebuild prefab dropdown");
            }
        }

        public void Destroy()
        {
            SearchSizeToggle.onValueChanged.RemoveAllListeners();
            SearchField.onValueChanged.RemoveAllListeners();
            SpawnButton.onClick.RemoveAllListeners();
        }
    }
}
