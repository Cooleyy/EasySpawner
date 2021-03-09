using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace EasySpawner
{
    [BepInPlugin("cooley.easyspawner", "Easy Spawner", "1.1.1")]
    [BepInProcess("valheim.exe")]
    public class EasySpawner : BaseUnityPlugin
    {
        public static ConfigEntry<string> firstOpenHotkey;
        public static ConfigEntry<string> secondOpenHotkey;
        public static ConfigEntry<string> firstSpawnHotkey;
        public static ConfigEntry<string> secondSpawnHotkey;

        public static List<string> prefabNames = new List<string>();
        private static List<string> playerNames = new List<string>();

        public static AssetBundle menuAssetBundle;
        public static GameObject menuPrefab;
        public static GameObject menuGameObject;
        public static Dropdown prefabDropdown;
        public static Dropdown playerDropdown;
        public static Text prefabDropdownLabel;
        public static InputField searchField;
        public static InputField amountField;
        public static InputField levelField;
        public static Toggle putInInventoryToggle;
        public static Toggle ignoreStackSizeToggle;
        public static Toggle searchSizeToggle;
        public static Button spawnButton;

        private static readonly string PlaceholderOptionText = "Choose object to spawn";
        private static readonly string assetBundleName = "EasySpawnerAssetBundle";

        public Harmony harmony;

        void Awake()
        {
            Debug.Log("Easy spawner: Easy spawner loaded plugin");

            harmony = new Harmony("cooley.easyspawner");
            harmony.PatchAll();

            firstOpenHotkey = Config.Bind("Hotkeys", "firstOpenHotkey", "/", "Main hotkey to show/hide the menu. To find appropriate hotkeys use https://answers.unity.com/questions/762073/c-list-of-string-name-for-inputgetkeystring-name.html");
            secondOpenHotkey = Config.Bind("Hotkeys", "secondOpenHotkey", "[/]", "Secondary hotkey to show/hide the menu");
            firstSpawnHotkey = Config.Bind("Hotkeys", "firstSpawnHotkey", "=", "Main hotkey to spawn selected prefab");
            secondSpawnHotkey = Config.Bind("Hotkeys", "secondSpawnHotkey", "[+]", "Secondary hotkey to spawn selected prefab");
        }

        void Update()
        {
            if ((Input.GetKeyDown(firstOpenHotkey.Value) || Input.GetKeyDown(secondOpenHotkey.Value)) && Player.m_localPlayer)
            {
                if (!menuGameObject)
                    CreateMenu();
                else
                    menuGameObject.SetActive(!menuGameObject.activeSelf);
            }

            if (menuGameObject)
            {
                if ((Input.GetKeyDown(firstSpawnHotkey.Value) || Input.GetKeyDown(secondSpawnHotkey.Value)))
                {
                    spawnButton.onClick.Invoke();
                }
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            if (harmony != null)
                harmony.UnpatchSelf();
            if (menuAssetBundle)
                menuAssetBundle.Unload(true);
            if (menuGameObject != null)
            {
                searchSizeToggle.onValueChanged.RemoveAllListeners();
                searchField.onValueChanged.RemoveAllListeners();
                spawnButton.onClick.RemoveAllListeners();
                Destroy(menuGameObject);
            }
        }

        private static void LoadAsset(string assetFileName)
        {
            if (menuAssetBundle)
            {
                Debug.Log("Easy spawner: menu asset bundle already loaded");
                return;
            }

            string pathToAsset = Path.Combine(Paths.PluginPath, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assetFileName);
            menuAssetBundle = AssetBundle.LoadFromFile(pathToAsset);

            if (menuAssetBundle)
                Debug.Log("Easy spawner: menu asset bundle loaded");
            else
            {
                Debug.Log("Easy spawner: menu asset bundle failed to load");
            }
        }

        private void CreateMenu()
        {
            Debug.Log("Easy spawner: Loading menu prefab");

            if(!menuAssetBundle)
            {
                Debug.Log("EasySpawner: Asset bundle not loaded");
                return;
            }    

            if (!menuPrefab)
                menuPrefab = menuAssetBundle.LoadAsset<GameObject>("EasySpawnerMenu");
            
            if (!menuPrefab)
            {
                Debug.Log("Easy spawner: Loading menu prefab failed");
                return;
            }
            Debug.Log("Easy spawner: Successfully loaded menu prefab");

            //Add script to make menu mouse draggable as assetbundle cannot contain script
            if(!menuPrefab.GetComponent<UIElementDragger>())
                menuPrefab.AddComponent<UIElementDragger>();

            menuGameObject = Instantiate(menuPrefab);

            //Attach menu to Valheims UI gameobject
            var uiGO = GameObject.Find("IngameGui(Clone)");

            if(!uiGO)
            {
                Debug.Log("Easy spawner: Couldnt find UI gameobject");
                return;
            }

            menuGameObject.transform.SetParent(uiGO.transform);
            menuGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            menuGameObject.transform.localPosition = new Vector3(-650, 0, 0);

            prefabDropdown = menuGameObject.transform.Find("PrefabDropdown").GetComponent<Dropdown>();
            prefabDropdownLabel = menuGameObject.transform.Find("PrefabDropdown").Find("Label").GetComponent<Text>();

            playerDropdown = menuGameObject.transform.Find("PlayerDropdown").GetComponent<Dropdown>();

            searchField = menuGameObject.transform.Find("SearchInputField").GetComponent<InputField>();
            searchField.onValueChanged.AddListener(delegate { RebuildPrefabDropdown(); });

            amountField = menuGameObject.transform.Find("AmountInputField").GetComponent<InputField>();
            levelField = menuGameObject.transform.Find("LevelInputField").GetComponent<InputField>();

            putInInventoryToggle = menuGameObject.transform.Find("PutInInventoryToggle").GetComponent<Toggle>();
            ignoreStackSizeToggle = menuGameObject.transform.Find("IgnoreStackSizeToggle").GetComponent<Toggle>();
            searchSizeToggle = menuGameObject.transform.Find("SearchSizeToggle").GetComponent<Toggle>();
            searchSizeToggle.onValueChanged.AddListener(delegate { RebuildPrefabDropdown(); });//Rebuild prefabdropdown when toggled

            spawnButton = menuGameObject.transform.Find("SpawnButton").GetComponent<Button>();
            spawnButton.onClick.AddListener(SpawnButtonPress);

            playerNames = GetPlayerNames(); 
            playerDropdown.ClearOptions();
            RebuildPlayerDropdown();

            //Attach CheckForNewPlayersCoroutine to UIElementdragger on the menu Gameobject so if it gets destroyed it also stops the coroutine
            menuGameObject.GetComponent<UIElementDragger>().StartCoroutine(CheckForNewPlayersCoroutine());

            //Create dropdown initial options
            prefabDropdown.ClearOptions();
            prefabDropdown.options.Add(new Dropdown.OptionData(PlaceholderOptionText));//Add placeholder option
            foreach (string prefabName in prefabNames)
            {
                if (!searchSizeToggle.isOn && prefabDropdown.options.Count >= 100)
                    break;
                prefabDropdown.options.Add(new Dropdown.OptionData(prefabName));
            }
            prefabDropdownLabel.text = PlaceholderOptionText;
        }

        private void RebuildPlayerDropdown()
        {
            if(playerDropdown && ZNet.instance && Player.m_localPlayer)
            {
                playerDropdown.ClearOptions();
                playerDropdown.options.Add(new Dropdown.OptionData(Player.m_localPlayer.GetPlayerName()));

                foreach (ZNet.PlayerInfo player in ZNet.instance.GetPlayerList())
                {
                    if (player.m_name != Player.m_localPlayer.GetPlayerName())
                        playerDropdown.options.Add(new Dropdown.OptionData(player.m_name));
                }
            }
            else
            {
                StartCoroutine(RetryBuildPlayerDropdown());
                Debug.Log("EasySpawner: Cannot rebuild player dropdown");
            }
        }

        //Update dropdown options using new search parameters
        private void RebuildPrefabDropdown()
        {
            if(prefabDropdown)
            {
                prefabDropdown.ClearOptions();
                foreach (string prefabName in prefabNames)
                {
                    if (!searchSizeToggle.isOn && prefabDropdown.options.Count >= 100)
                        break;
                    if (prefabName.IndexOf(searchField.text, StringComparison.OrdinalIgnoreCase) >= 0)
                        prefabDropdown.options.Add(new Dropdown.OptionData(prefabName));
                }
                prefabDropdownLabel.text = PlaceholderOptionText;
            }
            else
            {
                Debug.Log("EasySpawner: Cannot rebuild prefab dropdown");
            }
        }

        private void SpawnButtonPress()
        {
            if (prefabDropdown.options.Count == 0 || prefabDropdown.options[prefabDropdown.value].text == PlaceholderOptionText)
                return;
            string prefabName = prefabDropdown.options[prefabDropdown.value].text;
            bool pickup = putInInventoryToggle.isOn;
            bool ignoreStackSize = ignoreStackSizeToggle.isOn;
            Player player = Player.m_localPlayer;

            if (!Int32.TryParse(amountField.text, out int amount) || amount < 1)
                amount = 1;
            if (!Int32.TryParse(levelField.text, out int level) || level < 1)
                level = 1;

            //If not local player selected in player dropdown then get the Player object for selected player, if player not found then player will stay as local player
            if (!playerDropdown.options[playerDropdown.value].text.Equals(player.GetPlayerName()))
            {
                pickup = false;//Cannot place items in other players inventories

                foreach (Player playerCheck in Player.GetAllPlayers())
                {
                    if (playerCheck.GetPlayerName().Equals(playerDropdown.options[playerDropdown.value].text))
                    {
                        player = playerCheck;
                        break;
                    }
                }
            }

            SpawnPrefab(prefabName, player, amount, level, pickup, ignoreStackSize);
        }

        public void SpawnPrefab(string prefabName, Player player, int amount = 1 , int level = 1, bool pickup = false, bool ignoreStackSize = false)
        {
            Debug.Log("Easy spawner: Trying to spawn " + prefabName);
            GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (!prefab)
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, prefabName + " does not exist");
                Debug.Log("Easy spawner: spawning " + prefabName + " failed");
            }
            else 
            {
                //If prefab is an npc/enemy
                if (prefab.GetComponent<Character>())
                {
                    for (int i = 0; i < amount; i++)
                    {
                        Character character = Instantiate(prefab, player.transform.position + player.transform.forward * 2f + Vector3.up, Quaternion.identity).GetComponent<Character>();
                        if (level > 1)
                            character.SetLevel(level);
                    }
                }
                //if prefab is an item
                else if (prefab.GetComponent<ItemDrop>())
                {
                    ItemDrop itemPrefab = prefab.GetComponent<ItemDrop>();
                    if(itemPrefab.m_itemData.IsEquipable())
                    {
                        if (ignoreStackSize)
                        {
                            itemPrefab.m_itemData.m_stack = amount;
                            amount = 1;
                        }
                        itemPrefab.m_itemData.m_quality = level;
                        itemPrefab.m_itemData.m_durability = itemPrefab.m_itemData.GetMaxDurability();
                        for (int i = 0; i < amount; i++)
                        {
                            SpawnItem(pickup, prefab, player);
                        }
                    }
                    else
                    {
                        int noOfStacks = 1;
                        int lastStack = 0;
                        if (ignoreStackSize)
                        {
                            itemPrefab.m_itemData.m_stack = amount;
                        }
                        else
                        {
                            int maxStack = itemPrefab.m_itemData.m_shared.m_maxStackSize;
                            itemPrefab.m_itemData.m_stack = maxStack;
                            noOfStacks = amount / maxStack;
                            lastStack = amount % maxStack;
                        }

                        for (int i = 0; i < noOfStacks; i++)
                        {
                            SpawnItem(pickup, prefab, player);
                        }
                        if(lastStack != 0)
                        {
                            itemPrefab.m_itemData.m_stack = lastStack;
                            SpawnItem(pickup, prefab, player);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < amount; i++)
                    {
                        Instantiate(prefab, player.transform.position + player.transform.forward * 2f, Quaternion.identity);
                    }
                }

                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Spawning object " + prefabName);
                Debug.Log("Easy spawner: Spawned " + amount + " " + prefabName);
            }
        }

        private void SpawnItem(bool pickup, GameObject prefab, Player player)
        {
            if (pickup)
                Player.m_localPlayer.PickupPrefab(prefab);
            else
                Instantiate(prefab, player.transform.position + player.transform.forward * 2f + Vector3.up, Quaternion.identity);
        }

        private List<string> GetPlayerNames()
        {
            List<string> newPlayerNames = new List<string>();
            if (ZNet.instance)
            {
                foreach (ZNet.PlayerInfo player in ZNet.instance.GetPlayerList())
                {
                    newPlayerNames.Add(player.m_name);
                }
            }
            return newPlayerNames;
        }

        private void PlayerListChanged()
        {
            Debug.Log("EasySpawner: Player list changed, updating player dropdown");
            playerNames = GetPlayerNames();
            RebuildPlayerDropdown();
        }

        //Coroutine to check if list of player names has changed every 3 seconds while menu gameobject exists
        private IEnumerator CheckForNewPlayersCoroutine()
        {
            Debug.Log("EasySpawner: Starting check for new players coroutine");
            while (menuGameObject)
            {
                yield return new WaitForSeconds(3);

                if (menuGameObject.activeSelf && ZNet.instance && Player.m_localPlayer)
                {
                    List<string> newPlayerNames = GetPlayerNames();
                    if(newPlayerNames.Count != playerNames.Count)
                    {
                        PlayerListChanged();
                        continue;
                    }

                    foreach (string name in newPlayerNames)
                    {
                        if(!playerNames.Contains(name))
                        {
                            PlayerListChanged();
                            break;
                        }
                    }
                }
            }
            Debug.Log("EasySpawner: Stopping check for new players coroutine");
        }

        private IEnumerator RetryBuildPlayerDropdown()
        {
            yield return new WaitForSeconds(1);
            RebuildPlayerDropdown();
        }

        //Patch TakeInput on PlayerController to refuse player input if any input fields on the menu are being interacted with
        [HarmonyPatch(typeof(PlayerController), "TakeInput")]
        static class PlayerControllerTakeInputPatch
        {
            static void Postfix(ref bool __result)
            {
                if (menuGameObject != null && menuGameObject.activeSelf && (searchField.isFocused || amountField.isFocused || levelField.isFocused))
                    __result = false;
            }
        }

        //Patch LoadMainScene on FejdStartup to load the menu asset bundle and get the prefabNames from ZNetScene
        [HarmonyPatch(typeof(FejdStartup), "LoadMainScene")]
        static class FejdStartupLoadMainScenePatch
        {
            static void Postfix()
            {
                Debug.Log("Easy spawner: Easy spawner loading assets");
                LoadAsset(assetBundleName);
                Debug.Log("Easy spawner: Easy spawner loaded assets");

                //Start thread to wait for ZNetScene then get prefabNames
                new Thread(() =>
                {
                    while (!ZNetScene.instance)
                        Thread.Sleep(100);

                    //If we do not have the list of prefabs, retrieve them from ZNetScene
                    if (prefabNames.Count == 0 && ZNetScene.instance)
                    {
                        foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
                        {
                            prefabNames.Add(prefab.name);
                        }
                    }
                }).Start();
            }
        }

        //Patch Shutdown on ZNetScene to destroy menu gameobject and unload assetbundle when user logs out back to main menu
        [HarmonyPatch(typeof(ZNetScene), "Shutdown")]
        static class ZNetSceneShutdownPatch
        {
            static void Prefix()
            {
                Debug.Log("Easy spawner: Easy spawner unloading assets");
                if(menuGameObject)
                {
                    searchSizeToggle.onValueChanged.RemoveAllListeners();
                    searchField.onValueChanged.RemoveAllListeners();
                    spawnButton.onClick.RemoveAllListeners();
                    Destroy(menuGameObject);
                }
                menuPrefab = null;
                menuAssetBundle.Unload(true);
                Debug.Log("Easy spawner: Easy spawner unloaded assets");
            }
        }
    }
}