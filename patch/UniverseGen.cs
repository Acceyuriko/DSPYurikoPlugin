using System;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace DSPYurikoPlugin
{
  public class UniverseGenPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UniverseGen), "CreateGalaxy")]
    public static bool UniverseGenCreateGalaxyPatch(
      ref GalaxyData __result,
      ref GameDesc gameDesc,
      ref List<VectorLF3> ___tmp_poses
    )
    {
      int galaxyAlgo = gameDesc.galaxyAlgo;
      int galaxySeed = gameDesc.galaxySeed;
      int starCount = gameDesc.starCount;
      if (galaxyAlgo < 20200101 || galaxyAlgo > 20591231)
        throw new Exception("Wrong version of unigen algorithm!");
      DotNet35Random dotNet35Random = new DotNet35Random(galaxySeed);
      MethodInfo generateTempPoses = typeof(UniverseGen).GetMethod("GenerateTempPoses", BindingFlags.NonPublic | BindingFlags.Static);
      double density = 1;
      int tempPoses = (int)generateTempPoses.Invoke(
        null,
        new object[] {
          dotNet35Random.Next(),
          starCount,
          4,
          2.0 / density,
          2.3 / density,
          3.5 / density,
          0.18
        }
      );
      GalaxyData galaxy = new GalaxyData();
      galaxy.seed = galaxySeed;
      galaxy.starCount = tempPoses;
      galaxy.stars = new StarData[tempPoses];
      Assert.Positive(tempPoses);
      if (tempPoses <= 0)
      {
        __result = galaxy;
        return false;
      }
      float num1 = (float)dotNet35Random.NextDouble();
      float num2 = (float)dotNet35Random.NextDouble();
      float num3 = (float)dotNet35Random.NextDouble();
      float num4 = (float)dotNet35Random.NextDouble();
      int num5 = Mathf.CeilToInt((float)(0.00999999977648258 * (double)tempPoses + (double)num1 * 0.300000011920929));
      int num6 = Mathf.CeilToInt((float)(0.00999999977648258 * (double)tempPoses + (double)num2 * 0.300000011920929));
      int num7 = Mathf.CeilToInt((float)(0.0160000007599592 * (double)tempPoses + (double)num3 * 0.400000005960464));
      int num8 = Mathf.CeilToInt((float)(0.0130000002682209 * (double)tempPoses + (double)num4 * 1.39999997615814));
      int num9 = tempPoses - num5;
      int num10 = num9 - num6;
      int num11 = num10 - num7;
      int num12 = (num11 - 1) / num8 / 4;
      int num13 = 0;
      for (int index = 0; index < tempPoses; ++index)
      {
        ESpectrType needSpectr = ESpectrType.X;
        double p = dotNet35Random.NextDouble();
        if (p > 0.85)
        {
          needSpectr = ESpectrType.O;
        }

        EStarType needtype = EStarType.MainSeqStar;
        if (index % num12 == num13)
          needtype = EStarType.GiantStar;
        if (index >= num9)
          needtype = EStarType.BlackHole;
        else if (index >= num10)
          needtype = EStarType.NeutronStar;
        else if (index >= num11)
          needtype = EStarType.WhiteDwarf;

        galaxy.stars[index] = StarGen.CreateStar(galaxy, ___tmp_poses[index], index + 1, dotNet35Random.Next(), needtype, needSpectr);
      }
      AstroPose[] astroPoses = galaxy.astroPoses;
      StarData[] stars = galaxy.stars;
      for (int index = 0; index < galaxy.astroPoses.Length; ++index)
      {
        astroPoses[index].uRot.w = 1f;
        astroPoses[index].uRotNext.w = 1f;
      }
      for (int index = 0; index < tempPoses; ++index)
      {
        StarGen.CreateStarPlanets(galaxy, stars[index], gameDesc);
        astroPoses[stars[index].id * 100].uPos = astroPoses[stars[index].id * 100].uPosNext = stars[index].uPosition;
        astroPoses[stars[index].id * 100].uRot = astroPoses[stars[index].id * 100].uRotNext = Quaternion.identity;
        astroPoses[stars[index].id * 100].uRadius = stars[index].physicsRadius;
      }
      galaxy.UpdatePoses(0.0);
      galaxy.birthPlanetId = 0;
      if (tempPoses > 0)
      {
        StarData starData = stars[0];
        for (int index = 0; index < starData.planetCount; ++index)
        {
          PlanetData planet = starData.planets[index];
          ThemeProto themeProto = LDB.themes.Select(planet.theme);
          if (themeProto != null && themeProto.Distribute == EThemeDistribute.Birth)
          {
            galaxy.birthPlanetId = planet.id;
            galaxy.birthStarId = starData.id;
            break;
          }
        }
      }
      Assert.Positive(galaxy.birthPlanetId);
      for (int index1 = 0; index1 < tempPoses; ++index1)
      {
        StarData star = galaxy.stars[index1];
        for (int index2 = 0; index2 < star.planetCount; ++index2)
          PlanetModelingManager.Algorithm(star.planets[index2]).GenerateVeins(true);
      }
      UniverseGen.CreateGalaxyStarGraph(galaxy);
      __result = galaxy;
      return false;
    }

  }
}