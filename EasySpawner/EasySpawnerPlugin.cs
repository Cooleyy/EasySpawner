using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using EasySpawner.UI;
using EasySpawner.Config;

namespace EasySpawner
{
    [BepInPlugin("cooley.easyspawner", "Easy Spawner", "1.4.0")]
    [BepInProcess("valheim.exe")]
    public class EasySpawnerPlugin : BaseUnityPlugin
    {
        private static AssetBundle menuAssetBundle;
        public static GameObject menuPrefab;
        public static GameObject menuGameObject;

        public static EasySpawnerMenu menu = new EasySpawnerMenu();
        public static EasySpawnerConfig config = new EasySpawnerConfig();

        public static List<string> prefabNames = new List<string>();
        private static List<string> playerNames = new List<string>();

        //List of each set of gameobjects spawned. Each list correlates to a spawn action
        public static Stack<List<GameObject>> spawnActions = new Stack<List<GameObject>>();

        public static readonly string assetBundleName = "EasySpawnerAssetBundle";
        public const string favouritesFileName = "favouriteItems.txt";

        public Harmony harmony;

        void Awake()
        {
            Debug.Log("Easy spawner: Easy spawner loaded plugin");

            harmony = new Harmony("cooley.easyspawner");
            harmony.PatchAll();

            config.ConfigFile = Config;
            config.SetupConfig();
        }

        void Update()
        {
            if(Player.m_localPlayer)
            {
                if (config.IfMenuHotkeyPressed())
                {
                    if (!menuGameObject)
                        CreateMenu();
                    else
                        menuGameObject.SetActive(!menuGameObject.activeSelf);
                }
                else if (menuGameObject)
                {
                    if(config.IfSpawnHotkeyPressed())
                    {
                        Debug.Log("Spawn hotkey pressed");
                        menu.SpawnButton.onClick.Invoke();
                    }
                    else if (config.IfUndoHotkeyPressed() && spawnActions.Count > 0)
                    {
                        UndoSpawn();
                    }
                }
            }
        }

        public static AssetBundle GetAssetBundleFromResources(string fileName)
        {
            Assembly execAssembly = Assembly.GetExecutingAssembly();
            string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(fileName));
            using (Stream stream = execAssembly.GetManifestResourceStream(resourceName))
            {
                return AssetBundle.LoadFromStream(stream);
            }
        }

        public static void LoadAsset(string assetFileName)
        {
            if (menuAssetBundle)
            {
                Debug.Log("Easy spawner: menu asset bundle already loaded");
                return;
            }

            menuAssetBundle = GetAssetBundleFromResources(assetFileName);

            if (menuAssetBundle)
                Debug.Log("Easy spawner: menu asset bundle loaded");
            else
                Debug.Log("Easy spawner: menu asset bundle failed to load");
        }

        /// <summary>
        /// Load favourite prefabs from file, into PrefabState dictionary
        /// </summary>
        public static void LoadFavourites()
        {
            Debug.Log("Easy spawner: load favourite Items from file");
            Assembly assembly = typeof(EasySpawnerPlugin).Assembly;
            string pathToFile = Path.Combine(Paths.PluginPath, Path.GetDirectoryName(assembly.Location), favouritesFileName);
            if (!File.Exists(pathToFile)) return;

            using (StreamReader file = File.OpenText(pathToFile))
            {
                while (!file.EndOfStream)
                {
                    string prefabName = file.ReadLine();

                    if (prefabName == null || !EasySpawnerMenu.PrefabStates.ContainsKey(prefabName))
                    {
                        Debug.Log("Easy spawner: favourite prefab '"+ prefabName + "' not found");
                        continue;
                    }

                    EasySpawnerMenu.PrefabStates[prefabName].isFavourite = true;
                }
            }
        }

        /// <summary>
        /// Saves favourite prefabs to file, from PrefabState dictionary
        /// </summary>
        public static void SaveFavourites()
        {
            Debug.Log("Easy spawner: save favourite Items to file");
            Assembly assembly = typeof(EasySpawnerPlugin).Assembly;
            string pathToFile = Path.Combine(Paths.PluginPath, Path.GetDirectoryName(assembly.Location), favouritesFileName);

            using (StreamWriter file = File.CreateText(pathToFile))
            {
                foreach (KeyValuePair<string, PrefabState> pair in EasySpawnerMenu.PrefabStates)
                {
                    if(!pair.Value.isFavourite) continue;

                    file.WriteLine(pair.Key);
                }
            }
        }

