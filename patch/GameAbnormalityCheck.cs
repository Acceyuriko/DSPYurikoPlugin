using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class GameAbnormalityCheckPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameAbnormalityData), "IsGameNormal")]
    public static bool IsGameNormal(ref bool __result)
    {
      __result = true;
      return false;
    }
  }
}