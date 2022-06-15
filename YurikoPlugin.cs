using BepInEx;
using HarmonyLib;

namespace DSPYurikoPlugin
{
    [BepInPlugin("cc.acceyuriko.dsp", "YurikoPlugin", "1.0.7")]
    public class YurikoPlugin : BaseUnityPlugin
    {
        public void Start()
        {
            Harmony.CreateAndPatchAll(typeof(AchievementLogicPatch));
            Harmony.CreateAndPatchAll(typeof(AbnormalityLogicPatch));
        }
    }
}
