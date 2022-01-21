using HarmonyLib;
using System;

namespace DSPYurikoPlugin
{
  public class StorageComponentPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StorageComponent), "AddItem", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) })]
    public static bool AddItem(
      ref int __result,
      StorageComponent __instance,
      int itemId,
      int count,
      int inc,
      bool useBan,
      object[] __args
    )
    {
      if (!useBan || __instance.size != __instance.bans || itemId == 0 || (uint)count <= 0U)
      {
        return true;
      }
      __args[3] = inc;
      // GameMain.mainPlayer.SetSandCount(GameMain.mainPlayer.sandCount + 100);
      __result = 1;
      return false;
    }
  }
}