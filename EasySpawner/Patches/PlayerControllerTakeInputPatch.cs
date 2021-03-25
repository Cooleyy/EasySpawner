using HarmonyLib;

namespace EasySpawner.Patches
{
    //Patch TakeInput on PlayerController to refuse player input if any input fields on the menu are being interacted with
    [HarmonyPatch(typeof(PlayerController), "TakeInput")]
    static class PlayerControllerTakeInputPatch
    {
        private static void Postfix(ref bool __result)
        {
            if (EasySpawnerPlugin.menuGameObject != null && EasySpawnerPlugin.menuActive && (EasySpawnerPlugin.menu.SearchField.isFocused || EasySpawnerPlugin.menu.AmountField.isFocused || EasySpawnerPlugin.menu.LevelField.isFocused))
                __result = false;
        }
    }
}
