﻿using System;
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
                        num2 *= 10;// 传送带倍速，实际倍速不足，10倍大概能跑到 100/s
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

        // 分拣器加速
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

        // 分拣器加速
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

        // 分馏塔加速
        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(FractionateComponent),
            "InternalUpdate",
            new Type[] {
                typeof(PlanetFactory),
                typeof(float),
                typeof(SignData[]),
                typeof(int[]),
                typeof(int[])
            }
        )]
        public static bool FractionateInternalUpdatePatch(
            ref uint __result,
            ref FractionateComponent __instance,
            PlanetFactory factory,
            float power,
            SignData[] signPool,
            int[] productRegister,
            int[] consumeRegister
        )
        {
            if ((double)power < 0.100000001490116)
            {
                __result = 0;
                return false;
            }
            int threshold = 10000;
            if (__instance.fluidInputCount > 0 && __instance.productOutputCount < __instance.productOutputMax && __instance.fluidOutputCount < __instance.fluidOutputMax)
            {
                if (__instance.progress == 0)
                {
                    __instance.isRand = true;
                }
                if (__instance.isRand)
                {
                    __instance.seed = (uint)((ulong)(__instance.seed % 2147483646U + 1U) * 48271UL % (ulong)int.MaxValue) - 1U;
                    __instance.fractionateSuccess = (double)__instance.seed / 2147483646.0 / 2 < (double)__instance.produceProb;
                    __instance.isRand = false;
                }
                int num = (int)((double)power * (500.0 / 3.0) * (double)__instance.fluidInputCount + 0.5);
                __instance.progress += num;
                if (__instance.progress >= threshold)
                {
                    if (__instance.fractionateSuccess)
                    {
                        ++__instance.productOutputCount;
                        ++__instance.productOutputTotal;
                        lock (productRegister)
                        {
                            ++productRegister[__instance.productId];
                        }
                        lock (consumeRegister)
                        {
                            ++consumeRegister[__instance.fluidId];
                        }
                    }
                    else
                    {
                        ++__instance.fluidOutputCount;
                        ++__instance.fluidOutputTotal;
                    }
                    --__instance.fluidInputCount;
                    __instance.progress -= threshold;
                    __instance.isRand = true;
                }
            }
            CargoTraffic cargoTraffic = factory.cargoTraffic;
            if (__instance.belt1 > 0)
            {
                if (__instance.isOutput1)
                {
                    while (__instance.fluidOutputCount > 0 && cargoTraffic.TryInsertItemAtHead(__instance.belt1, __instance.fluidId))
                    {
                        --__instance.fluidOutputCount;
                    }
                }
                else if (!__instance.isOutput1 && __instance.fluidInputCount < __instance.fluidInputMax)
                {
                    if (__instance.fluidId > 0)
                    {
                        while (cargoTraffic.TryPickItemAtRear(__instance.belt1, __instance.fluidId, null) > 0)
                        {
                            ++__instance.fluidInputCount;
                        }
                    }
                    else
                    {
                        int needId;
                        while ((needId = cargoTraffic.TryPickItemAtRear(__instance.belt1, 0, RecipeProto.fractionateNeeds)) > 0)
                        {
                            ++__instance.fluidInputCount;
                            if (__instance.fluidId == 0)
                            {
                                __instance.SetRecipe(needId, signPool);
                            }
                        }
                    }
                }
            }
            if (__instance.belt2 > 0)
            {
                if (__instance.isOutput2)
                {
                    while (__instance.fluidOutputCount > 0 && cargoTraffic.TryInsertItemAtHead(__instance.belt2, __instance.fluidId))
                    {
                        --__instance.fluidOutputCount;
                    }
                }
                else if (!__instance.isOutput2 && __instance.fluidInputCount < __instance.fluidInputMax)
                {
                    if (__instance.fluidId > 0)
                    {
                        while (cargoTraffic.TryPickItemAtRear(__instance.belt2, __instance.fluidId, null) > 0)
                        {
                            ++__instance.fluidInputCount;
                        }
                    }
                    else
                    {
                        int needId;
                        while ((needId = cargoTraffic.TryPickItemAtRear(__instance.belt2, 0, RecipeProto.fractionateNeeds)) > 0)
                        {
                            ++__instance.fluidInputCount;
                            if (__instance.fluidId == 0)
                            {
                                __instance.SetRecipe(needId, signPool);
                            }
                        }
                    }
                }
            }
            while (__instance.belt0 > 0 && __instance.isOutput0 && __instance.productOutputCount > 0 && cargoTraffic.TryInsertItemAtHead(__instance.belt0, __instance.productId))
            {
                --__instance.productOutputCount;
            }
            if (__instance.fluidInputCount == 0 && __instance.fluidOutputCount == 0 && __instance.productOutputCount == 0)
            {
                __instance.fluidId = 0;
            }
            __instance.isWorking = __instance.fluidInputCount > 0 && __instance.productOutputCount < __instance.productOutputMax && __instance.fluidOutputCount < __instance.fluidOutputMax;
            __result = !__instance.isWorking ? 0U : 1U;
            return false;
        }

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
            int timeSpend = __instance.timeSpend / 2;
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

        // 轨道弹射器
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
            int ratio = 2;
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

        // 垂直发射井
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
            int ratio = 2;
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

        // 物流站小飞机
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StationComponent), "InternalTickLocal")]
        public static bool StationInternalTickLocalPatch(ref float dt)
        {
            dt *= 2;
            return true;
        }

        // 物流站大飞机
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StationComponent), "InternalTickRemote")]
        public static bool StationInternalTickRemotePatch(
            ref double dt,
            ref float shipSailSpeed,
            ref float shipWarpSpeed)
        {
            int ratio = 2;
            dt *= ratio;
            shipSailSpeed *= ratio;
            shipWarpSpeed *= ratio;
            return true;
        }

        // 轨道采集器
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StationComponent), "UpdateCollection")]
        public static bool StationUpdateCollectionPatch(ref float collectSpeedRate)
        {
            collectSpeedRate *= 10;
            return true;
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
