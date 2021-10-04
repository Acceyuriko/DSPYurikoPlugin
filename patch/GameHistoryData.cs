using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;

namespace DSPYurikoPlugin
{
  public class GameHistoryDataPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameHistoryData), "UnlockTechFunction")]
    public static bool UnlockTechFunction(
      ref GameHistoryData __instance,
      int func,
      double value,
      int level
    )
    {
      int num = value > 0.0 ? (int)(value + 0.5) : (int)(value - 0.5);
      Player mainPlayer = GameMain.mainPlayer;
      Mecha mecha = mainPlayer.mecha;
      switch (func)
      {
        case 1:
          mecha.droneCount += num;
          break;
        case 2:
          mecha.reactorPowerGen += value;
          break;
        case 3:
          mecha.walkSpeed += (float)value * YurikoConstants.WALK_SPEED_RATIO;
          break;
        case 4:
          mecha.thrusterLevel = num;
          break;
        case 5:
          mainPlayer.package.SetSize(mainPlayer.package.size + num * 10);
          break;
        case 6:
          mainPlayer.mecha.coreEnergyCap += value;
          mainPlayer.mecha.coreLevel = level;
          break;
        case 7:
          mainPlayer.mecha.replicateSpeed += (float)value * YurikoConstants.DEFAULT_SPEED_RATIO;
          break;
        case 8:
          __instance.useIonLayer = true;
          break;
        case 9:
          mainPlayer.mecha.droneMovement += num;
          break;
        case 10:
          mainPlayer.mecha.droneSpeed += (float)value * YurikoConstants.DEFAULT_SPEED_RATIO;
          break;
        case 11:
          mainPlayer.mecha.maxSailSpeed += (float)value * YurikoConstants.DEFAULT_SPEED_RATIO;
          break;
        case 12:
          __instance.solarSailLife += (float)value;
          break;
        case 13:
          __instance.solarEnergyLossRate *= (float)value;
          break;
        case 14:
          __instance.inserterStackCount = num;
          break;
        case 15:
          __instance.logisticDroneSpeedScale += (float)value;
          break;
        case 16:
          __instance.logisticShipSpeedScale += (float)value;
          break;
        case 17:
          __instance.logisticShipWarpDrive = true;
          break;
        case 18:
          __instance.logisticDroneCarries += num;
          break;
        case 19:
          __instance.logisticShipCarries += num;
          break;
        case 20:
          __instance.miningCostRate *= (float)value;
          break;
        case 21:
          __instance.miningSpeedScale += (float)value;
          __instance.miningSpeedScale = Mathf.Round(__instance.miningSpeedScale * 1000f) / 1000f;
          break;
        case 22:
          __instance.techSpeed += num;
          break;
        case 23:
          __instance.universeObserveLevel = num;
          break;
        case 24:
          __instance.storageLevel += num;
          break;
        case 25:
          __instance.labLevel += num;
          break;
        case 26:
          __instance.dysonNodeLatitude += (float)value;
          break;
        case 27:
          mainPlayer.mecha.maxWarpSpeed += (float)(value * 40000.0) * YurikoConstants.DEFAULT_SPEED_RATIO;
          break;
        case 28:
          __instance.blueprintLimit = num;
          break;
        case 99:
          __instance.missionAccomplished = true;
          break;
      }
      return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameHistoryData), "Export")]
    public static bool Export(ref GameHistoryData __instance, BinaryWriter w)
    {
      w.Write(6);
      w.Write(__instance.recipeUnlocked.Count);
      foreach (int num in __instance.recipeUnlocked)
        w.Write(num);
      w.Write(__instance.tutorialUnlocked.Count);
      foreach (int num in __instance.tutorialUnlocked)
        w.Write(num);
      w.Write(__instance.featureKeys.Count);
      foreach (int featureKey in __instance.featureKeys)
        w.Write(featureKey);
      w.Write(__instance.featureValues.Count);
      foreach (KeyValuePair<int, int> featureValue in __instance.featureValues)
      {
        w.Write(featureValue.Key);
        w.Write(featureValue.Value);
      }
      __instance.journalSystem.Export(w);
      w.Write(__instance.techStates.Count);
      foreach (KeyValuePair<int, global::TechState> techState in __instance.techStates)
      {
        w.Write(techState.Key);
        w.Write(techState.Value.unlocked);
        w.Write(techState.Value.curLevel);
        w.Write(techState.Value.maxLevel);
        w.Write(techState.Value.hashUploaded);
        w.Write(techState.Value.hashNeeded);
      }
      w.Write(__instance.autoManageLabItems);
      w.Write(__instance.currentTech);
      w.Write(__instance.techQueue.Length);
      for (int index = 0; index < __instance.techQueue.Length; ++index)
        w.Write(__instance.techQueue[index]);
      w.Write(__instance.universeObserveLevel);
      w.Write(__instance.blueprintLimit);
      w.Write(__instance.solarSailLife);
      w.Write(__instance.solarEnergyLossRate);
      w.Write(__instance.useIonLayer);
      w.Write(__instance.inserterStackCount);
      w.Write(__instance.logisticDroneSpeed / YurikoConstants.DEFAULT_SPEED_RATIO);
      w.Write(__instance.logisticDroneSpeedScale);
      w.Write(__instance.logisticDroneCarries);
      w.Write(__instance.logisticShipSailSpeed / YurikoConstants.DEFAULT_SPEED_RATIO);
      w.Write(__instance.logisticShipWarpSpeed / YurikoConstants.DEFAULT_SPEED_RATIO);
      w.Write(__instance.logisticShipSpeedScale);
      w.Write(__instance.logisticShipWarpDrive);
      w.Write(__instance.logisticShipCarries);
      w.Write(__instance.miningCostRate);
      w.Write(__instance.miningSpeedScale);
      w.Write(__instance.storageLevel);
      w.Write(__instance.labLevel);
      w.Write(__instance.techSpeed);
      w.Write(__instance.dysonNodeLatitude);
      w.Write(__instance.universeMatrixPointUploaded);
      w.Write(__instance.missionAccomplished);
      return false;
    }
  }
}