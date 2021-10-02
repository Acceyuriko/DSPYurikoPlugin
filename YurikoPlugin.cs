using System;
using System.Collections.Generic;
using System.Reflection;
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
            if (__instance.buffer[index] == (byte)250)
            {
              int cargoId = (int)__instance.buffer[index + 1] - 1 + ((int)__instance.buffer[index + 2] - 1) * 100 + ((int)__instance.buffer[index + 3] - 1) * 10000 + ((int)__instance.buffer[index + 4] - 1) * 1000000;
              if (__instance.closed)
              {
                if (__instance.outputPath.TryInsertCargoNoSqueeze(__instance.outputIndex, cargoId))
                {
                  Array.Clear((Array)__instance.buffer, index - 4, 10);
                  ___updateLen = ___bufferLength;
                }
              }
              else if (__instance.outputPath.TryInsertCargo(__instance.outputIndex, cargoId))
              {
                Array.Clear((Array)__instance.buffer, index - 4, 10);
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
        for (int index = ___updateLen - 1; index >= 0 && __instance.buffer[index] != (byte)0; --index)
          --___updateLen;
        if (___updateLen == 0)
        {
          return false;
        }
        int num1 = ___updateLen;
        for (int index1 = ___chunkCount - 1; index1 >= 0; --index1)
        {
          int index2 = __instance.chunks[index1 * 3];
          int speed = __instance.chunks[index1 * 3 + 2] * 2; // 倍速
          if (index2 < num1)
          {
            if (__instance.buffer[index2] != (byte)0)
            {
              for (int index3 = index2 - 5; index3 < index2 + 4; ++index3)
              {
                if (index3 >= 0 && __instance.buffer[index3] == (byte)250)
                {
                  index2 = index3 >= index2 ? index3 - 4 : index3 + 5 + 1;
                  break;
                }
              }
            }
            int num2 = 0;
          label_41:
            while (num2 < speed)
            {
              int num3 = num1 - index2;
              if (num3 >= 10)
              {
                int length = 0;
                for (int index4 = 0; index4 < speed - num2 && __instance.buffer[num1 - 1 - index4] == (byte)0; ++index4)
                  ++length;
                if (length > 0)
                {
                  Array.Copy((Array)__instance.buffer, index2, (Array)__instance.buffer, index2 + length, num3 - length);
                  Array.Clear((Array)__instance.buffer, index2, length);
                  num2 += length;
                }
                int index5 = num1 - 1;
                while (true)
                {
                  if (index5 >= 0 && __instance.buffer[index5] != (byte)0)
                  {
                    --num1;
                    --index5;
                  }
                  else
                    goto label_41;
                }
              }
              else
                break;
            }
            int num4 = index2 + (num2 == 0 ? 1 : num2);
            if (num1 > num4)
              num1 = num4;
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
        timeSpend /= 10;
      }
      else if (__instance.recipeType == ERecipeType.Assemble)
      {
        timeSpend /= 10;
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
          __instance.fractionateSuccess = (double)__instance.seed / 2147483646.0 / 10 < (double)__instance.produceProb;
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

    // 实验室加速
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

    // 物流站小飞机
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StationComponent), "InternalTickLocal")]
    public static bool StationInternalTickLocalPatch(ref float dt)
    {
      // dt *= 2;
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
      int ratio = 10;
      shipSailSpeed *= ratio;
      shipWarpSpeed *= ratio;
      return true;
    }

    // 物流站耗电
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StationComponent), "CalcTripEnergyCost", new Type[] { typeof(double), typeof(float), typeof(bool) })]
    public static bool StationCalcTripEnergyCost(
      ref StationComponent __instance,
      ref long __result,
      double trip,
      float maxSpeed,
      bool canWarp
    )
    {
      double num1 = trip * 0.03 + 100.0;
      if (num1 > (double)maxSpeed)
        num1 = (double)maxSpeed;
      if (num1 > 3000.0)
        num1 = 3000.0;
      double num2 = num1 * 200000.0;
      __result = (long)(6000000.0 + trip * 30.0 + num2) / 10;
      return false;
    }

    // 轨道采集器
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StationComponent), "UpdateCollection")]
    public static bool StationUpdateCollectionPatch(ref float collectSpeedRate)
    {
      collectSpeedRate *= 10;
      return true;
    }

    // 射线接收器
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DysonNode), "OrderConstructCp")]
    public static bool DysonNodeOrderConstructCpPatch(
      ref DysonNode __instance,
      long gameTick,
      DysonSwarm swarm
    )
    {
      for (var i = 0; i < 10; i++)
      {
        if (__instance.cpReqOrder <= 0 || !swarm.AbsorbSail(__instance, gameTick))
          return false;
        ++__instance.cpOrdered;
      }
      return false;
    }

    // 星球矿机
    [HarmonyPostfix]
    [HarmonyPatch(typeof(FactorySystem), "CheckBeforeGameTick")]
    public static void FactorySystemGameTickPostPatch(
        ref FactorySystem __instance)
    {
      uint ticks = 60;
      if (!frames.ContainsKey(__instance.planet.id))
      {
        frames.Add(__instance.planet.id, 0);
      }
      if (frames[__instance.planet.id]++ % ticks != 0)
      {
        return;
      }
      // var logger = BepInEx.Logging.Logger.CreateLogSource("YurikoPlanetMiner");
      int ratio = 1;
      var veinPool = __instance.factory.veinPool;
      HashSet<int> veinSet = new HashSet<int>();
      for (int i = 0; i < veinPool.Length; ++i)
      {
        veinSet.Add(veinPool[i].productId);
      }

      int[] productRegister = (int[])null;
      if (GameMain.statistics.production.factoryStatPool[__instance.factory.index] != null)
      {
        productRegister = GameMain.statistics.production.factoryStatPool[__instance.factory.index].productRegister;
      }
      Dictionary<int, List<int[]>> localResourceSupply = new Dictionary<int, List<int[]>>();
      for (int stationIndex = 1; stationIndex < __instance.planet.factory.transport.stationCursor; ++stationIndex)
      {
        ref var station = ref __instance.planet.factory.transport.stationPool[stationIndex];
        if (station != null && station.storage != null)
        {
          // 填满翘曲
          if (station.warperCount < station.warperMaxCount)
          {
            station.warperCount = station.warperMaxCount;
          }
          for (int storageIndex = 0; storageIndex < station.storage.Length; ++storageIndex)
          {
            ref var stationStore = ref station.storage[storageIndex];

            if (veinSet.Contains(stationStore.itemId) || stationStore.itemId == __instance.planet.waterItemId)
            {
              if (stationStore.localLogic == ELogisticStorage.Demand && stationStore.count < stationStore.max)
              {
                float amount = 0f;
                amount += ticks;
                amount *= ratio * GameMain.history.miningSpeedScale;
                stationStore.count += (int)amount;
                if (productRegister != null)
                {
                  productRegister[stationStore.itemId] += (int)amount;
                }
              }
            }
            else
            {
              if (stationStore.itemId != 0 && stationStore.localLogic == ELogisticStorage.Supply)
              {
                if (!localResourceSupply.ContainsKey(stationStore.itemId))
                {
                  localResourceSupply.Add(stationStore.itemId, new List<int[]>());
                }
                localResourceSupply[stationStore.itemId].Add(new int[] { stationIndex, storageIndex });
              }
            }
          }
        }
      }
      for (int stationIndex = 1; stationIndex < __instance.planet.factory.transport.stationCursor; ++stationIndex)
      {
        ref var station = ref __instance.planet.factory.transport.stationPool[stationIndex];
        if (station != null && station.storage != null)
        {
          for (int storageIndex = 0; storageIndex < station.storage.Length; ++storageIndex)
          {
            ref var stationStore = ref station.storage[storageIndex];
            if (
              stationStore.itemId != 0 &&
              stationStore.localLogic == ELogisticStorage.Demand &&
              !veinSet.Contains(stationStore.itemId) &&
              stationStore.itemId != __instance.planet.waterItemId &&
              localResourceSupply.ContainsKey(stationStore.itemId)
            )
            {
              float amount = 0f;
              amount += ticks;
              amount *= ratio * GameMain.history.miningSpeedScale;
              amount = Math.Max(Math.Min(amount, (float)(stationStore.max - stationStore.count)), 0f);
              foreach (var indexs in localResourceSupply[stationStore.itemId])
              {
                ref var storeToSupply = ref __instance.planet.factory.transport.stationPool[indexs[0]].storage[indexs[1]];
                int eachAmount = Math.Min((int)amount, storeToSupply.count);
                if (eachAmount > 0)
                {
                  stationStore.count += eachAmount;
                  storeToSupply.count -= eachAmount;
                  amount -= (float)eachAmount;
                  if (amount <= 0)
                  {
                    break;
                  }
                }
              }
            }
          }
        }
      }
    }

    // 异常检测
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameAbnormalityCheck), "isGameNormal")]
    public static bool GameAbnormalityCheckPatch(ref bool __result)
    {
      __result = true;
      return false;
    }

    // 游戏数据
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameMain), "Begin")]
    public static void GameMainBeginPatch()
    {
      if (DSPGame.IsMenuDemo)
      {
        return;
      }
      // var logger = BepInEx.Logging.Logger.CreateLogSource("YurikoGameMainBegin");
      foreach (var tech in LDB._techs.dataArray)
      {
        var ts = GameMain.history.TechState(tech.ID);
        if (!ts.unlocked)
        {
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
            ts.hashNeeded = tech.GetHashNeeded(ts.curLevel);
          }
          GameMain.history.techStates[tech.ID] = ts;
          for (int index = 0; index < tech.UnlockRecipes.Length; ++index)
          {
            GameMain.history.UnlockRecipe(tech.UnlockRecipes[index]);
          }
          for (int index = 0; index < tech.UnlockFunctions.Length; ++index)
          {
            GameMain.history.UnlockTechFunction(tech.UnlockFunctions[index], tech.UnlockValues[index], ts.curLevel);
          }
          for (int index = 0; index < tech.AddItems.Length; ++index)
          {
            GameMain.history.GainTechAwards(tech.AddItems[index], tech.AddItemCounts[index]);
          }
          if (tech.ID > 1)
          {
            GameMain.history.RegFeatureKey(1000100);
          }
        }
      }
    }

    // 创建行星
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlanetGen), "CreatePlanet")]
    public static bool PlanetGenCreatePlanetPatch(
      ref PlanetData __result,
      GalaxyData galaxy,
      StarData star,
      int[] themeIds,
      int index,
      int orbitAround,
      int orbitIndex,
      int number,
      bool gasGiant,
      int info_seed,
      int gen_seed
    )
    {
      PlanetData planet = new PlanetData();
      DotNet35Random dotNet35Random = new DotNet35Random(info_seed);
      planet.index = index;
      planet.galaxy = star.galaxy;
      planet.star = star;
      planet.seed = gen_seed;
      planet.infoSeed = info_seed;
      planet.orbitAround = orbitAround;
      planet.orbitIndex = orbitIndex;
      planet.number = number;
      planet.id = star.id * 100 + index + 1;
      StarData[] stars = galaxy.stars;
      int num1 = 0;
      for (int index1 = 0; index1 < star.index; ++index1)
        num1 += stars[index1].planetCount;
      int num2 = num1 + index;
      if (orbitAround > 0)
      {
        for (int index2 = 0; index2 < star.planetCount; ++index2)
        {
          if (orbitAround == star.planets[index2].number && star.planets[index2].orbitAround == 0)
          {
            planet.orbitAroundPlanet = star.planets[index2];
            if (orbitIndex > 1)
            {
              planet.orbitAroundPlanet.singularity |= EPlanetSingularity.MultipleSatellites;
              break;
            }
            break;
          }
        }
        Assert.NotNull((object)planet.orbitAroundPlanet);
      }
      string str = star.planetCount > 20 ? (index + 1).ToString() : NameGen.roman[index + 1];
      planet.name = star.name + " " + str + "号星".Translate();
      double num3 = dotNet35Random.NextDouble();
      double num4 = dotNet35Random.NextDouble();
      double num5 = dotNet35Random.NextDouble();
      double num6 = dotNet35Random.NextDouble();
      double num7 = dotNet35Random.NextDouble();
      double num8 = dotNet35Random.NextDouble();
      double num9 = dotNet35Random.NextDouble();
      double num10 = dotNet35Random.NextDouble();
      double num11 = dotNet35Random.NextDouble();
      double num12 = dotNet35Random.NextDouble();
      double num13 = dotNet35Random.NextDouble();
      double num14 = dotNet35Random.NextDouble();
      double rand1 = dotNet35Random.NextDouble();
      double num15 = dotNet35Random.NextDouble();
      double rand2 = dotNet35Random.NextDouble();
      double rand3 = dotNet35Random.NextDouble();
      double rand4 = dotNet35Random.NextDouble();
      int theme_seed = dotNet35Random.Next();
      float a = Mathf.Pow(1.2f, (float)(num3 * (num4 - 0.5) * 0.5));
      float f1;
      if (orbitAround == 0)
      {
        float b = StarGen.orbitRadius[orbitIndex] * star.orbitScaler;
        float num16 = (float)(((double)a - 1.0) / (double)Mathf.Max(1f, b) + 1.0);
        f1 = b * num16;
      }
      else
        f1 = (float)(((1600.0 * (double)orbitIndex + 200.0) * (double)Mathf.Pow(star.orbitScaler, 0.3f) * (double)Mathf.Lerp(a, 1f, 0.5f) + (double)planet.orbitAroundPlanet.realRadius) / 40000.0);
      planet.orbitRadius = f1;
      planet.orbitInclination = (float)(num5 * 16.0 - 8.0);
      if (orbitAround > 0)
        planet.orbitInclination *= 2.2f;
      planet.orbitLongitude = (float)(num6 * 360.0);
      if (star.type >= EStarType.NeutronStar)
      {
        if ((double)planet.orbitInclination > 0.0)
          planet.orbitInclination += 3f;
        else
          planet.orbitInclination -= 3f;
      }
      planet.orbitalPeriod = planet.orbitAroundPlanet != null ? Math.Sqrt(39.4784176043574 * (double)f1 * (double)f1 * (double)f1 / 1.08308421068537E-08) : Math.Sqrt(39.4784176043574 * (double)f1 * (double)f1 * (double)f1 / (1.35385519905204E-06 * (double)star.mass));
      planet.orbitPhase = (float)(num7 * 360.0);
      if (num15 < 0.0399999991059303)
      {
        planet.obliquity = (float)(num8 * (num9 - 0.5) * 39.9);
        if ((double)planet.obliquity < 0.0)
          planet.obliquity -= 70f;
        else
          planet.obliquity += 70f;
        planet.singularity |= EPlanetSingularity.LaySide;
      }
      else if (num15 < 0.100000001490116)
      {
        planet.obliquity = (float)(num8 * (num9 - 0.5) * 80.0);
        if ((double)planet.obliquity < 0.0)
          planet.obliquity -= 30f;
        else
          planet.obliquity += 30f;
      }
      else
        planet.obliquity = (float)(num8 * (num9 - 0.5) * 60.0);
      planet.rotationPeriod = (num10 * num11 * 1000.0 + 400.0) * (orbitAround == 0 ? (double)Mathf.Pow(f1, 0.25f) : 1.0) * (gasGiant ? 0.200000002980232 : 1.0);
      if (!gasGiant)
      {
        if (star.type == EStarType.WhiteDwarf)
          planet.rotationPeriod *= 0.5;
        else if (star.type == EStarType.NeutronStar)
          planet.rotationPeriod *= 0.200000002980232;
        else if (star.type == EStarType.BlackHole)
          planet.rotationPeriod *= 0.150000005960464;
      }
      planet.rotationPhase = (float)(num12 * 360.0);
      planet.sunDistance = orbitAround == 0 ? planet.orbitRadius : planet.orbitAroundPlanet.orbitRadius;
      planet.scale = 1f;
      double num17 = orbitAround == 0 ? planet.orbitalPeriod : planet.orbitAroundPlanet.orbitalPeriod;
      planet.rotationPeriod = 1.0 / (1.0 / num17 + 1.0 / planet.rotationPeriod);
      if (orbitAround == 0 && orbitIndex <= 4 && !gasGiant)
      {
        if (num15 > 0.959999978542328)
        {
          planet.obliquity *= 0.01f;
          planet.rotationPeriod = planet.orbitalPeriod;
          planet.singularity |= EPlanetSingularity.TidalLocked;
        }
        else if (num15 > 0.930000007152557)
        {
          planet.obliquity *= 0.1f;
          planet.rotationPeriod = planet.orbitalPeriod * 0.5;
          planet.singularity |= EPlanetSingularity.TidalLocked2;
        }
        else if (num15 > 0.899999976158142)
        {
          planet.obliquity *= 0.2f;
          planet.rotationPeriod = planet.orbitalPeriod * 0.25;
          planet.singularity |= EPlanetSingularity.TidalLocked4;
        }
      }
      if (num15 > 0.85 && num15 <= 0.9)
      {
        planet.rotationPeriod = -planet.rotationPeriod;
        planet.singularity |= EPlanetSingularity.ClockwiseRotate;
      }
      planet.runtimeOrbitRotation = Quaternion.AngleAxis(planet.orbitLongitude, Vector3.up) * Quaternion.AngleAxis(planet.orbitInclination, Vector3.forward);
      if (planet.orbitAroundPlanet != null)
        planet.runtimeOrbitRotation = planet.orbitAroundPlanet.runtimeOrbitRotation * planet.runtimeOrbitRotation;
      planet.runtimeSystemRotation = planet.runtimeOrbitRotation * Quaternion.AngleAxis(planet.obliquity, Vector3.forward);
      float habitableRadius = star.habitableRadius;
      if (gasGiant)
      {
        planet.type = EPlanetType.Gas;
        planet.radius = 80f;
        planet.scale = 10f;
        planet.habitableBias = 100f;
      }
      else
      {
        float num18 = Mathf.Ceil((float)star.galaxy.starCount * 0.29f);
        if ((double)num18 < 11.0)
          num18 = 11f;
        double num19 = (double)num18 - (double)star.galaxy.habitableCount;
        float num20 = (float)(star.galaxy.starCount - star.index);
        float sunDistance = planet.sunDistance;
        float num21 = 1000f;
        float f2 = 1000f;
        if ((double)habitableRadius > 0.0 && (double)sunDistance > 0.0)
        {
          f2 = sunDistance / habitableRadius;
          num21 = Mathf.Abs(Mathf.Log(f2));
        }
        float num22 = Mathf.Clamp(Mathf.Sqrt(habitableRadius), 1f, 2f) - 0.04f;
        double num23 = (double)num20;
        float num24 = Mathf.Clamp(Mathf.Lerp((float)(num19 / num23), 0.35f, 0.5f), 0.08f, 0.8f);
        planet.habitableBias = num21 * num22;
        planet.temperatureBias = (float)(1.20000004768372 / ((double)f2 + 0.200000002980232) - 1.0);
        float num25 = Mathf.Pow(Mathf.Clamp01(planet.habitableBias / num24), num24 * 10f);
        if (num13 > (double)num25 && star.index > 0 || planet.orbitAround > 0 && planet.orbitIndex == 1 && star.index == 0)
        {
          planet.type = EPlanetType.Ocean;
          ++star.galaxy.habitableCount;
        }
        else if ((double)f2 < 0.833333015441895)
        {
          float num26 = Mathf.Max(0.15f, (float)((double)f2 * 2.5 - 0.850000023841858));
          planet.type = num14 >= (double)num26 ? EPlanetType.Vocano : EPlanetType.Desert;
        }
        else if ((double)f2 < 1.20000004768372)
        {
          planet.type = EPlanetType.Desert;
        }
        else
        {
          float num27 = (float)(0.899999976158142 / (double)f2 - 0.100000001490116);
          planet.type = num14 >= (double)num27 ? EPlanetType.Ice : EPlanetType.Desert;
        }
        planet.radius = 200f;
      }
      if (planet.type != EPlanetType.Gas && planet.type != EPlanetType.None)
      {
        planet.precision = 200;
        planet.segment = 5;
      }
      else
      {
        planet.precision = 64;
        planet.segment = 2;
      }
      planet.luminosity = Mathf.Pow(planet.star.lightBalanceRadius / (planet.sunDistance + 0.01f), 0.6f);
      if ((double)planet.luminosity > 1.0)
      {
        planet.luminosity = Mathf.Log(planet.luminosity) + 1f;
        planet.luminosity = Mathf.Log(planet.luminosity) + 1f;
        planet.luminosity = Mathf.Log(planet.luminosity) + 1f;
      }
      planet.luminosity = Mathf.Round(planet.luminosity * 100f) / 100f;
      PlanetGen.SetPlanetTheme(planet, themeIds, rand1, rand2, rand3, rand4, theme_seed);
      star.galaxy.astroPoses[planet.id].uRadius = planet.realRadius;
      planet.radius *= 2;
      __result = planet;
      return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StarGen), "CreateStar")]
    public static bool StarGenCreateStarPatch(
      ref StarData __result,
      GalaxyData galaxy,
      VectorLF3 pos,
      int id,
      int seed,
      EStarType needtype,
      ESpectrType needSpectr = ESpectrType.X
    )
    {
      StarData starData = new StarData()
      {
        galaxy = galaxy,
        index = id - 1
      };
      starData.level = galaxy.starCount <= 1 ? 0.0f : (float)starData.index / (float)(galaxy.starCount - 1);
      starData.id = id;
      starData.seed = seed;
      DotNet35Random dotNet35Random1 = new DotNet35Random(seed);
      int seed1 = dotNet35Random1.Next();
      int Seed = dotNet35Random1.Next();
      starData.position = pos;
      float num1 = (float)pos.magnitude / 32f;
      if ((double)num1 > 1.0)
        num1 = Mathf.Log(Mathf.Log(Mathf.Log(Mathf.Log(Mathf.Log(num1) + 1f) + 1f) + 1f) + 1f) + 1f;
      starData.resourceCoef = Mathf.Pow(7f, num1) * 0.6f;
      DotNet35Random dotNet35Random2 = new DotNet35Random(Seed);
      double r1 = dotNet35Random2.NextDouble();
      double r2 = dotNet35Random2.NextDouble();
      double num2 = dotNet35Random2.NextDouble();
      double rn = dotNet35Random2.NextDouble();
      double rt = dotNet35Random2.NextDouble();
      double num3 = (dotNet35Random2.NextDouble() - 0.5) * 0.2;
      double num4 = dotNet35Random2.NextDouble() * 0.2 + 0.9;
      double y = dotNet35Random2.NextDouble() * 0.4 - 0.2;
      double num5 = Math.Pow(2.0, y);
      float num6 = Mathf.Lerp(-0.98f, 0.88f, starData.level);
      float averageValue = (double)num6 >= 0.0 ? num6 + 0.65f : num6 - 0.65f;
      float standardDeviation = 0.33f;
      if (needtype == EStarType.GiantStar)
      {
        averageValue = y > -0.08 ? -1.5f : 1.6f;
        standardDeviation = 0.3f;
      }
      MethodInfo randNormal = typeof(StarGen).GetMethod("RandNormal", BindingFlags.NonPublic | BindingFlags.Static);
      float num7 = (float)randNormal.Invoke(null, new object[] { averageValue, standardDeviation, r1, r2 });
      switch (needSpectr)
      {
        case ESpectrType.M:
          num7 = -3f;
          break;
        case ESpectrType.O:
          num7 = 3f;
          break;
      }
      float p1 = (float)((double)Mathf.Clamp((double)num7 <= 0.0 ? num7 * 1f : num7 * 2f, -2.4f, 4.65f) + num3 + 1.0);
      switch (needtype)
      {
        case EStarType.WhiteDwarf:
          starData.mass = (float)(1.0 + r2 * 5.0);
          break;
        case EStarType.NeutronStar:
          starData.mass = (float)(7.0 + r1 * 11.0);
          break;
        case EStarType.BlackHole:
          starData.mass = (float)(18.0 + r1 * r2 * 30.0);
          break;
        default:
          starData.mass = Mathf.Pow(2f, p1);
          break;
      }
      double d = 5.0;
      if ((double)starData.mass < 2.0)
        d = 2.0 + 0.4 * (1.0 - (double)starData.mass);
      starData.lifetime = (float)(10000.0 * Math.Pow(0.1, Math.Log10((double)starData.mass * 0.5) / Math.Log10(d) + 1.0) * num4);
      switch (needtype)
      {
        case EStarType.GiantStar:
          starData.lifetime = (float)(10000.0 * Math.Pow(0.1, Math.Log10((double)starData.mass * 0.58) / Math.Log10(d) + 1.0) * num4);
          starData.age = (float)(num2 * 0.0399999991059303 + 0.959999978542328);
          break;
        case EStarType.WhiteDwarf:
        case EStarType.NeutronStar:
        case EStarType.BlackHole:
          starData.age = (float)(num2 * 0.400000005960464 + 1.0);
          if (needtype == EStarType.WhiteDwarf)
          {
            starData.lifetime += 10000f;
            break;
          }
          if (needtype == EStarType.NeutronStar)
          {
            starData.lifetime += 1000f;
            break;
          }
          break;
        default:
          starData.age = (double)starData.mass >= 0.5 ? ((double)starData.mass >= 0.8 ? (float)(num2 * 0.699999988079071 + 0.200000002980232) : (float)(num2 * 0.400000005960464 + 0.100000001490116)) : (float)(num2 * 0.119999997317791 + 0.0199999995529652);
          break;
      }
      float num8 = starData.lifetime * starData.age;
      if ((double)num8 > 5000.0)
        num8 = (float)(((double)Mathf.Log(num8 / 5000f) + 1.0) * 5000.0);
      if ((double)num8 > 8000.0)
        num8 = (float)(((double)Mathf.Log(Mathf.Log(Mathf.Log(num8 / 8000f) + 1f) + 1f) + 1.0) * 8000.0);
      starData.lifetime = num8 / starData.age;
      float f = (float)(1.0 - (double)Mathf.Pow(Mathf.Clamp01(starData.age), 20f) * 0.5) * starData.mass;
      starData.temperature = (float)(Math.Pow((double)f, 0.56 + 0.14 / (Math.Log10((double)f + 4.0) / Math.Log10(5.0))) * 4450.0 + 1300.0);
      double num9 = Math.Log10(((double)starData.temperature - 1300.0) / 4500.0) / Math.Log10(2.6) - 0.5;
      if (num9 < 0.0)
      {
        num9 *= 4.0;
      }
      if (num9 > 2.0)
        num9 = 2.0;
      else if (num9 < -4.0)
        num9 = -4.0;
      starData.spectr = (ESpectrType)Mathf.RoundToInt((float)num9 + 4f);
      starData.color = Mathf.Clamp01((float)((num9 + 3.5) * 0.200000002980232));
      starData.classFactor = (float)num9;
      starData.luminosity = Mathf.Pow(f, 0.7f);
      starData.radius = (float)(Math.Pow((double)starData.mass, 0.4) * num5);
      starData.acdiskRadius = 0.0f;
      float p2 = (float)num9 + 2f;
      starData.habitableRadius = Mathf.Pow(1.7f, p2) + 0.25f * Mathf.Min(1f, starData.orbitScaler);
      starData.lightBalanceRadius = Mathf.Pow(1.7f, p2);
      starData.orbitScaler = Mathf.Pow(1.35f, p2);
      if ((double)starData.orbitScaler < 1.0)
        starData.orbitScaler = Mathf.Lerp(starData.orbitScaler, 1f, 0.6f);
      StarGen.SetStarAge(starData, starData.age, rn, rt);
      starData.dysonRadius = starData.orbitScaler * 0.28f * 1.5f;
      if ((double)starData.dysonRadius * 40000.0 < (double)starData.physicsRadius * 1.5)
        starData.dysonRadius = (float)((double)starData.physicsRadius * 1.5 / 40000.0);
      starData.uPosition = starData.position * 2400000.0;
      starData.name = NameGen.RandomStarName(seed1, starData, galaxy);
      starData.overrideName = "";
      starData.radius *= 4;
      __result = starData;
      return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StarGen), "CreateStarPlanets")]
    public static bool StarGenCreateStarPlanetsPatch(
      GalaxyData galaxy,
      StarData star,
      GameDesc gameDesc,
      double[] ___pGas
    )
    {
      DotNet35Random dotNet35Random1 = new DotNet35Random(star.seed);
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random1.Next());
      double num1 = dotNet35Random2.NextDouble();
      double num2 = dotNet35Random2.NextDouble();
      double num3 = dotNet35Random2.NextDouble();
      double num4 = dotNet35Random2.NextDouble();
      double num5 = dotNet35Random2.NextDouble();
      double num6 = dotNet35Random2.NextDouble() * 0.2 + 0.9;
      double num7 = dotNet35Random2.NextDouble() * 0.2 + 0.9;
      {
        Array.Clear((Array)___pGas, 0, ___pGas.Length);
        star.planetCount = num1 >= 0.1 ? (num1 >= 0.2 ? (num1 >= 0.7 ? (num1 >= 0.95 ? 9 : 8) : 7) : 6) : 5;
        ___pGas[0] = 0.1;
        ___pGas[1] = 0.2;
        ___pGas[2] = 0.25;
        ___pGas[3] = 0.3;
        ___pGas[4] = 0.32;
        ___pGas[5] = 0.35;
        ___pGas[6] = 0.38;
        ___pGas[7] = 0.40;
        ___pGas[8] = 0.42;
        star.planets = new PlanetData[star.planetCount];
        int num8 = 0;
        int num9 = 0;
        int orbitAround = 0;
        int num10 = 1;
        for (int index = 0; index < star.planetCount; ++index)
        {
          int info_seed = dotNet35Random2.Next();
          int gen_seed = dotNet35Random2.Next();
          double num11 = dotNet35Random2.NextDouble();
          double num12 = dotNet35Random2.NextDouble();
          bool gasGiant = false;
          if (orbitAround == 0)
          {
            ++num8;
            if (index < star.planetCount - 1 && num11 < ___pGas[index])
            {
              gasGiant = true;
              if (num10 < 3)
                num10 = 3;
            }
            for (; star.index != 0 || num10 != 3; ++num10)
            {
              int num13 = star.planetCount - index;
              int num14 = 9 - num10;
              if (num14 > num13)
              {
                float a = (float)num13 / (float)num14;
                float num15 = num10 <= 3 ? Mathf.Lerp(a, 1f, 0.15f) + 0.01f : Mathf.Lerp(a, 1f, 0.45f) + 0.01f;
                if (dotNet35Random2.NextDouble() < (double)num15)
                  goto label_62;
              }
              else
                goto label_62;
            }
            gasGiant = true;
          }
          else
          {
            ++num9;
            gasGiant = false;
          }
        label_62:
          star.planets[index] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, index, orbitAround, orbitAround == 0 ? num10 : num9, orbitAround == 0 ? num8 : num9, gasGiant, info_seed, gen_seed);
          ++num10;
          if (gasGiant)
          {
            orbitAround = num8;
            num9 = 0;
          }
          if (num9 >= 1 && num12 < 0.8)
          {
            orbitAround = 0;
            num9 = 0;
          }
        }
      }
      int num16 = 0;
      int num17 = 0;
      int index1 = 0;
      for (int index2 = 0; index2 < star.planetCount; ++index2)
      {
        if (star.planets[index2].type == EPlanetType.Gas)
        {
          num16 = star.planets[index2].orbitIndex;
          break;
        }
      }
      for (int index3 = 0; index3 < star.planetCount; ++index3)
      {
        if (star.planets[index3].orbitAround == 0)
          num17 = star.planets[index3].orbitIndex;
      }
      if (num16 > 0)
      {
        int num18 = num16 - 1;
        bool flag = true;
        for (int index4 = 0; index4 < star.planetCount; ++index4)
        {
          if (star.planets[index4].orbitAround == 0 && star.planets[index4].orbitIndex == num16 - 1)
          {
            flag = false;
            break;
          }
        }
        if (flag && num4 < 0.2 + (double)num18 * 0.2)
          index1 = num18;
      }
      int index5 = num5 >= 0.2 ? (num5 >= 0.4 ? (num5 >= 0.8 ? 0 : num17 + 1) : num17 + 2) : num17 + 3;
      if (index5 != 0 && index5 < 5)
        index5 = 5;
      star.asterBelt1OrbitIndex = (float)index1;
      star.asterBelt2OrbitIndex = (float)index5;
      if (index1 > 0)
        star.asterBelt1Radius = StarGen.orbitRadius[index1] * (float)num6 * star.orbitScaler;
      if (index5 <= 0)
        return false;
      star.asterBelt2Radius = StarGen.orbitRadius[index5] * (float)num7 * star.orbitScaler;
      return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UniverseGen), "CreateGalaxy")]
    public static bool UniverseGenCreateGalaxyPatch(
      ref GalaxyData __result,
      ref GameDesc gameDesc,
      ref List<VectorLF3> ___tmp_poses
    )
    {
      int galaxyAlgo = gameDesc.galaxyAlgo;
      int galaxySeed = gameDesc.galaxySeed;
      int starCount = gameDesc.starCount;
      if (galaxyAlgo < 20200101 || galaxyAlgo > 20591231)
        throw new Exception("Wrong version of unigen algorithm!");
      DotNet35Random dotNet35Random = new DotNet35Random(galaxySeed);
      MethodInfo generateTempPoses = typeof(UniverseGen).GetMethod("GenerateTempPoses", BindingFlags.NonPublic | BindingFlags.Static);
      double density = 1;
      int tempPoses = (int)generateTempPoses.Invoke(
        null,
        new object[] {
          dotNet35Random.Next(),
          starCount,
          4,
          2.0 / density,
          2.3 / density,
          3.5 / density,
          0.18
        }
      );
      GalaxyData galaxy = new GalaxyData();
      galaxy.seed = galaxySeed;
      galaxy.starCount = tempPoses;
      galaxy.stars = new StarData[tempPoses];
      Assert.Positive(tempPoses);
      if (tempPoses <= 0)
      {
        __result = galaxy;
        return false;
      }
      float num1 = (float)dotNet35Random.NextDouble();
      float num2 = (float)dotNet35Random.NextDouble();
      float num3 = (float)dotNet35Random.NextDouble();
      float num4 = (float)dotNet35Random.NextDouble();
      int num5 = Mathf.CeilToInt((float)(0.00999999977648258 * (double)tempPoses + (double)num1 * 0.300000011920929));
      int num6 = Mathf.CeilToInt((float)(0.00999999977648258 * (double)tempPoses + (double)num2 * 0.300000011920929));
      int num7 = Mathf.CeilToInt((float)(0.0160000007599592 * (double)tempPoses + (double)num3 * 0.400000005960464));
      int num8 = Mathf.CeilToInt((float)(0.0130000002682209 * (double)tempPoses + (double)num4 * 1.39999997615814));
      int num9 = tempPoses - num5;
      int num10 = num9 - num6;
      int num11 = num10 - num7;
      int num12 = (num11 - 1) / num8 / 4;
      int num13 = 0;
      for (int index = 0; index < tempPoses; ++index)
      {
        ESpectrType needSpectr = ESpectrType.X;
        double p = dotNet35Random.NextDouble();
        if (p > 0.85)
        {
          needSpectr = ESpectrType.O;
        }

        EStarType needtype = EStarType.MainSeqStar;
        if (index % num12 == num13)
          needtype = EStarType.GiantStar;
        if (index >= num9)
          needtype = EStarType.BlackHole;
        else if (index >= num10)
          needtype = EStarType.NeutronStar;
        else if (index >= num11)
          needtype = EStarType.WhiteDwarf;

        galaxy.stars[index] = StarGen.CreateStar(galaxy, ___tmp_poses[index], index + 1, dotNet35Random.Next(), needtype, needSpectr);
      }
      AstroPose[] astroPoses = galaxy.astroPoses;
      StarData[] stars = galaxy.stars;
      for (int index = 0; index < galaxy.astroPoses.Length; ++index)
      {
        astroPoses[index].uRot.w = 1f;
        astroPoses[index].uRotNext.w = 1f;
      }
      for (int index = 0; index < tempPoses; ++index)
      {
        StarGen.CreateStarPlanets(galaxy, stars[index], gameDesc);
        astroPoses[stars[index].id * 100].uPos = astroPoses[stars[index].id * 100].uPosNext = stars[index].uPosition;
        astroPoses[stars[index].id * 100].uRot = astroPoses[stars[index].id * 100].uRotNext = Quaternion.identity;
        astroPoses[stars[index].id * 100].uRadius = stars[index].physicsRadius;
      }
      galaxy.UpdatePoses(0.0);
      galaxy.birthPlanetId = 0;
      if (tempPoses > 0)
      {
        StarData starData = stars[0];
        for (int index = 0; index < starData.planetCount; ++index)
        {
          PlanetData planet = starData.planets[index];
          ThemeProto themeProto = LDB.themes.Select(planet.theme);
          if (themeProto != null && themeProto.Distribute == EThemeDistribute.Birth)
          {
            galaxy.birthPlanetId = planet.id;
            galaxy.birthStarId = starData.id;
            break;
          }
        }
      }
      Assert.Positive(galaxy.birthPlanetId);
      for (int index1 = 0; index1 < tempPoses; ++index1)
      {
        StarData star = galaxy.stars[index1];
        for (int index2 = 0; index2 < star.planetCount; ++index2)
          PlanetModelingManager.Algorithm(star.planets[index2]).GenerateVeins(true);
      }
      UniverseGen.CreateGalaxyStarGraph(galaxy);
      __result = galaxy;
      return false;
    }

    private static Dictionary<int, ulong> frames = new Dictionary<int, ulong>();
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
