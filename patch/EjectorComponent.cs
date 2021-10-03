using System;
using HarmonyLib;
using UnityEngine;

namespace DSPYurikoPlugin
{
  public class EjectorComponentPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(
        typeof(EjectorComponent),
        "InternalUpdate",
        new Type[] { typeof(float), typeof(DysonSwarm), typeof(AstroPose[]), typeof(AnimData[]), typeof(int[]) }
    )]
    public static bool EjectorInternalUpdatePatch(
       ref uint __result,
       ref EjectorComponent __instance,
       float power,
       DysonSwarm swarm,
       AstroPose[] astroPoses,
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
      animPool[__instance.entityId].prepare_length = __instance.localDir.x;
      animPool[__instance.entityId].working_length = __instance.localDir.y;
      animPool[__instance.entityId].power = __instance.localDir.z;
      __instance.targetState = EjectorComponent.ETargetState.None;
      if (__instance.fired)
      {
        animPool[__instance.entityId].time += 0.01666667f * ratio;
        if ((double)animPool[__instance.entityId].time >= 11.0)
        {
          __instance.fired = false;
          animPool[__instance.entityId].time = 0.0f;
        }
      }
      else
      {
        animPool[__instance.entityId].time = __instance.direction <= 0
            ? (__instance.direction >= 0 ? 0.0f : (float)-(double)__instance.time / (float)coldSpend)
            : (float)__instance.time / (float)chargeSpend;
      }
      if (__instance.orbitId < 0 ||
          __instance.orbitId >= swarm.orbitCursor ||
          swarm.orbits[__instance.orbitId].id != __instance.orbitId ||
          !swarm.orbits[__instance.orbitId].enabled)
      {
        __instance.orbitId = 0;
      }
      int num1 = (int)((double)power * 10000.0 + 0.100000001490116);
      if (__instance.orbitId == 0)
      {
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
        if ((double)power < 0.100000001490116)
        {
          __result = 0;
          return false;
        }
        __instance.localDir.x *= 0.9f;
        __instance.localDir.y *= 0.9f;
        __instance.localDir.z = (float)((double)__instance.localDir.z * 0.899999976158142 + 0.100000001490116);
        __result = 1;
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
      __instance.targetState = EjectorComponent.ETargetState.OK;
      bool flag1 = true;
      int index1 = __instance.planetId / 100 * 100;
      float num2 = (float)((double)__instance.localAlt + (double)__instance.pivotY + ((double)__instance.muzzleY - (double)__instance.pivotY) / (double)Mathf.Max(0.1f, Mathf.Sqrt((float)(1.0 - (double)__instance.localDir.y * (double)__instance.localDir.y))));
      Vector3 vector3_1 = new Vector3(__instance.localPosN.x * num2, __instance.localPosN.y * num2, __instance.localPosN.z * num2);
      VectorLF3 vectorLf3_1 = astroPoses[__instance.planetId].uPos + Maths.QRotateLF(astroPoses[__instance.planetId].uRot, (VectorLF3)vector3_1);
      Quaternion q = astroPoses[__instance.planetId].uRot * __instance.localRot;
      VectorLF3 uPos = astroPoses[index1].uPos;
      VectorLF3 b = uPos - vectorLf3_1;
      VectorLF3 vectorLf3_2 = uPos + VectorLF3.Cross((VectorLF3)swarm.orbits[__instance.orbitId].up, b).normalized * (double)swarm.orbits[__instance.orbitId].radius;
      VectorLF3 vectorLf3_3 = vectorLf3_2 - vectorLf3_1;
      __instance.targetDist = vectorLf3_3.magnitude;
      vectorLf3_3.x /= __instance.targetDist;
      vectorLf3_3.y /= __instance.targetDist;
      vectorLf3_3.z /= __instance.targetDist;
      Vector3 v = (Vector3)vectorLf3_3;
      Vector3 vector3_2 = Maths.QInvRotate(q, v);
      __instance.localDir.x = (float)((double)__instance.localDir.x * 0.899999976158142 + (double)vector3_2.x * 0.100000001490116);
      __instance.localDir.y = (float)((double)__instance.localDir.y * 0.899999976158142 + (double)vector3_2.y * 0.100000001490116);
      __instance.localDir.z = (float)((double)__instance.localDir.z * 0.899999976158142 + (double)vector3_2.z * 0.100000001490116);
      if ((double)vector3_2.y < 0.08715574 || (double)vector3_2.y > 0.866025388240814)
      {
        __instance.targetState = EjectorComponent.ETargetState.AngleLimit;
        flag1 = false;
      }
      bool flag2 = __instance.bulletCount > 0;
      if (flag2 & flag1)
      {
        for (int index2 = index1 + 1; index2 <= __instance.planetId + 2; ++index2)
        {
          if (index2 != __instance.planetId)
          {
            double uRadius = (double)astroPoses[index2].uRadius;
            if (uRadius > 1.0)
            {
              VectorLF3 vectorLf3_4 = astroPoses[index2].uPos - vectorLf3_1;
              double num3 = vectorLf3_4.x * vectorLf3_4.x + vectorLf3_4.y * vectorLf3_4.y + vectorLf3_4.z * vectorLf3_4.z;
              double num4 = vectorLf3_4.x * vectorLf3_3.x + vectorLf3_4.y * vectorLf3_3.y + vectorLf3_4.z * vectorLf3_3.z;
              if (num4 > 0.0)
              {
                double num5 = num3 - num4 * num4;
                double num6 = uRadius + 120.0;
                double num7 = num6 * num6;
                if (num5 < num7)
                {
                  flag1 = false;
                  __instance.targetState = EjectorComponent.ETargetState.Blocked;
                  break;
                }
              }
            }
          }
        }
      }
      bool flag3 = flag1 & flag2;
      uint num8 = flag2 ? (flag1 ? 4U : 3U) : 2U;
      if (__instance.direction == 1)
      {
        if (!flag3)
        {
          __instance.time = (int)((long)__instance.time * (long)coldSpend / (long)chargeSpend);
          __instance.direction = -1;
        }
      }
      else if (__instance.direction == 0 && flag3)
        __instance.direction = 1;
      if (__instance.direction == 1)
      {
        __instance.time += num1;
        if (__instance.time >= chargeSpend)
        {
          __instance.fired = true;
          animPool[__instance.entityId].time = 10f;
          swarm.AddBullet(new SailBullet()
          {
            maxt = (float)(__instance.targetDist / 4000.0),
            lBegin = vector3_1,
            uEndVel = (Vector3)(VectorLF3.Cross(vectorLf3_2 - uPos, (VectorLF3)swarm.orbits[__instance.orbitId].up).normalized * Math.Sqrt((double)swarm.dysonSphere.gravity / (double)swarm.orbits[__instance.orbitId].radius)),
            uBegin = vectorLf3_1,
            uEnd = vectorLf3_2
          }, __instance.orbitId);
          --__instance.bulletCount;
          lock (consumeRegister)
          {
            ++consumeRegister[__instance.bulletId];
          }
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
          __instance.direction = flag3 ? 1 : 0;
        }
      }
      else
      {
        __instance.time = 0;
      }
      __result = num8;
      return false;
    }

  }
}