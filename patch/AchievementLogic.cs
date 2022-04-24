using HarmonyLib;

namespace DSPYurikoPlugin
{
    public class AchievementLogicPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AchievementLogic), "active", MethodType.Getter)]
        public static bool Active(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}