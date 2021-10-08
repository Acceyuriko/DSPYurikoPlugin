using System.IO;
using HarmonyLib;

namespace DSPYurikoPlugin
{
  public static class MechaPatch
  {

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Mecha), "Export")]
    public static bool Export(ref Mecha __instance, BinaryWriter w)
    {
      w.Write(1);
      w.Write(__instance.coreEnergyCap);
      w.Write(__instance.coreEnergy);
      w.Write(__instance.corePowerGen);
      w.Write(__instance.reactorPowerGen);
      w.Write(__instance.reactorEnergy);
      w.Write(__instance.reactorItemId);
      __instance.reactorStorage.Export(w);
      __instance.warpStorage.Export(w);
      w.Write(__instance.walkPower);
      w.Write(__instance.jumpEnergy);
      w.Write(__instance.thrustPowerPerAcc);
      w.Write(__instance.warpKeepingPowerPerSpeed);
      w.Write(__instance.warpStartPowerPerSpeed);
      w.Write(__instance.miningPower);
      w.Write(__instance.replicatePower);
      w.Write(__instance.researchPower);
      w.Write(__instance.droneEjectEnergy);
      w.Write(__instance.droneEnergyPerMeter);
      w.Write(__instance.coreLevel);
      w.Write(__instance.thrusterLevel);
      w.Write(__instance.miningSpeed / YurikoConstants.DEFAULT_SPEED_RATIO);
      w.Write(__instance.replicateSpeed / YurikoConstants.DEFAULT_SPEED_RATIO);
      w.Write(__instance.walkSpeed / YurikoConstants.WALK_SPEED_RATIO);
      w.Write(__instance.jumpSpeed);
      w.Write(__instance.maxSailSpeed / YurikoConstants.DEFAULT_SPEED_RATIO);
      w.Write(__instance.maxWarpSpeed);
      w.Write(__instance.buildArea);
      __instance.forge.Export(w);
      __instance.lab.Export(w);
      w.Write(__instance.droneCount);
      w.Write(__instance.droneSpeed / YurikoConstants.DEFAULT_SPEED_RATIO);
      w.Write(__instance.droneMovement);
      for (int index = 0; index < __instance.droneCount; ++index)
        __instance.drones[index].Export(w);
      w.Write(__instance.mainColors.Length);
      for (int index = 0; index < __instance.mainColors.Length; ++index)
      {
        w.Write(__instance.mainColors[index].r);
        w.Write(__instance.mainColors[index].g);
        w.Write(__instance.mainColors[index].b);
        w.Write(__instance.mainColors[index].a);
      }
      return false;
    }
  }
}