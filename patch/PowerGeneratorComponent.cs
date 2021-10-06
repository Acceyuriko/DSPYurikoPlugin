using HarmonyLib;

namespace DSPYurikoPlugin {
  public class PowerGeneratorComponentPatch {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PowerGeneratorComponent), "EnergyCap_Gamma_Req")]
    public static void EnergyCap_Gamma_Req(ref PowerGeneratorComponent __instance) {
      __instance.warmupSpeed *= YurikoConstants.RAY_RECEIVER_RATIO;
    }
  }
}