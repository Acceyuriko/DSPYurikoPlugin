using HarmonyLib;
using System;

namespace DSPYurikoPlugin
{
  public class StorageComponentPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StorageComponent), "AddItem", new Type[] { typeof(int), typeof(int), typeof(bool) })]
    public static bool DoTrash(
      ref int __result,
      StorageComponent __instance,
      int itemId,
      int count,
      bool useBan = false
    )
    {
      if (!useBan || __instance.size != __instance.bans || itemId == 0 || (uint)count <= 0U)
      {
        return true;
      }
      // GameMain.mainPlayer.SetSandCount(GameMain.mainPlayer.sandCount + 100);
      __result = 1;
      return false;
    }
  }
}