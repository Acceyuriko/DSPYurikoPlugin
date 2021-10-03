using System;
using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class StationComponentPatch
  {
    // 物流站耗电
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StationComponent), "CalcTripEnergyCost", new Type[] { typeof(double), typeof(float), typeof(bool) })]
    public static bool StationCalcTripEnergyCost(
      ref StationComponent __instance,
      ref long __result,
      double trip,
      float maxSpeed,
      bool canWarp
    )
    {
      double num1 = trip * 0.03 + 100.0;
      if (num1 > (double)maxSpeed)
        num1 = (double)maxSpeed;
      if (num1 > 3000.0)
        num1 = 3000.0;
      double num2 = num1 * 200000.0;
      __result = (long)(6000000.0 + trip * 30.0 + num2) / 10;
      return false;
    }

    // 轨道采集器
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StationComponent), "UpdateCollection")]
    public static bool StationUpdateCollectionPatch(ref float collectSpeedRate)
    {
      collectSpeedRate *= 10;
      return true;
    }
  }
}