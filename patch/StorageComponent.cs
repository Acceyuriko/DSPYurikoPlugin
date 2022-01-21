using HarmonyLib;
using System;

namespace DSPYurikoPlugin
{
    public class StorageComponentPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(
          typeof(StorageComponent),
          "AddItem",
          new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) },
          new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal }
        )]
        public static bool AddItem(
          ref int __result,
          StorageComponent __instance,
          int itemId,
          int count,
          int inc,
          out int remainInc,
          bool useBan
        )
        {
            remainInc = inc;
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