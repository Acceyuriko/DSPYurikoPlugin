using System;
using System.Collections.Generic;
using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class StationComponentPatch
  {
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
      collectSpeedRate *= YurikoConstants.STATION_COLLECT_SPEED_RATIO;
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StationComponent), "RematchRemotePairs")]
    public static bool RematchRemotePairs(
      ref StationComponent __instance,
      StationComponent[] gStationPool,
      int gStationCursor,
      int keyStationGId,
      int shipCarries
    )
    {
      __instance.ClearRemotePairs();
      ref var astroPoses = ref GameMain.data.galaxy.astroPoses;
      SortedList<double, SupplyDemandPair> sortedRemotePairs = new SortedList<double, SupplyDemandPair>();
      for (int index = 0; index < __instance.storage.Length; ++index)
      {
        var store = __instance.storage[index];
        for (int remoteStationId = 1; remoteStationId < gStationCursor; ++remoteStationId)
        {
          if (
            gStationPool[remoteStationId] == null ||
            gStationPool[remoteStationId].gid != remoteStationId ||
            gStationPool[remoteStationId].planetId == __instance.planetId
          )
          {
            continue;
          }
          for (int remoteStoreIndex = 0; remoteStoreIndex < gStationPool[remoteStationId].storage.Length; ++remoteStoreIndex)
          {
            var remoteStore = gStationPool[remoteStationId].storage[remoteStoreIndex];
            var trip = (astroPoses[__instance.planetId].uPos - astroPoses[gStationPool[remoteStationId].planetId].uPos).magnitude +
              astroPoses[__instance.planetId].uRadius + astroPoses[gStationPool[remoteStationId].planetId].uRadius;
            if (remoteStore.itemId == store.itemId)
            {
              if (remoteStore.remoteLogic == ELogisticStorage.Supply && store.remoteLogic == ELogisticStorage.Demand)
              {
                sortedRemotePairs.Add(trip, new SupplyDemandPair(remoteStationId, remoteStoreIndex, __instance.gid, index));
              }
              else if (remoteStore.remoteLogic == ELogisticStorage.Demand && store.remoteLogic == ELogisticStorage.Supply)
              {
                sortedRemotePairs.Add(trip, new SupplyDemandPair(__instance.gid, index, remoteStationId, remoteStoreIndex));
              }
            }
          }
        }
      }

      foreach(var pair in sortedRemotePairs.Values) {
        __instance.AddRemotePair(pair.supplyId, pair.supplyIndex, pair.demandId, pair.demandIndex);
      }

      if (keyStationGId <= 0)
        return false;
      for (int index1 = 0; index1 < __instance.workShipCount; ++index1)
      {
        StationComponent stationComponent = gStationPool[__instance.workShipDatas[index1].otherGId];
        StationStore[] stationStoreArray = stationComponent == null ? (StationStore[])null : stationComponent.storage;
        if (keyStationGId == __instance.gid)
        {
          if (__instance.workShipDatas[index1].itemCount == 0 && __instance.workShipDatas[index1].direction > 0 && __instance.workShipDatas[index1].otherGId > 0 && __instance.HasRemoteDemand(__instance.workShipDatas[index1].itemId) == -1)
          {
            if (__instance.workShipOrders[index1].itemId > 0)
            {
              if (__instance.storage[__instance.workShipOrders[index1].thisIndex].itemId == __instance.workShipOrders[index1].itemId)
                __instance.storage[__instance.workShipOrders[index1].thisIndex].remoteOrder -= __instance.workShipOrders[index1].thisOrdered;
              if (stationStoreArray[__instance.workShipOrders[index1].otherIndex].itemId == __instance.workShipOrders[index1].itemId)
                stationStoreArray[__instance.workShipOrders[index1].otherIndex].remoteOrder -= __instance.workShipOrders[index1].otherOrdered;
              __instance.workShipOrders[index1].ClearThis();
              __instance.workShipOrders[index1].ClearOther();
            }
            __instance.workShipDatas[index1].itemId = 0;
            for (int index2 = 0; index2 < __instance.storage.Length; ++index2)
            {
              if (__instance.storage[index2].remoteLogic == ELogisticStorage.Demand)
              {
                int index3 = stationComponent.HasRemoteSupply(__instance.storage[index2].itemId, 1);
                if (index3 >= 0)
                {
                  __instance.workShipDatas[index1].itemId = __instance.storage[index2].itemId;
                  __instance.workShipDatas[index1].direction = 1;
                  __instance.workShipOrders[index1].itemId = __instance.workShipDatas[index1].itemId;
                  __instance.workShipOrders[index1].otherStationGId = __instance.workShipDatas[index1].otherGId;
                  __instance.workShipOrders[index1].thisIndex = index2;
                  __instance.workShipOrders[index1].otherIndex = index3;
                  __instance.workShipOrders[index1].thisOrdered = shipCarries;
                  __instance.workShipOrders[index1].otherOrdered = -shipCarries;
                  __instance.storage[index2].remoteOrder += shipCarries;
                  stationStoreArray[index3].remoteOrder -= shipCarries;
                  break;
                }
              }
            }
            if (__instance.workShipDatas[index1].itemId == 0)
            {
              __instance.workShipDatas[index1].otherGId = 0;
              __instance.workShipDatas[index1].direction = -1;
            }
          }
          if (__instance.workShipDatas[index1].itemCount != 0 && __instance.workShipDatas[index1].direction < 0)
          {
            int itemId = __instance.workShipDatas[index1].itemId;
            if (__instance.HasRemoteDemand(itemId) == -1 && __instance.workShipOrders[index1].itemId > 0)
            {
              if (__instance.storage[__instance.workShipOrders[index1].thisIndex].itemId == __instance.workShipOrders[index1].itemId)
                __instance.storage[__instance.workShipOrders[index1].thisIndex].remoteOrder -= __instance.workShipOrders[index1].thisOrdered;
              if (stationStoreArray[__instance.workShipOrders[index1].otherIndex].itemId == __instance.workShipOrders[index1].itemId)
                stationStoreArray[__instance.workShipOrders[index1].otherIndex].remoteOrder -= __instance.workShipOrders[index1].otherOrdered;
              __instance.workShipOrders[index1].ClearThis();
              __instance.workShipOrders[index1].ClearOther();
              __instance.workShipOrders[index1].itemId = itemId;
            }
          }
        }
        if (keyStationGId == __instance.workShipDatas[index1].otherGId)
        {
          if ((gStationPool[__instance.workShipDatas[index1].otherGId] == null || gStationPool[__instance.workShipDatas[index1].otherGId].gid == 0) && __instance.workShipDatas[index1].direction > 0)
          {
            if (__instance.workShipOrders[index1].itemId > 0)
            {
              if (__instance.storage[__instance.workShipOrders[index1].thisIndex].itemId == __instance.workShipOrders[index1].itemId)
                __instance.storage[__instance.workShipOrders[index1].thisIndex].remoteOrder -= __instance.workShipOrders[index1].thisOrdered;
              __instance.workShipOrders[index1].ClearThis();
              __instance.workShipOrders[index1].ClearOther();
            }
            __instance.workShipDatas[index1].otherGId = 0;
            __instance.workShipDatas[index1].direction = -1;
          }
          else if ((gStationPool[__instance.workShipDatas[index1].otherGId] == null || gStationPool[__instance.workShipDatas[index1].otherGId].gid == 0) && __instance.workShipDatas[index1].direction < 0)
          {
            __instance.workShipDatas[index1].otherGId = 0;
            __instance.workShipDatas[index1].direction = -1;
          }
          else if (__instance.workShipDatas[index1].itemCount > 0 && __instance.workShipDatas[index1].direction > 0 && __instance.workShipDatas[index1].otherGId > 0)
          {
            if (stationComponent.HasRemoteDemand(__instance.workShipDatas[index1].itemId, 0) == -1)
            {
              if (__instance.workShipOrders[index1].itemId > 0)
              {
                if (__instance.storage[__instance.workShipOrders[index1].thisIndex].itemId == __instance.workShipOrders[index1].itemId)
                  __instance.storage[__instance.workShipOrders[index1].thisIndex].remoteOrder -= __instance.workShipOrders[index1].thisOrdered;
                if (stationStoreArray[__instance.workShipOrders[index1].otherIndex].itemId == __instance.workShipOrders[index1].itemId)
                  stationStoreArray[__instance.workShipOrders[index1].otherIndex].remoteOrder -= __instance.workShipOrders[index1].otherOrdered;
                __instance.workShipOrders[index1].ClearThis();
                __instance.workShipOrders[index1].ClearOther();
              }
              __instance.workShipDatas[index1].otherGId = 0;
              __instance.workShipDatas[index1].direction = -1;
            }
          }
          else if (__instance.workShipDatas[index1].itemCount == 0 && __instance.workShipDatas[index1].direction > 0 && __instance.workShipDatas[index1].otherGId > 0)
          {
            int itemId = __instance.workShipDatas[index1].itemId;
            if (stationComponent.HasRemoteSupply(itemId) == -1)
            {
              if (__instance.workShipOrders[index1].itemId > 0)
              {
                if (__instance.storage[__instance.workShipOrders[index1].thisIndex].itemId == __instance.workShipOrders[index1].itemId)
                  __instance.storage[__instance.workShipOrders[index1].thisIndex].remoteOrder -= __instance.workShipOrders[index1].thisOrdered;
                if (stationStoreArray[__instance.workShipOrders[index1].otherIndex].itemId == __instance.workShipOrders[index1].itemId)
                  stationStoreArray[__instance.workShipOrders[index1].otherIndex].remoteOrder -= __instance.workShipOrders[index1].otherOrdered;
                __instance.workShipOrders[index1].ClearThis();
                __instance.workShipOrders[index1].ClearOther();
              }
              __instance.workShipDatas[index1].itemId = 0;
              for (int index4 = 0; index4 < __instance.storage.Length; ++index4)
              {
                if (__instance.storage[index4].remoteLogic == ELogisticStorage.Demand)
                {
                  int index5 = stationComponent.HasRemoteSupply(__instance.storage[index4].itemId, 1);
                  if (index5 >= 0)
                  {
                    __instance.workShipDatas[index1].itemId = __instance.storage[index4].itemId;
                    __instance.workShipDatas[index1].direction = 1;
                    __instance.workShipOrders[index1].itemId = __instance.workShipDatas[index1].itemId;
                    __instance.workShipOrders[index1].otherStationGId = __instance.workShipDatas[index1].otherGId;
                    __instance.workShipOrders[index1].thisIndex = index4;
                    __instance.workShipOrders[index1].otherIndex = index5;
                    __instance.workShipOrders[index1].thisOrdered = shipCarries;
                    __instance.workShipOrders[index1].otherOrdered = -shipCarries;
                    __instance.storage[index4].remoteOrder += shipCarries;
                    stationStoreArray[index5].remoteOrder -= shipCarries;
                    break;
                  }
                }
              }
              if (__instance.workShipDatas[index1].itemId == 0)
              {
                __instance.workShipDatas[index1].otherGId = 0;
                __instance.workShipDatas[index1].direction = -1;
              }
            }
          }
        }
      }
      return false;
    }
  }
}