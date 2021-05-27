using HarmonyLib;
using UnityEngine;

namespace EasySpawner.Patches
{
    // Patch GetButton on ZInput to refuse player input if any input fields on the menu are being interacted with
    [HarmonyPatch]
    public static class ZInputPatcher
    {
        public static bool EasySpawnerIsFocused()
        {
            if (EasySpawnerPlugin.menuGameObject == null || !EasySpawnerPlugin.menuGameObject.activeSelf)
                return false;
            return EasySpawnerPlugin.menu.SearchField.isFocused || EasySpawnerPlugin.menu.AmountField.isFocused ||
                   EasySpawnerPlugin.menu.LevelField.isFocused;
        }

        [HarmonyPatch(typeof(ZInput), "GetButtonDown"), HarmonyPostfix]
        public static void GetButtonDownPatch(ref bool __result)
        {
            if (EasySpawnerIsFocused())
                __result = false;
        }

        [HarmonyPatch(typeof(ZInput), "GetButtonUp"), HarmonyPostfix]
        public static void GetButtonUpPatch(ref bool __result)
        {
            if (EasySpawnerIsFocused())
                __result = false;
        }

        [HarmonyPatch(typeof(ZInput), "GetButton"), HarmonyPostfix]
        public static void GetButtonPatch(ref bool __result)
        {
            if (EasySpawnerIsFocused())
                __result = false;
        }
    }
}
