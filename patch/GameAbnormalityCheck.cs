using HarmonyLib;
using ABN;

namespace DSPYurikoPlugin
{
  public class GameAbnormalityCheckPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameAbnormalityData_0925), "NothingAbnormal")]
    public static bool IsGameNormal(ref bool __result)
    {
      __result = true;
      return false;
    }
  }
}