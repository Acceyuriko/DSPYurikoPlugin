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
        } else if (__instance.prefabDesc.isAssembler) {
          __instance.prefabDesc.assemblerSpeed *= YurikoConstants.ASSEMBLE_SPEED_RATIO;
        }
      }
    }
  }
}