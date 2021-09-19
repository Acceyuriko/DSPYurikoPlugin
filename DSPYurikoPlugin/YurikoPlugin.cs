using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace DSPYurikoPlugin
{
    [BepInPlugin("cc.acceyuriko.dsp", "YurikoPlugin", "1.0")]
    public class YurikoPlugin : BaseUnityPlugin
    {
        public void Start()
        {
            Harmony.CreateAndPatchAll(typeof(YurikoPlugin));
        }

        // 垃圾桶
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StorageComponent), "AddItem", new Type[] { typeof(int), typeof(int), typeof(bool) })]
        public static bool DoTrash(
            ref int __result,
            StorageComponent __instance,
            int itemId,
            int count,
            bool useBan = false
         )
        {
            if (!useBan || __instance.size != __instance.bans || itemId == 0 || (uint)count <= 0U)
            {
                return true;
            }
            // GameMain.mainPlayer.SetSandCount(GameMain.mainPlayer.sandCount + 100);
            __result = 1;
            return false;
        }

        // 传送带加速
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CargoTraffic), "GameTick")]
        public static bool CargoTrafficGameTickPatch(CargoTraffic __instance)
        {
            int num = 2;
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Belt);
            for (int i = 0; i < num; ++i)
            {
                for (int j = 1; j < __instance.pathCursor; ++j)
                {
                    if (__instance.pathPool[j] != null && __instance.pathPool[j].id == j)
                    {
                        __instance.pathPool[j].Update();
                    }
                }
            }
            PerformanceMonitor.EndSample(ECpuWorkEntry.Belt);
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Splitter);
            for (int i = 0; i < num; ++i)
            {
                for (int j = 1; j < __instance.splitterCursor; ++j)
                {
                    if (__instance.splitterPool[j].id == j)
                    {
                        __instance.splitterPool[j].CheckPriorityPreset();
                        __instance.UpdateSplitter(
                            j,
                            __instance.splitterPool[j].input0,
                            __instance.splitterPool[j].input1,
                            __instance.splitterPool[j].input2,
                            __instance.splitterPool[j].output0,
                            __instance.splitterPool[j].output1,
                            __instance.splitterPool[j].output2,
                            __instance.splitterPool[j].outFilter
                        );
                    }
                }
            }
            PerformanceMonitor.EndSample(ECpuWorkEntry.Splitter);
            return false;
        }

        // 传送带加速
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CargoPath), "Update")]
        public static bool CargoPathUpdatePatch(CargoPath __instance, int ___bufferLength, ref int ___updateLen, int ___chunkCount)
        {
            if (__instance.outputPath != null)
            {
                byte[] numArray = __instance.id > __instance.outputPath.id ? __instance.buffer : __instance.outputPath.buffer;
                lock (__instance.id < __instance.outputPath.id ? __instance.buffer : __instance.outputPath.buffer)
                {
                    lock (numArray)
                    {
                        int index = ___bufferLength - 5 - 1;
                        if (__instance.buffer[index] == 250)
                        {
                            int cargoId = __instance.buffer[index + 1] - 1 +
                                (__instance.buffer[index + 2] - 1) * 100 +
                                (__instance.buffer[index + 3] - 1) * 10000 +
                                (__instance.buffer[index + 4] - 1) * 1000000;
                            if (__instance.closed)
                            {
                                if (__instance.outputPath.TryInsertCargoNoSqueeze(__instance.outputIndex, cargoId))
                                {
                                    Array.Clear(__instance.buffer, index - 4, 10);
                                    ___updateLen = ___bufferLength;
                                }
                            }
                            else if (__instance.outputPath.TryInsertCargo(__instance.outputIndex, cargoId))
                            {
                                Array.Clear(__instance.buffer, index - 4, 10);
                                ___updateLen = ___bufferLength;
                            }
                        }
                    }
                }
            }
            else if (___bufferLength <= 10)
            {
                return false;
            }
            lock (__instance.buffer)
            {
                for (int index = ___updateLen - 1; index >= 0 && __instance.buffer[index] != 0; --index)
                {
                    --___updateLen;
                }
                if (___updateLen == 0)
                {
                    return false;
                }
                int num1 = ___updateLen;
                for (int index1 = ___chunkCount - 1; index1 >= 0; --index1)
                {
                    int index2 = __instance.chunks[index1 * 3];
                    int num2 = __instance.chunks[index1 * 3 + 2];
                    if (index2 < num1)
                    {
                        if (__instance.buffer[index2] != 0)
                        {
                            for (int index3 = index2 - 5; index3 < index2 + 4; ++index3)
                            {
                                if (index3 >= 0 && __instance.buffer[index3] == 250)
                                {
                                    index2 = index3 >= index2 ? index3 - 4 : index3 + 5 + 1;
                                    break;
                                }
                            }
                        }
                        num2 *= 2; // 传送带倍速
                        if (num2 > 10)
                        {
                            for (int index4 = 10; index4 <= num2; ++index4)
                            {
                                if (index2 + index4 + 10 >= num1)
                                {
                                    num2 = index4;
                                    break;
                                }
                                if (__instance.buffer[index2 + index4] != 0)
                                {
                                    num2 = index4;
                                    break;
                                }
                            }
                            if (num2 < 10)
                            {
                                num2 = 10;
                            }
                        }
                        int num3 = 0;
                    label_54:
                        while (num3 < num2)
                        {
                            int num4 = num1 - index2;
                            if (num4 >= 10)
                            {
                                int length = 0;
                                for (int index5 = 0; index5 < num2 - num3; ++index5)
                                {
                                    int index6 = num1 - 1 - index5;
                                    if (__instance.buffer[index6] == 0)
                                    {
                                        ++length;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                if (length > 0)
                                {
                                    Array.Copy(__instance.buffer, index2, __instance.buffer, index2 + length, num4 - length);
                                    Array.Clear(__instance.buffer, index2, length);
                                    num3 += length;
                                }
                                int index7 = num1 - 1;
                                while (true)
                                {
                                    if (index7 >= 0 && __instance.buffer[index7] != 0)
                                    {
                                        --num1;
                                        --index7;
                                    }
                                    else
                                    {
                                        goto label_54;
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        int num5 = index2 + (num3 == 0 ? 1 : num3);
                        if (num1 > num5)
                        {
                            num1 = num5;
                        }
                    }
                }
            }
            return false;
        }


        // 工作台加速
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
                timeSpend /= 2;
            }
            else if (__instance.recipeType == ERecipeType.Assemble)
            {
                timeSpend /= 2;
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
