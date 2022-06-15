using ABN;
using HarmonyLib;

namespace DSPYurikoPlugin
{
    public class AbnormalityLogicPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AbnormalityLogic), "GameTick")]
        public static bool GameTick()
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameAbnormalityData_0925), "TriggerAbnormality")]
        public static bool TriggerAbnormality()
        {
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameAbnormalityData_0925), "Import")]
        public static void Import(ref GameAbnormalityData_0925 __instance)
        {
            __instance.runtimeDatas = new AbnormalityRuntimeData[3000];
        }
    }
}
