using System.IO;
using System.Collections.Generic;
using HarmonyLib;

namespace DSPYurikoPlugin
{
    public static class MechaPatch
    {

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Mecha), "Export")]
        public static bool Export(ref Mecha __instance, BinaryWriter w)
        {
            w.Write(6);
            w.Write(__instance.coreEnergyCap);
            w.Write(__instance.coreEnergy);
            w.Write(__instance.corePowerGen);
            w.Write(__instance.reactorPowerGen);
            w.Write(__instance.reactorEnergy);
            w.Write(__instance.reactorItemId);
            w.Write(__instance.reactorItemInc);
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
            w.Write(__instance.droneSpeed);
            w.Write(__instance.droneMovement);
            for (int index = 0; index < __instance.droneCount; ++index)
                __instance.drones[index].Export(w);
            __instance.appearance.Export(w);
            __instance.diyAppearance.Export(w);
            w.Write(__instance.diyItems.items.Count);
            foreach (KeyValuePair<int, int> keyValuePair in __instance.diyItems.items)
            {
                w.Write(keyValuePair.Key);
                w.Write(keyValuePair.Value);
            }
            w.Write(2119973658);
            return false;
        }
    }
}