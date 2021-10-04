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

      GameMain.mainPlayer.mecha.walkSpeed *= YurikoConstants.WALK_SPEED_RATIO;
      GameMain.mainPlayer.mecha.droneSpeed *= YurikoConstants.DEFAULT_SPEED_RATIO;
      GameMain.mainPlayer.mecha.miningSpeed *= YurikoConstants.DEFAULT_SPEED_RATIO;
      GameMain.mainPlayer.mecha.maxSailSpeed *= YurikoConstants.DEFAULT_SPEED_RATIO;
      GameMain.mainPlayer.mecha.maxWarpSpeed *= YurikoConstants.DEFAULT_SPEED_RATIO;
      GameMain.mainPlayer.mecha.replicateSpeed *= YurikoConstants.DEFAULT_SPEED_RATIO;

      GameMain.history.logisticShipSailSpeed *= YurikoConstants.DEFAULT_SPEED_RATIO;
      GameMain.history.logisticShipWarpSpeed *= YurikoConstants.DEFAULT_SPEED_RATIO;
      GameMain.history.logisticDroneSpeed *= YurikoConstants.DEFAULT_SPEED_RATIO;

      // for (int i = 0; i < GameMain.data.factoryCount; i++)
      // {
      //   ref var factory = ref GameMain.data.factories[i];
      //   for (int j = 1; j < factory.powerSystem.nodeCursor; j++)
      //   {
      //     ref var node = ref factory.powerSystem.nodePool[j];
      //     if (node.id == j)
      //     {
      //       node.connectDistance *= YurikoConstants.POWER_NODE_CONN_RATIO;
      //       node.coverRadius *= YurikoConstants.POWER_NODE_COVER_RATIO;
      //     }
      //   }
      // }

      foreach (var tech in LDB._techs.dataArray)
      {
        var ts = GameMain.history.TechState(tech.ID);
        if (!ts.unlocked)
        {
          for (var i = ts.curLevel; i < 20 && i <= ts.maxLevel; i++)
          {
            ++ts.curLevel;
            ts.hashUploaded = 0L;
            ts.hashNeeded = tech.GetHashNeeded(ts.curLevel);
            for (int j = 0; j < tech.UnlockRecipes.Length; ++j)
            {
              GameMain.history.UnlockRecipe(tech.UnlockRecipes[j]);
            }
            for (int j = 0; j < tech.UnlockFunctions.Length; ++j)
            {
              GameMain.history.UnlockTechFunction(tech.UnlockFunctions[j], tech.UnlockValues[j], ts.curLevel);
            }
            for (int j = 0; j < tech.AddItems.Length; ++j)
            {
              GameMain.history.GainTechAwards(tech.AddItems[j], tech.AddItemCounts[j]);
            }
            if (tech.ID > 1)
            {
              GameMain.history.RegFeatureKey(1000100);
            }
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
    }
  }
}