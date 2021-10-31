using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class AchievementSystemPatch
  {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AchievementSystem), "Start")]
    public static void Start()
    {
      foreach (var ach in LDB.achievements.dataArray)
      {
        if (!DSPGame.achievementSystem.achievements.ContainsKey(ach.ID))
        {
          continue;
        }
        var state = DSPGame.achievementSystem.achievements[ach.ID];
        YurikoLogging.logger.LogInfo($"ID: {ach.ID}, desc: {ach.Description}, unlock: {state.unlocked}");
        if (state.unlocked)
        {
          continue;
        }
        YurikoLogging.logger.LogInfo($"ID: {ach.ID}, desc: {ach.Description}");
      }
    }
  }
}