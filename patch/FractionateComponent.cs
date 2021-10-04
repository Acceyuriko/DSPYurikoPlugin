using System;
using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class FractionateComponentPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(
        typeof(FractionateComponent),
        "InternalUpdate",
        new Type[] {
          typeof(PlanetFactory),
          typeof(float),
          typeof(SignData[]),
          typeof(int[]),
          typeof(int[])
        }
    )]
    public static bool FractionateInternalUpdatePatch(
        ref uint __result,
        ref FractionateComponent __instance,
        PlanetFactory factory,
        float power,
        SignData[] signPool,
        int[] productRegister,
        int[] consumeRegister
    )
    {
      if ((double)power < 0.100000001490116)
      {
        __result = 0;
        return false;
      }
      int threshold = 10000;
      if (__instance.fluidInputCount > 0 && __instance.productOutputCount < __instance.productOutputMax && __instance.fluidOutputCount < __instance.fluidOutputMax)
      {
        if (__instance.progress == 0)
        {
          __instance.isRand = true;
        }
        if (__instance.isRand)
        {
          __instance.seed = (uint)((ulong)(__instance.seed % 2147483646U + 1U) * 48271UL % (ulong)int.MaxValue) - 1U;
          __instance.fractionateSuccess = (double)__instance.seed / 2147483646.0 / 10 < (double)__instance.produceProb;
          __instance.isRand = false;
        }
        int num = (int)((double)power * (500.0 / 3.0) * (double)__instance.fluidInputCount + 0.5);
        __instance.progress += num;
        if (__instance.progress >= threshold)
        {
          if (__instance.fractionateSuccess)
          {
            ++__instance.productOutputCount;
            ++__instance.productOutputTotal;
            lock (productRegister)
            {
              ++productRegister[__instance.productId];
            }
            lock (consumeRegister)
            {
              ++consumeRegister[__instance.fluidId];
            }
          }
          else
          {
            ++__instance.fluidOutputCount;
            ++__instance.fluidOutputTotal;
          }
          --__instance.fluidInputCount;
          __instance.progress -= threshold;
          __instance.isRand = true;
        }
      }
      CargoTraffic cargoTraffic = factory.cargoTraffic;
      if (__instance.belt1 > 0)
      {
        if (__instance.isOutput1)
        {
          while (__instance.fluidOutputCount > 0 && cargoTraffic.TryInsertItemAtHead(__instance.belt1, __instance.fluidId))
          {
            --__instance.fluidOutputCount;
          }
        }
        else if (!__instance.isOutput1 && __instance.fluidInputCount < __instance.fluidInputMax)
        {
          if (__instance.fluidId > 0)
          {
            while (cargoTraffic.TryPickItemAtRear(__instance.belt1, __instance.fluidId, null) > 0)
            {
              ++__instance.fluidInputCount;
            }
          }
          else
          {
            int needId;
            while ((needId = cargoTraffic.TryPickItemAtRear(__instance.belt1, 0, RecipeProto.fractionateNeeds)) > 0)
            {
              ++__instance.fluidInputCount;
              if (__instance.fluidId == 0)
              {
                __instance.SetRecipe(needId, signPool);
              }
            }
          }
        }
      }
      if (__instance.belt2 > 0)
      {
        if (__instance.isOutput2)
        {
          while (__instance.fluidOutputCount > 0 && cargoTraffic.TryInsertItemAtHead(__instance.belt2, __instance.fluidId))
          {
            --__instance.fluidOutputCount;
          }
        }
        else if (!__instance.isOutput2 && __instance.fluidInputCount < __instance.fluidInputMax)
        {
          if (__instance.fluidId > 0)
          {
            while (cargoTraffic.TryPickItemAtRear(__instance.belt2, __instance.fluidId, null) > 0)
            {
              ++__instance.fluidInputCount;
            }
          }
          else
          {
            int needId;
            while ((needId = cargoTraffic.TryPickItemAtRear(__instance.belt2, 0, RecipeProto.fractionateNeeds)) > 0)
            {
              ++__instance.fluidInputCount;
              if (__instance.fluidId == 0)
              {
                __instance.SetRecipe(needId, signPool);
              }
            }
          }
        }
      }
      while (__instance.belt0 > 0 && __instance.isOutput0 && __instance.productOutputCount > 0 && cargoTraffic.TryInsertItemAtHead(__instance.belt0, __instance.productId))
      {
        --__instance.productOutputCount;
      }
      if (__instance.fluidInputCount == 0 && __instance.fluidOutputCount == 0 && __instance.productOutputCount == 0)
      {
        __instance.fluidId = 0;
      }
      __instance.isWorking = __instance.fluidInputCount > 0 && __instance.productOutputCount < __instance.productOutputMax && __instance.fluidOutputCount < __instance.fluidOutputMax;
      __result = !__instance.isWorking ? 0U : 1U;
      return false;
    }


  }
}