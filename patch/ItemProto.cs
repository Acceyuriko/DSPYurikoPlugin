using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class ItemProtoPatch
  {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ItemProto), "Preload")]
    public static void Preload(ref ItemProto __instance)
    {
      if (__instance.prefabDesc != null) {
        if (__instance.prefabDesc.isStation) {
          __instance.prefabDesc.stationMaxItemCount *= YurikoConstants.STATION_MAX_ITEM_COUNT_RATIO;
        }
      }
    }
  }
}