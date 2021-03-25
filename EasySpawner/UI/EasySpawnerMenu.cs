using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using EasySpawner.Config;

namespace EasySpawner.UI
{
    public class EasySpawnerMenu
    {
        public Dropdown PlayerDropdown;
        public InputField SearchField;
        public InputField AmountField;
        public InputField LevelField;
        public Toggle PutInInventoryToggle;
        public Toggle IgnoreStackSizeToggle;
        public Button SpawnButton;
        public Text SpawnText;
        public Text UndoText;
        public Text CloseText;
        public ScrollRect PrefabScrollView;
        public PrefabItem[] PrefabItems;
        public PrefabItem SelectedPrefabItem;

        private static readonly string PlaceholderOptionText = "Choose object to spawn";

        public void CreateMenu(GameObject menuGameObject)
        {
            PrefabScrollView = menuGameObject.transform.Find("PrefabScrollView").GetComponent<ScrollRect>();

            PlayerDropdown = menuGameObject.transform.Find("PlayerDropdown").GetComponent<Dropdown>();

            SearchField = menuGameObject.transform.Find("SearchInputField").GetComponent<InputField>();
            SearchField.onValueChanged.AddListener(delegate { RebuildPrefabDropdown(); });

            AmountField = menuGameObject.transform.Find("AmountInputField").GetComponent<InputField>();
            LevelField = menuGameObject.transform.Find("LevelInputField").GetComponent<InputField>();

            PutInInventoryToggle = menuGameObject.transform.Find("PutInInventoryToggle").GetComponent<Toggle>();
            IgnoreStackSizeToggle = menuGameObject.transform.Find("IgnoreStackSizeToggle").GetComponent<Toggle>();

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

            //Create prefab dropdown with all options
            PrefabItems = new PrefabItem[EasySpawnerPlugin.prefabNames.Count];
            PrefabItem template = PrefabScrollView.content.GetChild(0).gameObject.AddComponent<PrefabItem>();
            template.rectTransform = template.GetComponent<RectTransform>();
            template.toggle = template.GetComponent<Toggle>();
            template.label = template.transform.Find("ItemLabel").GetComponent<Text>();
            template.originalHeight = template.rectTransform.rect.height;

            for (int i = 0; i < EasySpawnerPlugin.prefabNames.Count; i++) {
                string prefabName = EasySpawnerPlugin.prefabNames[i];
                GameObject option = UnityEngine.Object.Instantiate(template.gameObject, PrefabScrollView.content);
                PrefabItem item = option.GetComponent<PrefabItem>();
                item.label.text = prefabName;
                item.toggle.isOn = false;
                item.toggle.onValueChanged.AddListener(delegate { SelectPrefab(item); });

                PrefabItems[i] = item;
            }

            PrefabScrollView.content.GetChild(0).gameObject.SetActive(false);
            RebuildPrefabDropdown();
        }

        private void SelectPrefab(PrefabItem caller)
        {
            Parallel.ForEach(PrefabItems, prefabItem =>
            {
                // Disable all other prefabItems, without calling this method recursively
                prefabItem.toggle.SetIsOnWithoutNotify(prefabItem == caller);
            });

            SelectedPrefabItem = caller;
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
            if (SelectedPrefabItem == null)
                 return;
            string prefabName = SelectedPrefabItem.label.text;
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
            float downY = 0;
            float upY = 1000;
            bool anyActive = false;

            // search runs much faster when using Parallel
            Parallel.ForEach(PrefabItems, prefabItem =>
            {
                 prefabItem.isSearched = prefabItem.label.text.IndexOf(SearchField.text, StringComparison.OrdinalIgnoreCase) >= 0;
                 anyActive = anyActive || prefabItem.isSearched;
            });

            // cannot run in Parallel as PrefabItems rectTransform is changed
            foreach (PrefabItem prefabItem in PrefabItems)
            {
                Vector2 pos = new Vector2(prefabItem.rectTransform.anchoredPosition.x, 0);

                // updates position in scroll view
                if (prefabItem.isSearched)
                {
                    // active element
                    pos.y = -downY - prefabItem.originalHeight / 2f;
                    downY += prefabItem.originalHeight;
                }
                else
                {
                    // not active element, set out of view range. Cannot disable GameObject as is it to performance heavy
                    // and cannot put all at same y because it causes performance issues
                    pos.y = upY;
                    upY += prefabItem.originalHeight;
                }

                prefabItem.rectTransform.anchoredPosition = pos;
            }

            float scrollViewHeight = downY;
            PrefabScrollView.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, scrollViewHeight);

            // unselect prefab is none is active
            if (!anyActive)
            {
                SelectPrefab(null);
            }
        }

        public void Destroy()
        {
            SearchField.onValueChanged.RemoveAllListeners();
            SpawnButton.onClick.RemoveAllListeners();
        }
    }
}
