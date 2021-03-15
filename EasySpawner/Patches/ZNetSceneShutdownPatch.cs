using HarmonyLib;
using UnityEngine;

namespace EasySpawner.Patches
{
    //Patch Shutdown on ZNetScene to destroy menu gameobject and unload assetbundle when user logs out back to main menu
    [HarmonyPatch(typeof(ZNetScene), "Shutdown")]
    class ZNetSceneShutdownPatch
    {
        private static void Prefix()
        {
            EasySpawnerPlugin.DestroyMenu();
        }
    }
}
