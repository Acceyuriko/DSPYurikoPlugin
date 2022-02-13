using System;
using System.Reflection;
using HarmonyLib;

namespace DSPYurikoPlugin
{
    public class LabComponentPatch
    {
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
                int length = __instance.matrixServed.Length;
                int index1 = length == 0 ? 0 : 10;
                var splitIncLevel = typeof(LabComponent).GetMethod("split_inc_level", BindingFlags.NonPublic | BindingFlags.Instance);
                for (int index2 = 0; index2 < length; ++index2)
                {
                    if (__instance.matrixPoints[index2] > 0)
                    {
                        int num13 = __instance.matrixServed[index2] / 3600;
                        var arguments = new object[] { __instance.matrixServed[index2], __instance.matrixIncServed[index2], __instance.matrixPoints[index2] * num11 };
                        int num14 = (int)splitIncLevel.Invoke(__instance, arguments);
                        index1 = index1 < num14 ? index1 : num14;
                        int num15 = __instance.matrixServed[index2] / 3600;
                        if (__instance.matrixServed[index2] <= 0 || __instance.matrixIncServed[index2] < 0)
                            __instance.matrixIncServed[index2] = 0;
                        consumeRegister[LabComponent.matrixIds[index2]] += num13 - num15;
                    }
                }
                if (index1 < 0)
                    index1 = 0;
                __instance.extraSpeed = (int)(10000.0 * Cargo.incTableMilli[index1] * 10.0 + 0.1);
                __instance.extraPowerRatio = Cargo.powerTable[index1];
                __instance.extraHashBytes += (int)((double)power * (double)__instance.extraSpeed * (double)speed + 0.5);
                long num16 = (long)(__instance.extraHashBytes / 100000);
                __instance.extraHashBytes -= (int)num16 * 100000;
                long num17 = num16 < 0L ? 0L : num16;
                int num18 = (int)num17;
                ts.hashUploaded += (num11 + num17) * YurikoConstants.LAB_RESEARCH_RATIO;
                hashRegister += (num11 + num17) * YurikoConstants.LAB_RESEARCH_RATIO;
                uMatrixPoint += (long)ts.uPointPerHash * num11 * YurikoConstants.LAB_RESEARCH_RATIO;
                techHashedThisFrame += (int)(num11 + num18) * YurikoConstants.LAB_RESEARCH_RATIO;
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
            else
            {
                __instance.extraSpeed = 0;
                __instance.extraPowerRatio = 0;
            }
            __result = 1;
            return false;
        }
    }
}