using System;
using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class PowerGeneratorComponentPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PowerGeneratorComponent), "GameTick_Gamma")]
    public static bool PowerGeneratorGameTickGammaPatch(
        ref PowerGeneratorComponent __instance,
        bool useIon,
        bool useCata,
        PlanetFactory factory,
        int[] productRegister,
        int[] consumeRegister)
    {
      int ratio = 10;
      if (__instance.catalystPoint > 0)
      {
        int num1 = __instance.catalystPoint / 3600;
        if (useCata)
        {
          --__instance.catalystPoint;
        }
        int num2 = __instance.catalystPoint / 3600;
        lock (consumeRegister)
        {
          consumeRegister[__instance.catalystId] += num1 - num2;
        }
      }
      if (__instance.productId > 0 && (double)__instance.productCount < 20.0)
      {
        int productCount1 = (int)__instance.productCount;
        __instance.productCount += (float)__instance.capacityCurrentTick / (float)__instance.productHeat * ratio;
        int productCount2 = (int)__instance.productCount;
        lock (productRegister)
        {
          productRegister[__instance.productId] += productCount2 - productCount1;
        }
        if ((double)__instance.productCount > 20.0)
        {
          __instance.productCount = 20f;
        }
      }
      __instance.warmup += __instance.warmupSpeed * ratio;
      __instance.warmup = (double)__instance.warmup > 1.0 ? 1f : ((double)__instance.warmup < 0.0 ? 0.0f : __instance.warmup);
      bool flag1 = __instance.productId > 0 && (double)__instance.productCount >= 1.0;
      bool flag2 = useIon && (double)__instance.catalystPoint < 72000.0;
      if (!(flag1 | flag2))
      {
        return false;
      }
      bool isOutput1;
      int otherObjId1;
      factory.ReadObjectConn(__instance.entityId, 0, out isOutput1, out otherObjId1, out int _);
      bool isOutput2;
      int otherObjId2;
      factory.ReadObjectConn(__instance.entityId, 1, out isOutput2, out otherObjId2, out int _);
      bool flag3;
      bool flag4;
      if (otherObjId1 <= 0)
      {
        flag3 = false;
        flag4 = false;
        otherObjId1 = 0;
      }
      else
      {
        flag3 = isOutput1;
        flag4 = !isOutput1;
      }
      bool flag5;
      bool flag6;
      if (otherObjId2 <= 0)
      {
        flag5 = false;
        flag6 = false;
        otherObjId2 = 0;
      }
      else
      {
        flag5 = isOutput2;
        flag6 = !isOutput2;
      }
      if (flag1)
      {
        if (flag3 & flag5)
        {
          if (__instance.fuelHeat == 0L)
          {
            if (factory.InsertInto(otherObjId1, 0, __instance.productId))
            {
              --__instance.productCount;
              __instance.fuelHeat = 1L;
            }
            else if (factory.InsertInto(otherObjId2, 0, __instance.productId))
            {
              --__instance.productCount;
              __instance.fuelHeat = 0L;
            }
          }
          else if (factory.InsertInto(otherObjId2, 0, __instance.productId))
          {
            --__instance.productCount;
            __instance.fuelHeat = 0L;
          }
          else if (factory.InsertInto(otherObjId1, 0, __instance.productId))
          {
            --__instance.productCount;
            __instance.fuelHeat = 1L;
          }
        }
        else if (flag3)
        {
          if (factory.InsertInto(otherObjId1, 0, __instance.productId))
          {
            --__instance.productCount;
            __instance.fuelHeat = 1L;
          }
        }
        else if (flag5 && factory.InsertInto(otherObjId2, 0, __instance.productId))
        {
          --__instance.productCount;
          __instance.fuelHeat = 0L;
        }
      }
      if (!flag2)
      {
        return false;
      }
      if (flag4 && factory.PickFrom(otherObjId1, 0, __instance.catalystId, (int[])null) == __instance.catalystId)
      {
        __instance.catalystPoint += 3600;
      }
      if (!flag6 || factory.PickFrom(otherObjId2, 0, __instance.catalystId, (int[])null) != __instance.catalystId)
      {
        return false;
      }
      __instance.catalystPoint += 3600;
      return false;
    }

  }
}