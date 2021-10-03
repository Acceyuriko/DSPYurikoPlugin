using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class GameMainPatch
  {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameMain), "Begin")]
    public static void Begin()
    {
      if (DSPGame.IsMenuDemo)
      {
        return;
      }
      foreach (var tech in LDB._techs.dataArray)
      {
        var ts = GameMain.history.TechState(tech.ID);
        if (!ts.unlocked)
        {
          for (var index = ts.curLevel; index < 20 && index <= ts.maxLevel; index++)
          {
            ++ts.curLevel;
            ts.hashUploaded = 0L;
            ts.hashNeeded = tech.GetHashNeeded(ts.curLevel);
            GameMainPatch.unlockTech(tech, ts);
          }
          if (ts.curLevel >= ts.maxLevel)
          {
            ts.curLevel = ts.maxLevel;
            ts.hashUploaded = ts.hashNeeded;
            ts.unlocked = true;
          }
          GameMain.history.techStates[tech.ID] = ts;
        }
      }

      GameMain.mainPlayer.mecha.walkSpeed *= 10;
      GameMain.mainPlayer.mecha.jumpSpeed *= 10;
      GameMain.mainPlayer.mecha.droneSpeed *= 10;
      GameMain.mainPlayer.mecha.miningSpeed *= 10;
      GameMain.mainPlayer.mecha.maxSailSpeed *= 10;
      GameMain.mainPlayer.mecha.maxWarpSpeed *= 10;
      GameMain.mainPlayer.mecha.replicateSpeed *= 10;
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameMain), "End")]
    public static bool End()
    {
      if (DSPGame.IsMenuDemo)
      {
        return true;
      }

      GameMain.mainPlayer.mecha.walkSpeed /= 10;
      GameMain.mainPlayer.mecha.jumpSpeed /= 10;
      GameMain.mainPlayer.mecha.droneSpeed /= 10;
      GameMain.mainPlayer.mecha.miningSpeed /= 10;
      GameMain.mainPlayer.mecha.maxSailSpeed /= 10;
      GameMain.mainPlayer.mecha.maxWarpSpeed /= 10;
      GameMain.mainPlayer.mecha.replicateSpeed /= 10;
      return true;
    }

    private static void unlockTech(TechProto tech, TechState ts)
    {
      for (int index = 0; index < tech.UnlockRecipes.Length; ++index)
      {
        GameMain.history.UnlockRecipe(tech.UnlockRecipes[index]);
      }
      for (int index = 0; index < tech.UnlockFunctions.Length; ++index)
      {
        GameMain.history.UnlockTechFunction(tech.UnlockFunctions[index], tech.UnlockValues[index], ts.curLevel);
      }
      for (int index = 0; index < tech.AddItems.Length; ++index)
      {
        GameMain.history.GainTechAwards(tech.AddItems[index], tech.AddItemCounts[index]);
      }
      if (tech.ID > 1)
      {
        GameMain.history.RegFeatureKey(1000100);
      }
    }
    private static BepInEx.Logging.ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("YurikoGameMainBegin");
  }
}