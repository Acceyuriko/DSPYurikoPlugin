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
                        byte stack;
                        byte inc;
                        int num = factory.PickFrom(__instance.pickTarget, __instance.pickOffset, __instance.filter, needs, out stack, out inc);
                        if (num > 0)
                        {
                            __instance.itemId = num;
                            __instance.itemCount += (short)stack;
                            __instance.itemInc += (short)inc;
                            ++__instance.stackCount;
                            __instance.time++;
                        }
                    }
                }
                else
                {
                    byte stack;
                    byte inc;
                    int num = factory.PickFrom(__instance.pickTarget, __instance.pickOffset, __instance.filter, null, out stack, out inc);
                    if (num > 0)
                    {
                        __instance.itemId = num;
                        __instance.itemCount += (short)stack;
                        __instance.itemInc += (short)inc;
                        ++__instance.stackCount;
                        __instance.time++;
                    }
                }
            }
            if (__instance.itemId > 0 && __instance.stackCount > 0)
            {
                int num1 = (int)__instance.itemCount / __instance.stackCount;
                int num2 = (int)((double)__instance.itemInc / (double)__instance.itemCount * (double)num1 + 0.5);
                byte remainInc = (byte)num2;
                int num3 = factory.InsertInto(__instance.insertTarget, __instance.insertOffset, __instance.itemId, (byte)num1, (byte)num2, out remainInc);
                if (num3 > 0)
                {
                    if (remainInc == (byte)0 && num3 == num1)
                    {
                        --__instance.stackCount;
                    }
                    __instance.itemCount -= (short)num3;
                    __instance.itemInc -= (short)(num2 - (int)remainInc);
                    if (__instance.stackCount == 0)
                    {
                        __instance.itemId = 0;
                        __instance.time++;
                        __instance.itemCount = 0;
                        __instance.itemInc = 0;
                    }
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