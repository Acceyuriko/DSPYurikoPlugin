using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class ModelProtoPatch
  {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ModelProto), "Preload")]
    public static void Preload(ref ModelProto __instance)
    {
      if (__instance.prefabDesc != null)
      {
        if (__instance.prefabDesc.isPowerNode)
        {
          __instance.prefabDesc.powerConnectDistance *= YurikoConstants.POWER_NODE_CONN_RATIO;
          __instance.prefabDesc.powerCoverRadius *= YurikoConstants.POWER_NODE_COVER_RATIO;
        }
        else if (__instance.prefabDesc.isBelt)
        {
          __instance.prefabDesc.beltSpeed = YurikoConstants.BELT_SPEED;
        }
        else if (__instance.prefabDesc.isEjector)
        {
          __instance.prefabDesc.ejectorChargeFrame /= YurikoConstants.EJECTOR_RATIO;
          __instance.prefabDesc.ejectorColdFrame /= YurikoConstants.EJECTOR_RATIO;
        }
        else if (__instance.prefabDesc.isSilo)
        {
          __instance.prefabDesc.siloChargeFrame /= YurikoConstants.SILO_RATIO;
          __instance.prefabDesc.siloColdFrame /= YurikoConstants.SILO_RATIO;
        }
        else if (__instance.prefabDesc.isPowerGen)
        {
          __instance.prefabDesc.powerProductHeat /= YurikoConstants.RAY_RECEIVER_RATIO;
        }
      }
    }
  }
}