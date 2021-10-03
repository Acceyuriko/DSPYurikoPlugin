using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class GameAbnormalityCheckPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameAbnormalityCheck), "isGameNormal")]
    public static bool isGameNormal(ref bool __result)
    {
      __result = true;
      return false;
    }
  }
}