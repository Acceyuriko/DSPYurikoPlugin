using System;
using HarmonyLib;
using UnityEngine;

namespace DSPYurikoPlugin
{
  public class SiloComponentPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SiloComponent), "InternalUpdate", new Type[] { typeof(float), typeof(DysonSphere), typeof(AnimData[]), typeof(int[]) })]
    public static bool SiloInternalUpdatePatch(
        ref uint __result,
        ref SiloComponent __instance,
        float power,
        DysonSphere sphere,
        AnimData[] animPool,
        int[] consumeRegister
        )
    {
      int ratio = 10;
      int coldSpend = __instance.coldSpend / ratio;
      int chargeSpend = __instance.chargeSpend / ratio;

      if (__instance.needs == null)
      {
        __instance.needs = new int[6];
      }
      __instance.needs[0] = __instance.bulletCount >= 20 ? 0 : __instance.bulletId;
      if (__instance.fired && __instance.direction != -1)
      {
        __instance.fired = false;
      }
      if (__instance.direction == 1)
      {
        animPool[__instance.entityId].time = (float)__instance.time / (float)chargeSpend;
      }
      else if (__instance.direction == -1)
      {
        animPool[__instance.entityId].time = (float)-(double)__instance.time / (float)coldSpend;
      }
      animPool[__instance.entityId].power = power;
      int num1 = (int)((double)power * 10000.0 + 0.100000001490116);
      lock (sphere.dysonSphere_mx)
      {
        __instance.hasNode = sphere.GetAutoNodeCount() > 0;
        if (!__instance.hasNode)
        {
          __instance.autoIndex = 0;
          if (__instance.direction == 1)
          {
            __instance.time = (int)((long)__instance.time * (long)coldSpend / (long)chargeSpend);
            __instance.direction = -1;
          }
          if (__instance.direction == -1)
          {
            __instance.time -= num1;
            if (__instance.time <= 0)
            {
              __instance.time = 0;
              __instance.direction = 0;
            }
          }
          __result = (double)power >= 0.100000001490116 ? 1U : 0U;
          return false;
        }
        if ((double)power < 0.100000001490116)
        {
          if (__instance.direction == 1)
          {
            __instance.time = (int)((long)__instance.time * (long)coldSpend / (long)chargeSpend);
            __instance.direction = -1;
          }
          __result = 0;
          return false;
        }
        bool flag;
        uint num2 = (flag = __instance.bulletCount > 0) ? 3U : 2U;
        if (__instance.direction == 1)
        {
          if (!flag)
          {
            __instance.time = (int)((long)__instance.time * (long)coldSpend / (long)chargeSpend);
            __instance.direction = -1;
          }
        }
        else if (__instance.direction == 0 && flag)
          __instance.direction = 1;
        if (__instance.direction == 1)
        {
          __instance.time += num1;
          if (__instance.time >= chargeSpend)
          {
            AstroPose[] astroPoses = sphere.starData.galaxy.astroPoses;
            __instance.fired = true;
            DysonNode autoDysonNode = sphere.GetAutoDysonNode(__instance.autoIndex + __instance.id);
            DysonRocket rocket = new DysonRocket()
            {
              planetId = __instance.planetId,
              uPos = astroPoses[__instance.planetId].uPos + Maths.QRotateLF(astroPoses[__instance.planetId].uRot, (VectorLF3)(__instance.localPos + __instance.localPos.normalized * 6.1f)),
              uRot = astroPoses[__instance.planetId].uRot * __instance.localRot * Quaternion.Euler(-90f, 0.0f, 0.0f)
            };
            rocket.uVel = rocket.uRot * Vector3.forward;
            rocket.uSpeed = 0.0f;
            rocket.launch = __instance.localPos.normalized;
            sphere.AddDysonRocket(rocket, autoDysonNode);
            ++__instance.autoIndex;
            --__instance.bulletCount;
            lock (consumeRegister)
              ++consumeRegister[__instance.bulletId];
            __instance.time = coldSpend;
            __instance.direction = -1;
          }
        }
        else if (__instance.direction == -1)
        {
          __instance.time -= num1;
          if (__instance.time <= 0)
          {
            __instance.time = 0;
            __instance.direction = flag ? 1 : 0;
          }
        }
        else
        {
          __instance.time = 0;
        }
        __result = num2;
        return false;
      }
    }

  }
}