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
      }
      if (power < 0.100000001490116)
      {
        return false;
      }
      int stt = __instance.stt / 10;
      if (stt < 1)
      {
        stt = 1;
      }
      switch (__instance.stage)
      {
        case EInserterStage.Picking:
          if (animPool != null)
          {
            animPool[__instance.entityId].time = 0f;
          }
          if (__instance.pickTarget == 0 || __instance.insertTarget == 0)
          {
            if (animPool != null)
            {
              factory.entitySignPool[__instance.entityId].signType = 10;
            }
            break;
          }
          if (__instance.itemId == 0 || __instance.stackCount < __instance.stackSize)
          {
            if (__instance.careNeeds)
            {
              int[] needs = needsPool[__instance.insertTarget];
              if (needs != null && (needs[0] != 0 || needs[1] != 0 || needs[2] != 0 || needs[3] != 0 || needs[4] != 0 || needs[5] != 0))
              {
                int num;
                while (__instance.stackCount < __instance.stackSize &&
                    (num = factory.PickFrom(__instance.pickTarget, __instance.pickOffset, __instance.filter, needs)) > 0)
                {
                  if (__instance.itemId == 0)
                  {
                    __instance.itemId = num;
                  }
                  ++__instance.stackCount;
                  __instance.time = 0;
                }
              }
            }
            else
            {
              int num;
              while (__instance.stackCount < __instance.stackSize &&
                  (num = factory.PickFrom(__instance.pickTarget, __instance.pickOffset, __instance.filter, null)) > 0)
              {
                if (__instance.itemId == 0)
                {
                  __instance.itemId = num;
                }
                ++__instance.stackCount;
                __instance.time = 0;
              }
            }
          }
          if (__instance.itemId > 0)
          {
            __instance.time += __instance.speed;
            if (__instance.stackCount == __instance.stackSize || __instance.time >= __instance.delay)
            {
              __instance.time = (int)((double)power * __instance.speed);
              __instance.stage = EInserterStage.Sending;
              break;
            }
            break;
          }
          __instance.time = 0;
          break;
        case EInserterStage.Sending:
          __instance.time += (int)((double)power * __instance.speed);
          if (__instance.time >= stt)
          {
            __instance.stage = EInserterStage.Inserting;
            __instance.time -= stt;
            if (animPool != null)
            {
              animPool[__instance.entityId].time = 0.5f;
            }
          }
          else
          {
            if (animPool != null)
            {
              animPool[__instance.entityId].time = __instance.time / (stt * 2f);
            }
          }
          if (__instance.itemId == 0)
          {
            __instance.stage = EInserterStage.Returning;
            __instance.time = stt - __instance.time;
            break;
          }
          break;
        case EInserterStage.Inserting:
          if (animPool != null)
          {
            animPool[__instance.entityId].time = 0.5f;
          }
          if (__instance.insertTarget == 0)
          {
            if (animPool != null)
            {
              factory.entitySignPool[__instance.entityId].signType = 10;
            }
            break;
          }
          if (__instance.itemId == 0 || __instance.stackCount == 0)
          {
            __instance.itemId = 0;
            __instance.stackCount = 0;
            __instance.time += (int)((double)power * __instance.speed);
            __instance.stage = EInserterStage.Returning;
            break;
          }
          while (factory.InsertInto(__instance.insertTarget, __instance.insertOffset, __instance.itemId))
          {
            --__instance.stackCount;
            if (__instance.stackCount == 0)
            {
              __instance.itemId = 0;
              __instance.time += (int)((double)power * __instance.speed);
              __instance.stage = EInserterStage.Returning;
              break;
            }
          }
          break;
        case EInserterStage.Returning:
          __instance.time += (int)((double)power * __instance.speed);
          if (__instance.time >= stt)
          {
            __instance.stage = EInserterStage.Picking;
            __instance.time = 0;
            if (animPool != null)
            {
              animPool[__instance.entityId].time = 0.0f;
            }
            break;
          }
          if (animPool != null)
          {
            animPool[__instance.entityId].time = (__instance.time + stt) / (stt * 2f);
          }
          break;
      }
      if (animPool != null)
      {
        animPool[__instance.entityId].state = (uint)__instance.itemId;
      }
      return false;
    }

  }
}