using System;
using System.Collections.Generic;
using System.Linq;
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
        public Queue<PrefabItem> PrefabItemPool = new Queue<PrefabItem>();
        public string SelectedPrefabName;
        public static Dictionary<string, bool> PrefabNamesSearched = new Dictionary<string, bool>();
        public List<string> SearchItems = new List<string>();

        public void CreateMenu(GameObject menuGameObject)
        {
            PlayerDropdown = menuGameObject.transform.Find("PlayerDropdown").GetComponent<Dropdown>();
            PrefabScrollView = menuGameObject.transform.Find("PrefabScrollView").GetComponent<ScrollRect>();

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

            //Create prefab dropdown pool
            PrefabItems = new PrefabItem[20];
            PrefabItem template = PrefabScrollView.content.GetChild(0).gameObject.AddComponent<PrefabItem>();
            template.rectTransform = template.GetComponent<RectTransform>();
            template.toggle = template.GetComponent<Toggle>();
            template.label = template.transform.Find("ItemLabel").GetComponent<Text>();
            template.originalHeight = template.rectTransform.rect.height;
            template.gameObject.SetActive(false);

            for (int i = 0; i < 20; i++)
            {
                GameObject option = UnityEngine.Object.Instantiate(template.gameObject, PrefabScrollView.content);
                PrefabItem item = option.GetComponent<PrefabItem>();
                item.toggle.isOn = false;
                item.toggle.onValueChanged.AddListener(delegate { SelectPrefab(item); });
                PrefabItemPool.Enqueue(item);
                PrefabItems[i] = item;
            }

            PrefabScrollView.onValueChanged.AddListener(UpdateItemPrefabPool);
            RebuildPrefabDropdown();
        }

        public void PoolPrefabItem(PrefabItem item)
        {
            item.gameObject.SetActive(false);
            item.posIndex = -1;
            item.toggle.SetIsOnWithoutNotify(false);
            PrefabItemPool.Enqueue(item);
        }

        public void PoolAllPrefabItems()
        {
            foreach (PrefabItem item in PrefabItems)
            {
                if (PrefabItemPool.Contains(item))
                    continue;

                PoolPrefabItem(item);
            }
        }

        public void UpdateItemPrefabPool(Vector2 slider)
        {
            PrefabScrollView.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,(SearchItems.Count - 1) * 20f);
            Rect scrollRect = PrefabScrollView.GetComponent<RectTransform>().rect;
            Vector2 scrollPos = PrefabScrollView.content.anchoredPosition;

            // search for items that are out of visible scroll rect
            foreach (PrefabItem item in PrefabItems)
            {
                if (PrefabItemPool.Contains(item))
                    continue;

                float posY = item.rectTransform.anchoredPosition.y;

                if (posY > -scrollPos.y + 20 || posY < -scrollPos.y - scrollRect.height - 20)
                    PoolPrefabItem(item);
            }

            int startIndex = Mathf.Max(0, Mathf.CeilToInt((scrollPos.y - 20) / 20));
            int maxItems = Mathf.CeilToInt((scrollRect.height + 40) / 20);

            for (int i = startIndex; i < Mathf.Min(startIndex + maxItems, SearchItems.Count); i++)
            {
                if (PrefabItems.Any(x => x.posIndex == i))
                    continue;

                if (PrefabItemPool.Count > 0)
                {
                    PrefabItem item = PrefabItemPool.Dequeue();
                    item.rectTransform.anchoredPosition = new Vector2(0, -i * 20 - 10f);
                    item.posIndex = i;
                    item.label.text = SearchItems[i];
                    item.toggle.SetIsOnWithoutNotify(SelectedPrefabName == item.label.text);
                    item.gameObject.SetActive(true);
                }
            }
        }

        private void SelectPrefab(PrefabItem caller)
        {
            foreach (PrefabItem prefabItem in PrefabItems)
            {
                // Disable all other prefabItems, without calling this method recursively
                prefabItem.toggle.SetIsOnWithoutNotify(prefabItem == caller);
            }

            SelectedPrefabName = caller != null ? caller.label.text : null;
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
            if (SelectedPrefabName == null)
                return;
            string prefabName = SelectedPrefabName;
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
            SelectPrefab(null);
            SearchItems.Clear();

            Parallel.ForEach(EasySpawnerPlugin.prefabNames, name =>
            {
                bool isSearched = name.IndexOf(SearchField.text, StringComparison.OrdinalIgnoreCase) >= 0;
                PrefabNamesSearched[name] = isSearched;
            });

            foreach (string name in EasySpawnerPlugin.prefabNames)
            {
                if (PrefabNamesSearched[name])
                    SearchItems.Add(name);
            }

            PoolAllPrefabItems();
            UpdateItemPrefabPool(new Vector2(PrefabScrollView.horizontalScrollbar.value, PrefabScrollView.verticalScrollbar.value));
        }

        public void Destroy()
        {
            PrefabScrollView.onValueChanged.RemoveAllListeners();
            SearchField.onValueChanged.RemoveAllListeners();
            SpawnButton.onClick.RemoveAllListeners();
            PrefabItems = null;
            PrefabItemPool = new Queue<PrefabItem>();
        }
    }
}
