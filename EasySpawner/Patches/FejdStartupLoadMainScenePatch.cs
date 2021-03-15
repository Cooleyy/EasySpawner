using System.Threading;
using HarmonyLib;
using UnityEngine;

namespace EasySpawner.Patches
{
    //Patch LoadMainScene on FejdStartup to load the menu asset bundle and get the prefabNames from ZNetScene
    [HarmonyPatch(typeof(FejdStartup), "LoadMainScene")]
    class FejdStartupLoadMainScenePatch
    {
        private static void Postfix()
        {
            Debug.Log("Easy spawner: Easy spawner loading assets");
            EasySpawnerPlugin.LoadAsset(EasySpawnerPlugin.assetBundleName);
            Debug.Log("Easy spawner: Easy spawner loaded assets");

            //Start thread to wait for ZNetScene then get prefabNames
            new Thread(() =>
            {
                while (!ZNetScene.instance)
                    Thread.Sleep(100);

                //If we do not have the list of prefabs, retrieve them from ZNetScene
                if (EasySpawnerPlugin.prefabNames.Count == 0 && ZNetScene.instance)
                {
                    foreach (GameObject prefab in ZNetScene.instance.m_prefabs)
                    {
                        EasySpawnerPlugin.prefabNames.Add(prefab.name);
                    }
                }
            }).Start();
        }
    }
}
