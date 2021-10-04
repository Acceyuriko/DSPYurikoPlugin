using HarmonyLib;

namespace DSPYurikoPlugin {
  public class PowerSystemPatch {

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PowerSystem), "NewNodeComponent")]
    public static bool NewNodeComponent(int entityId, float conn, float cover) {
      conn *= YurikoConstants.POWER_NODE_CONN_RATIO;
      cover *= YurikoConstants.POWER_NODE_COVER_RATIO;
      return true;
    }
  }
}