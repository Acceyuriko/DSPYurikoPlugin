using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class GameAbnormalityCheckPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameAbnormalityCheck_Obsolete), "isGameNormal")]
    public static bool isGameNormal(ref bool __result)
    {
      __result = true;
      return false;
    }
  }
}