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
      GameMain.mainPlayer.mecha.miningSpeed *= YurikoConstants.DEFAULT_SPEED_RATIO;
      GameMain.mainPlayer.mecha.maxSailSpeed *= YurikoConstants.DEFAULT_SPEED_RATIO;
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
          if (proto != null && proto.prefabDesc != null && proto.prefabDesc.isPowerNode && proto.ID == YurikoConstants.MODEL_PROTO_ID_电力感应塔)
          {
            node.connectDistance = proto.prefabDesc.powerConnectDistance;
            node.coverRadius = proto.prefabDesc.powerCoverRadius;
          }
        }
        if (factory.planet != null && factory.planet.factoryModel != null)
        {
          factory.planet.factoryModel.RefreshPowerNodes();
        }
        for (int j = 1; j < factory.powerSystem.genCursor; j++)
        {
          ref var node = ref factory.powerSystem.genPool[j];
          if (node.gamma && node.productId > 0)
          {
            var proto = LDB.models.Select(factory.entityPool[node.entityId].modelIndex);
            if (proto != null && proto.prefabDesc != null && proto.prefabDesc.isPowerGen)
            {
              node.productHeat = proto.prefabDesc.powerProductHeat;
            }
          }
        }

        for (int j = 1; j < factory.cargoTraffic.beltCursor; j++)
        {
          ref var node = ref factory.cargoTraffic.beltPool[j];
          var proto = LDB.models.Select(factory.entityPool[node.entityId].modelIndex);
          if (proto != null && proto.prefabDesc != null && proto.prefabDesc.isBelt)
          {
            if (node.speed != proto.prefabDesc.beltSpeed)
            {
              node.speed = proto.prefabDesc.beltSpeed;
              factory.cargoTraffic.AlterBeltRenderer(j, factory.entityPool, factory.planet.physics.colChunks);
            }
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

        for (int j = 1; j < factory.factorySystem.assemblerCursor; j++)
        {
          ref var node = ref factory.factorySystem.assemblerPool[j];
          var proto = LDB.models.Select(factory.entityPool[node.entityId].modelIndex);
          if (proto != null && proto.prefabDesc != null && proto.prefabDesc.isAssembler && node.recipeId > 0)
          {
            node.speed = YurikoConstants.ASSEMBLER_SPEED;
            var recipeProto = LDB.recipes.Select(node.recipeId);
            if (recipeProto != null)
            {
              node.timeSpend = recipeProto.TimeSpend * 10000;
            }
          }
        }
        for (int j = 1; j < factory.factorySystem.labCursor; j++)
        {
          ref var node = ref factory.factorySystem.labPool[j];
          if (node.matrixMode)
          {
            var recipeProto = LDB.recipes.Select(node.recipeId);
            if (recipeProto != null)
            {
              node.timeSpend = recipeProto.TimeSpend * 10000;
            }
          }
        }

        for (int j = 1; j < factory.factorySystem.fractionateCursor; j++)
        {
          ref var node = ref factory.factorySystem.fractionatePool[j];
          if (node.fluidId > 0)
          {
            for (int k = 0; k < RecipeProto.fractionateRecipes.Length; k++)
            {
              var recipe = RecipeProto.fractionateRecipes[k];
              node.produceProb = (float)recipe.ResultCounts[0] / (float)recipe.ItemCounts[0];
            }
          }
        }

        for (int j = 1; j < factory.factorySystem.ejectorCursor; j++)
        {
          ref var node = ref factory.factorySystem.ejectorPool[j];
          var proto = LDB.models.Select(factory.entityPool[node.entityId].modelIndex);
          if (proto != null && proto.prefabDesc != null && proto.prefabDesc.isEjector)
          {
            node.coldSpend = proto.prefabDesc.ejectorChargeFrame * 10000;
            node.chargeSpend = proto.prefabDesc.ejectorColdFrame * 10000;
          }
        }

        for (int j = 1; j < factory.factorySystem.siloCursor; j++)
        {
          ref var node = ref factory.factorySystem.siloPool[j];
          var proto = LDB.models.Select(factory.entityPool[node.entityId].modelIndex);
          if (proto != null && proto.prefabDesc != null && proto.prefabDesc.isSilo)
          {
            node.coldSpend = proto.prefabDesc.siloColdFrame * 10000;
            node.chargeSpend = proto.prefabDesc.siloChargeFrame * 10000;
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