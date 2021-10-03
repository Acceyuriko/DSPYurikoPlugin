using System;
using System.Collections.Generic;
using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class FactorySystemPatch
  {
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

    private static Dictionary<int, ulong> frames = new Dictionary<int, ulong>();
  }
}