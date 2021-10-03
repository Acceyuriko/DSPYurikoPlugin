using System;
using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class LabComponentPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LabComponent), "InternalUpdateAssemble", new Type[] { typeof(float), typeof(int[]), typeof(int[]) })]
    public static bool LabInternalUpdateAssemblePatch(
        ref uint __result,
        ref LabComponent __instance,
        float power,
        int[] productRegister,
        int[] consumeRegister)
    {
      if ((double)power < 0.100000001490116)
      {
        __result = 0U;
        return false;
      }
      uint num = 0;
      int timeSpend = __instance.timeSpend / 10;
      if (!__instance.replicating)
      {
        for (int i = 0; i < __instance.requireCounts.Length; ++i)
        {
          if (__instance.served[i] < __instance.requireCounts[i])
          {
            __instance.time = 0;
            __result = 0U;
            return false;
          }
        }
        for (int i = 0; i < __instance.requireCounts.Length; ++i)
        {
          int requireCount = __instance.requireCounts[i];
          __instance.served[i] -= requireCount;
          lock (consumeRegister)
            consumeRegister[__instance.requires[i]] += requireCount;
        }
        __instance.replicating = true;
      }
      if (__instance.replicating && !__instance.outputing)
      {
        if (__instance.time < timeSpend)
        {
          __instance.time += (int)((double)power * 10000.0);
          num = (uint)(__instance.products[0] - LabComponent.matrixIds[0] + 1);
        }
        if (__instance.time >= timeSpend)
        {
          __instance.outputing = true;
        }
      }
      if (__instance.outputing)
      {
        for (int i = 0; i < __instance.productCounts.Length; ++i)
        {
          if (__instance.produced[i] + __instance.productCounts[i] > 10)
          {
            __result = 0U;
            return false;
          }
        }
        for (int i = 0; i < __instance.productCounts.Length; ++i)
        {
          int productCount = __instance.productCounts[i];
          __instance.produced[i] += productCount;
          lock (productRegister)
          {
            productRegister[__instance.products[i]] += productCount;
          }
        }
        __instance.outputing = false;
        __instance.time -= timeSpend;
        __instance.replicating = false;
        for (int i = 0; i < __instance.requireCounts.Length; ++i)
        {
          if (__instance.served[i] < __instance.requireCounts[i])
          {
            __instance.time = 0;
            __result = 0;
            return false;
          }
        }
        for (int i = 0; i < __instance.requireCounts.Length; ++i)
        {
          int requireCount = __instance.requireCounts[i];
          __instance.served[i] -= requireCount;
          lock (consumeRegister)
          {
            consumeRegister[__instance.requires[i]] += requireCount;
          }
        }
        __instance.replicating = true;
      }
      __result = num;
      return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LabComponent), "InternalUpdateResearch")]
    public static bool LabInternalUpdateResearchPatch(
      ref LabComponent __instance,
      ref uint __result,
      float power,
      float speed,
      int[] consumeRegister,
      ref TechState ts,
      ref int techHashedThisFrame,
      ref long uMatrixPoint,
      ref long hashRegister
    )
    {
      __instance.outputing = false;
      if ((double)power < 0.100000001490116)
      {
        __result = 0;
        return false;
      }
      int num1 = (int)((double)speed + 2.0);
      if (__instance.matrixPoints[0] > 0)
      {
        int num2 = __instance.matrixServed[0] / __instance.matrixPoints[0];
        if (num2 < num1)
        {
          num1 = num2;
          if (num1 == 0)
          {
            __instance.replicating = false;
            __result = 0;
            return false;
          }
        }
      }
      if (__instance.matrixPoints[1] > 0)
      {
        int num3 = __instance.matrixServed[1] / __instance.matrixPoints[1];
        if (num3 < num1)
        {
          num1 = num3;
          if (num1 == 0)
          {
            __instance.replicating = false;
            __result = 0;
            return false;
          }
        }
      }
      if (__instance.matrixPoints[2] > 0)
      {
        int num4 = __instance.matrixServed[2] / __instance.matrixPoints[2];
        if (num4 < num1)
        {
          num1 = num4;
          if (num1 == 0)
          {
            __instance.replicating = false;
            __result = 0;
            return false;
          }
        }
      }
      if (__instance.matrixPoints[3] > 0)
      {
        int num5 = __instance.matrixServed[3] / __instance.matrixPoints[3];
        if (num5 < num1)
        {
          num1 = num5;
          if (num1 == 0)
          {
            __instance.replicating = false;
            __result = 0;
            return false;
          }
        }
      }
      if (__instance.matrixPoints[4] > 0)
      {
        int num6 = __instance.matrixServed[4] / __instance.matrixPoints[4];
        if (num6 < num1)
        {
          num1 = num6;
          if (num1 == 0)
          {
            __instance.replicating = false;
            __result = 0;
            return false;
          }
        }
      }
      if (__instance.matrixPoints[5] > 0)
      {
        int num7 = __instance.matrixServed[5] / __instance.matrixPoints[5];
        if (num7 < num1)
        {
          num1 = num7;
          if (num1 == 0)
          {
            __instance.replicating = false;
            __result = 0;
            return false;
          }
        }
      }
      __instance.replicating = true;
      speed = (double)speed < (double)num1 ? speed : (float)num1;
      __instance.hashBytes += (int)((double)power * 10000.0 * (double)speed + 0.5);
      long num8 = (long)(__instance.hashBytes / 10000);
      __instance.hashBytes -= (int)num8 * 10000;
      long num9 = ts.hashNeeded - ts.hashUploaded;
      long num10 = num8 < num9 ? num8 : num9;
      long num11 = num10 < (long)num1 ? num10 : (long)num1;
      if (num11 > 0)
      {
        for (int index = 0; index < __instance.matrixServed.Length; ++index)
        {
          int num13 = __instance.matrixServed[index] / 3600;
          __instance.matrixServed[index] -= __instance.matrixPoints[index] * (int)num11;
          int num14 = __instance.matrixServed[index] / 3600;
          consumeRegister[LabComponent.matrixIds[index]] += num13 - num14;
        }
        num11 *= 10;
        ts.hashUploaded += num11;
        hashRegister += num11;
        uMatrixPoint += (long)ts.uPointPerHash * num11;
        techHashedThisFrame += (int)num11;
        if (ts.hashUploaded >= ts.hashNeeded)
        {
          TechProto techProto = LDB.techs.Select(__instance.techId);
          if (ts.curLevel >= ts.maxLevel)
          {
            ts.curLevel = ts.maxLevel;
            ts.hashUploaded = ts.hashNeeded;
            ts.unlocked = true;
          }
          else
          {
            ++ts.curLevel;
            ts.hashUploaded = 0L;
            ts.hashNeeded = techProto.GetHashNeeded(ts.curLevel);
          }
        }
      }
      __result = 1;
      return false;
    }
  }
}