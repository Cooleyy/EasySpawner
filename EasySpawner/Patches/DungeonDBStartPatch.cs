﻿using HarmonyLib;
using UnityEngine;
using EasySpawner.UI;

namespace EasySpawner.Patches
{
    //Patch Start on DungeonDB to load the menu asset bundle and get the prefabNames from ZNetScene
    [HarmonyPatch(typeof(DungeonDB), "Start")]
    class DungeonDB_Start_Patch
    {
        private static void Postfix()
        {
            Debug.Log("Easy spawner: Easy spawner loading assets");
            EasySpawnerPlugin.LoadAsset(EasySpawnerPlugin.assetBundleName);
            Debug.Log("Easy spawner: Easy spawner loaded assets");

            if (EasySpawnerPlugin.prefabNames.Count == 0)
            {
                if (!ZNetScene.instance)
                {
                    Debug.LogError("EasySpawner: Can't load prefab due to ZNetScene.instance is null");
                }
                else
                {
                    foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
                    {
                        string localizedName = "";

                        if (prefab.TryGetComponent(out ItemDrop itemDrop))
                            localizedName = Localization.instance.Localize(itemDrop.m_itemData.m_shared.m_name);

                        if (prefab.TryGetComponent(out Piece piece))
                            localizedName = Localization.instance.Localize(piece.m_name);

                        EasySpawnerPlugin.prefabNames.Add(prefab.name);
                        EasySpawnerMenu.PrefabStates.Add(prefab.name, new PrefabState() { localizedName = localizedName });
                    }

                    EasySpawnerPlugin.prefabNames.Sort();
                    EasySpawnerPlugin.LoadFavourites();
                }
            }
        }
    }
}
