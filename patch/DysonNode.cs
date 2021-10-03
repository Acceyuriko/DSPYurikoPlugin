using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class DysonNodePatch
  {
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
  }
}