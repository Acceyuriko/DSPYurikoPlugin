using System.Reflection;
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

      for (int i = 0; i < GameMain.data.factoryCount; i++)
      {
        ref var factory = ref GameMain.data.factories[i];
        for (int j = 1; j < factory.powerSystem.nodeCursor; j++)
        {
          ref var node = ref factory.powerSystem.nodePool[j];
          var proto = LDB.models.Select(factory.entityPool[node.entityId].modelIndex);
          if (proto != null && proto.prefabDesc != null && proto.prefabDesc.isPowerNode)
          {
            node.connectDistance = proto.prefabDesc.powerConnectDistance;
            node.coverRadius = proto.prefabDesc.powerCoverRadius;
          }
        }
        if (factory.planet != null && factory.planet.factoryModel != null)
        {
          factory.planet.factoryModel.RefreshPowerNodes();
        }

        for (int j = 1; j < factory.factorySystem.assemblerCursor; j++)
        {
          ref var node = ref factory.factorySystem.assemblerPool[j];
          var proto = LDB.models.Select(factory.entityPool[node.entityId].modelIndex);
          if (proto != null && proto.prefabDesc != null && proto.prefabDesc.isAssembler)
          {
            node.speed = proto.prefabDesc.assemblerSpeed;
          }
        }

        for (int j = 1; j < factory.cargoTraffic.beltCursor; j++)
        {
          ref var node = ref factory.cargoTraffic.beltPool[j];
          var proto = LDB.models.Select(factory.entityPool[node.entityId].modelIndex);
          if (proto != null && proto.prefabDesc != null && proto.prefabDesc.isBelt)
          {
            node.speed = proto.prefabDesc.beltSpeed;
            factory.cargoTraffic.AlterBeltRenderer(j, factory.entityPool, factory.planet.physics.colChunks);
          }
        }
        for (int j = 1; j < factory.cargoTraffic.pathCursor; j++)
        {
          ref var path = ref factory.cargoTraffic.pathPool[j];
          if (path != null)
          {
            var chunkCount = (int)(typeof(CargoPath).GetField("chunkCount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(path));
            for (int k = 0; k < chunkCount; k++)
            {
              path.chunks[k * 3 + 2] = YurikoConstants.BELT_SPEED;
            }
          }
        }
      }

      foreach (var tech in LDB.techs.dataArray)
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