        private void CreateMenu()
        {
            Debug.Log("Easy spawner: Loading menu prefab");

            if (!menuAssetBundle)
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
            if (!menuPrefab.GetComponent<UIElementDragger>())
                menuPrefab.AddComponent<UIElementDragger>();

            menuGameObject = Instantiate(menuPrefab);

            //Attach menu to Valheims UI gameobject
            var uiGO = GameObject.Find("IngameGui(Clone)");

            if (!uiGO)
            {
                Debug.Log("Easy spawner: Couldnt find UI gameobject");
                return;
            }

            menuGameObject.transform.SetParent(uiGO.transform);
            menuGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            menuGameObject.transform.localPosition = new Vector3(-650, 0, 0);

            playerNames = GetPlayerNames();

            menu.CreateMenu(menuGameObject);

            //Attach CheckForNewPlayersCoroutine to UIElementdragger on the menu Gameobject so if it gets destroyed it also stops the coroutine
            menuGameObject.GetComponent<UIElementDragger>().StartCoroutine(CheckForNewPlayersCoroutine());
        }

        public static void SpawnPrefab(string prefabName, Player player, int amount = 1, int level = 1, bool pickup = false, bool ignoreStackSize = false)
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
                List<GameObject> spawnedObjects = new List<GameObject>();

                //If prefab is an npc/enemy
                if (prefab.GetComponent<Character>())
                {
                    for (int i = 0; i < amount; i++)
                    {
                        GameObject spawnedChar = Instantiate(prefab, player.transform.position + player.transform.forward * 2f + Vector3.up, Quaternion.identity);
                        Character character = spawnedChar.GetComponent<Character>();
                        if (level > 1)
                            character.SetLevel(level);
                        spawnedObjects.Add(spawnedChar);
                    }
                }
                //if prefab is an item
                else if (prefab.GetComponent<ItemDrop>())
                {
                    ItemDrop itemPrefab = prefab.GetComponent<ItemDrop>();
                    if (itemPrefab.m_itemData.IsEquipable())
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
                            spawnedObjects.Add(SpawnItem(pickup, prefab, player));
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

                            //Some items maxStackSize incorrectly set to 0
                            if (maxStack < 1)
                                maxStack = 1;

                            itemPrefab.m_itemData.m_stack = maxStack;
                            noOfStacks = amount / maxStack;
                            lastStack = amount % maxStack;
                        }

                        for (int i = 0; i < noOfStacks; i++)
                        {
                            spawnedObjects.Add(SpawnItem(pickup, prefab, player));
                        }
                        if (lastStack != 0)
                        {
                            itemPrefab.m_itemData.m_stack = lastStack;
                            spawnedObjects.Add(SpawnItem(pickup, prefab, player));
                        }
                    }
                    itemPrefab.m_itemData.m_stack = 1;
                    itemPrefab.m_itemData.m_quality = 1;
                }
                else
                {
                    for (int i = 0; i < amount; i++)
                    {
                        spawnedObjects.Add(Instantiate(prefab, player.transform.position + player.transform.forward * 2f, Quaternion.identity));
                    }
                }

                spawnActions.Push(spawnedObjects);
                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Spawning object " + prefabName);
                Debug.Log("Easy spawner: Spawned " + amount + " " + prefabName);
            }
        }

        private static GameObject SpawnItem(bool pickup, GameObject prefab, Player player)
        {
            if (pickup)
            {
                Player.m_localPlayer.PickupPrefab(prefab);
                return null;
            }
            else
                return Instantiate(prefab, player.transform.position + player.transform.forward * 2f + Vector3.up, Quaternion.identity);
        }

        private void UndoSpawn()
        {
            Debug.Log("Easyspawner: Undo spawn action");

            if (spawnActions.Count <= 0)
                return;

            List<GameObject> spawnedGameObjects = spawnActions.Pop();

            Debug.Log("Easyspawner: Destroying " + spawnedGameObjects.Count + " objects");
            string objectName = "objects";

            foreach (GameObject GO in spawnedGameObjects)
            {
                if (GO != null)
                {
                    objectName = GO.name.Remove(GO.name.Length - 7);
                    ZNetView zNetV = GO.GetComponent<ZNetView>();
                    if (zNetV && zNetV.IsValid() && zNetV.IsOwner())
                        zNetV.Destroy();
                }
            }

            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Undo spawn of " + spawnedGameObjects.Count + " " + objectName);
            Debug.Log("Easyspawner: Spawn undone");
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
            menu.RebuildPlayerDropdown();
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
                    if (newPlayerNames.Count != playerNames.Count)
                    {
                        PlayerListChanged();
                        continue;
                    }

                    foreach (string name in newPlayerNames)
                    {
                        if (!playerNames.Contains(name))
                        {
                            PlayerListChanged();
                            break;
                        }
                    }
                }
            }
            Debug.Log("EasySpawner: Stopping check for new players coroutine");
        }

        public static void DestroyMenu()
        {
            Debug.Log("Easy spawner: Easy spawner unloading assets");

            if (menuGameObject)
            {
                menu.Destroy();
                Destroy(menuGameObject);
            }

            menuPrefab = null;

            if (menuAssetBundle)
                menuAssetBundle.Unload(true);

            Debug.Log("Easy spawner: Easy spawner unloaded assets");
        }

        private void OnDestroy()
        {
            StopAllCoroutines();

            if (menuGameObject != null)
                DestroyMenu();

            if (harmony != null)
                harmony.UnpatchSelf();
        }
    }
}