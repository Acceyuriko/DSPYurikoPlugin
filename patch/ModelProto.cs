using HarmonyLib;

namespace DSPYurikoPlugin {
  public class ModelProtoPatch {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ModelProto), "Preload")]
    public static void Preload(ref ModelProto __instance) {
      if (__instance.prefabDesc != null) {
        if (__instance.prefabDesc.isPowerNode) {
          __instance.prefabDesc.powerConnectDistance *= YurikoConstants.POWER_NODE_CONN_RATIO;
          __instance.prefabDesc.powerCoverRadius *= YurikoConstants.POWER_NODE_COVER_RATIO;
        } else if (__instance.prefabDesc.isBelt) {
          __instance.prefabDesc.beltSpeed = YurikoConstants.BELT_SPEED;
        } else if (__instance.prefabDesc.isEjector) {
          YurikoLogging.logger.LogInfo($"charge: {__instance.prefabDesc.ejectorChargeFrame}, cold: {__instance.prefabDesc.ejectorColdFrame}");
          __instance.prefabDesc.ejectorChargeFrame /= 10;
          __instance.prefabDesc.ejectorColdFrame /= 10;
        }
      }
    }
  }
}