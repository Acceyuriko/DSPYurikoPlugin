using System;
using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class AssemblerComponentPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AssemblerComponent), "InternalUpdate", new Type[] { typeof(float), typeof(int[]), typeof(int[]) })]
    public static bool AssemblerUpdatePatch(
        ref uint __result,
        ref AssemblerComponent __instance,
        float power,
        int[] productRegister,
        int[] consumeRegister
        )
    {
      if (power < 0.100000001490116)
      {
        __result = 0U;
        return false;
      }
      int timeSpend = __instance.timeSpend;
      if (__instance.recipeType == ERecipeType.Smelt)
      {
        timeSpend /= 10;
      }
      else if (__instance.recipeType == ERecipeType.Assemble)
      {
        timeSpend /= 10;
      }
      else if (__instance.recipeType == ERecipeType.Chemical)
      {
        timeSpend /= 10;
      }
      else if (__instance.recipeType == ERecipeType.Refine)
      {
        timeSpend /= 10;
      }
      else if (__instance.recipeType == ERecipeType.Particle)
      {
        timeSpend /= 10;
      }
      uint num1 = 0;
      if (!__instance.replicating)
      {
        for (int index = 0; index < __instance.requireCounts.Length; ++index)
        {
          if (__instance.served[index] < __instance.requireCounts[index])
          {
            __instance.time = 0;
            __result = 0U;
            return false;
          }
        }
        for (int index = 0; index < __instance.requireCounts.Length; ++index)
        {
          int requireCount = __instance.requireCounts[index];
          __instance.served[index] -= requireCount;
          lock (consumeRegister)
          {
            consumeRegister[__instance.requires[index]] += requireCount;
          }
        }
        __instance.replicating = true;
      }

      if (__instance.replicating && !__instance.outputing)
      {
        if (__instance.time < timeSpend)
        {
          __instance.time += (int)((double)power * (double)__instance.speed);
          num1 = 1U;
        }
        if (__instance.time >= timeSpend)
        {
          __instance.outputing = true;
        }
      }

      if (__instance.outputing)
      {
        if (__instance.recipeType == ERecipeType.Smelt)
        {
          for (int index = 0; index < __instance.productCounts.Length; ++index)
          {
            if (__instance.produced[index] + __instance.productCounts[index] > 100)
            {
              __result = 0U;
              return false;
            }
          }
        }
        else if (__instance.recipeType == ERecipeType.Assemble)
        {
          for (int index = 0; index < __instance.productCounts.Length; ++index)
          {
            if (__instance.produced[index] > __instance.productCounts[index] * 9)
            {
              __result = 0U;
              return false;
            }
          }
        }
        else if (__instance.recipeType == ERecipeType.Chemical)
        {
          for (int index = 0; index < __instance.productCounts.Length; ++index)
          {
            if (__instance.produced[index] > __instance.productCounts[index] * 19)
            {
              __result = 0U;
              return false;
            }
          }
        }
        else if (__instance.recipeType == ERecipeType.Refine)
        {
          for (int index = 0; index < __instance.productCounts.Length; ++index)
          {
            if (__instance.produced[index] > __instance.productCounts[index] * 19)
            {
              __result = 0U;
              return false;
            }
          }
        }
        else if (__instance.recipeType == ERecipeType.Particle)
        {
          for (int index = 0; index < __instance.productCounts.Length; ++index)
          {
            if (__instance.produced[index] > __instance.productCounts[index] * 19)
            {
              __result = 0U;
              return false;
            }
          }
        }
        else
        {
          for (int index = 0; index < __instance.productCounts.Length; ++index)
          {
            if (__instance.produced[index] > __instance.productCounts[index] * 19)
            {
              __result = 0U;
              return false;
            }
          }
        }
        for (int index = 0; index < __instance.productCounts.Length; ++index)
        {
          int productCount = __instance.productCounts[index];
          __instance.produced[index] += productCount;
          lock (productRegister)
            productRegister[__instance.products[index]] += productCount;
        }
        __instance.outputing = false;
        __instance.time -= timeSpend;
        __instance.replicating = false;
        for (int index = 0; index < __instance.requireCounts.Length; ++index)
        {
          if (__instance.served[index] < __instance.requireCounts[index])
          {
            __instance.time = 0;
            __result = 0U;
            return false;
          }
        }
        for (int index = 0; index < __instance.requireCounts.Length; ++index)
        {
          int requireCount = __instance.requireCounts[index];
          __instance.served[index] -= requireCount;
          lock (consumeRegister)
            consumeRegister[__instance.requires[index]] += requireCount;
        }
        __instance.replicating = true;
      }

      __result = num1;
      return false;
    }

  }
}