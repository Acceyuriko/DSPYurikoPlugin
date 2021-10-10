using System;
using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class InserterComponentPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(
        typeof(InserterComponent),
        "InternalUpdate",
        new Type[] { typeof(PlanetFactory), typeof(int[][]), typeof(AnimData[]), typeof(float) })
     ]
    public static bool InserterUpdatePatch(
        ref InserterComponent __instance,
        PlanetFactory factory,
        int[][] needsPool,
        AnimData[] animPool,
        float power)
    {
      return InserterUpdateCommonPatch(ref __instance, factory, needsPool, animPool, power);
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(InserterComponent), "InternalUpdateNoAnim", new Type[] { typeof(PlanetFactory), typeof(int[][]), typeof(float) })]
    public static bool InserterUpdateNoAnimPatch(
        ref InserterComponent __instance,
        PlanetFactory factory,
        int[][] needsPool,
        float power)
    {
      return InserterUpdateCommonPatch(ref __instance, factory, needsPool, null, power);
    }

    private static bool InserterUpdateCommonPatch(
        ref InserterComponent __instance,
        PlanetFactory factory,
        int[][] needsPool,
        AnimData[] animPool,
        float power)
    {
      if (animPool != null)
      {
        animPool[__instance.entityId].power = power;
        animPool[__instance.entityId].time = Math.Abs((float)(__instance.time % 60) / 60f);
      }
      if (power < 0.100000001490116)
      {
        return false;
      }
      if (__instance.pickTarget == 0 || __instance.insertTarget == 0)
      {
        if (animPool != null)
        {
          factory.entitySignPool[__instance.entityId].signType = 10;
        }
      }
      if (__instance.itemId == 0)
      {
        if (__instance.careNeeds)
        {
          int[] needs = needsPool[__instance.insertTarget];
          if (needs != null && (needs[0] != 0 || needs[1] != 0 || needs[2] != 0 || needs[3] != 0 || needs[4] != 0 || needs[5] != 0))
          {
            int num = factory.PickFrom(__instance.pickTarget, __instance.pickOffset, __instance.filter, needs);
            if (num > 0)
            {
              __instance.time++;
              __instance.itemId = num;
            }
          }
        }
        else
        {
          int num = factory.PickFrom(__instance.pickTarget, __instance.pickOffset, __instance.filter, null);
          if (num > 0)
          {
            __instance.itemId = num;
            __instance.time++;
          }
        }
      }
      if (__instance.itemId > 0)
      {
        if (factory.InsertInto(__instance.insertTarget, __instance.insertOffset, __instance.itemId))
        {
          __instance.itemId = 0;
          __instance.time++;
        }
      }
      if (animPool != null)
      {
        animPool[__instance.entityId].state = (uint)__instance.itemId;
      }
      return false;
    }
  }
}