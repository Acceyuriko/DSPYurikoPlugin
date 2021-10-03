using System;
using HarmonyLib;
using UnityEngine;

namespace DSPYurikoPlugin
{
  public class PlanetGenPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlanetGen), "CreatePlanet")]
    public static bool PlanetGenCreatePlanetPatch(
      ref PlanetData __result,
      GalaxyData galaxy,
      StarData star,
      int[] themeIds,
      int index,
      int orbitAround,
      int orbitIndex,
      int number,
      bool gasGiant,
      int info_seed,
      int gen_seed
    )
    {
      PlanetData planet = new PlanetData();
      DotNet35Random dotNet35Random = new DotNet35Random(info_seed);
      planet.index = index;
      planet.galaxy = star.galaxy;
      planet.star = star;
      planet.seed = gen_seed;
      planet.infoSeed = info_seed;
      planet.orbitAround = orbitAround;
      planet.orbitIndex = orbitIndex;
      planet.number = number;
      planet.id = star.id * 100 + index + 1;
      StarData[] stars = galaxy.stars;
      int num1 = 0;
      for (int index1 = 0; index1 < star.index; ++index1)
        num1 += stars[index1].planetCount;
      int num2 = num1 + index;
      if (orbitAround > 0)
      {
        for (int index2 = 0; index2 < star.planetCount; ++index2)
        {
          if (orbitAround == star.planets[index2].number && star.planets[index2].orbitAround == 0)
          {
            planet.orbitAroundPlanet = star.planets[index2];
            if (orbitIndex > 1)
            {
              planet.orbitAroundPlanet.singularity |= EPlanetSingularity.MultipleSatellites;
              break;
            }
            break;
          }
        }
        Assert.NotNull((object)planet.orbitAroundPlanet);
      }
      string str = star.planetCount > 20 ? (index + 1).ToString() : NameGen.roman[index + 1];
      planet.name = star.name + " " + str + "号星".Translate();
      double num3 = dotNet35Random.NextDouble();
      double num4 = dotNet35Random.NextDouble();
      double num5 = dotNet35Random.NextDouble();
      double num6 = dotNet35Random.NextDouble();
      double num7 = dotNet35Random.NextDouble();
      double num8 = dotNet35Random.NextDouble();
      double num9 = dotNet35Random.NextDouble();
      double num10 = dotNet35Random.NextDouble();
      double num11 = dotNet35Random.NextDouble();
      double num12 = dotNet35Random.NextDouble();
      double num13 = dotNet35Random.NextDouble();
      double num14 = dotNet35Random.NextDouble();
      double rand1 = dotNet35Random.NextDouble();
      double num15 = dotNet35Random.NextDouble();
      double rand2 = dotNet35Random.NextDouble();
      double rand3 = dotNet35Random.NextDouble();
      double rand4 = dotNet35Random.NextDouble();
      int theme_seed = dotNet35Random.Next();
      float a = Mathf.Pow(1.2f, (float)(num3 * (num4 - 0.5) * 0.5));
      float f1;
      if (orbitAround == 0)
      {
        float b = StarGen.orbitRadius[orbitIndex] * star.orbitScaler;
        float num16 = (float)(((double)a - 1.0) / (double)Mathf.Max(1f, b) + 1.0);
        f1 = b * num16;
      }
      else
        f1 = (float)(((1600.0 * (double)orbitIndex + 200.0) * (double)Mathf.Pow(star.orbitScaler, 0.3f) * (double)Mathf.Lerp(a, 1f, 0.5f) + (double)planet.orbitAroundPlanet.realRadius) / 40000.0);
      planet.orbitRadius = f1;
      planet.orbitInclination = (float)(num5 * 16.0 - 8.0);
      if (orbitAround > 0)
        planet.orbitInclination *= 2.2f;
      planet.orbitLongitude = (float)(num6 * 360.0);
      if (star.type >= EStarType.NeutronStar)
      {
        if ((double)planet.orbitInclination > 0.0)
          planet.orbitInclination += 3f;
        else
          planet.orbitInclination -= 3f;
      }
      planet.orbitalPeriod = planet.orbitAroundPlanet != null ? Math.Sqrt(39.4784176043574 * (double)f1 * (double)f1 * (double)f1 / 1.08308421068537E-08) : Math.Sqrt(39.4784176043574 * (double)f1 * (double)f1 * (double)f1 / (1.35385519905204E-06 * (double)star.mass));
      planet.orbitPhase = (float)(num7 * 360.0);
      if (num15 < 0.0399999991059303)
      {
        planet.obliquity = (float)(num8 * (num9 - 0.5) * 39.9);
        if ((double)planet.obliquity < 0.0)
          planet.obliquity -= 70f;
        else
          planet.obliquity += 70f;
        planet.singularity |= EPlanetSingularity.LaySide;
      }
      else if (num15 < 0.100000001490116)
      {
        planet.obliquity = (float)(num8 * (num9 - 0.5) * 80.0);
        if ((double)planet.obliquity < 0.0)
          planet.obliquity -= 30f;
        else
          planet.obliquity += 30f;
      }
      else
        planet.obliquity = (float)(num8 * (num9 - 0.5) * 60.0);
      planet.rotationPeriod = (num10 * num11 * 1000.0 + 400.0) * (orbitAround == 0 ? (double)Mathf.Pow(f1, 0.25f) : 1.0) * (gasGiant ? 0.200000002980232 : 1.0);
      if (!gasGiant)
      {
        if (star.type == EStarType.WhiteDwarf)
          planet.rotationPeriod *= 0.5;
        else if (star.type == EStarType.NeutronStar)
          planet.rotationPeriod *= 0.200000002980232;
        else if (star.type == EStarType.BlackHole)
          planet.rotationPeriod *= 0.150000005960464;
      }
      planet.rotationPhase = (float)(num12 * 360.0);
      planet.sunDistance = orbitAround == 0 ? planet.orbitRadius : planet.orbitAroundPlanet.orbitRadius;
      planet.scale = 1f;
      double num17 = orbitAround == 0 ? planet.orbitalPeriod : planet.orbitAroundPlanet.orbitalPeriod;
      planet.rotationPeriod = 1.0 / (1.0 / num17 + 1.0 / planet.rotationPeriod);
      if (orbitAround == 0 && orbitIndex <= 4 && !gasGiant)
      {
        if (num15 > 0.959999978542328)
        {
          planet.obliquity *= 0.01f;
          planet.rotationPeriod = planet.orbitalPeriod;
          planet.singularity |= EPlanetSingularity.TidalLocked;
        }
        else if (num15 > 0.930000007152557)
        {
          planet.obliquity *= 0.1f;
          planet.rotationPeriod = planet.orbitalPeriod * 0.5;
          planet.singularity |= EPlanetSingularity.TidalLocked2;
        }
        else if (num15 > 0.899999976158142)
        {
          planet.obliquity *= 0.2f;
          planet.rotationPeriod = planet.orbitalPeriod * 0.25;
          planet.singularity |= EPlanetSingularity.TidalLocked4;
        }
      }
      if (num15 > 0.85 && num15 <= 0.9)
      {
        planet.rotationPeriod = -planet.rotationPeriod;
        planet.singularity |= EPlanetSingularity.ClockwiseRotate;
      }
      planet.runtimeOrbitRotation = Quaternion.AngleAxis(planet.orbitLongitude, Vector3.up) * Quaternion.AngleAxis(planet.orbitInclination, Vector3.forward);
      if (planet.orbitAroundPlanet != null)
        planet.runtimeOrbitRotation = planet.orbitAroundPlanet.runtimeOrbitRotation * planet.runtimeOrbitRotation;
      planet.runtimeSystemRotation = planet.runtimeOrbitRotation * Quaternion.AngleAxis(planet.obliquity, Vector3.forward);
      float habitableRadius = star.habitableRadius;
      if (gasGiant)
      {
        planet.type = EPlanetType.Gas;
        planet.radius = 80f;
        planet.scale = 10f;
        planet.habitableBias = 100f;
      }
      else
      {
        float num18 = Mathf.Ceil((float)star.galaxy.starCount * 0.29f);
        if ((double)num18 < 11.0)
          num18 = 11f;
        double num19 = (double)num18 - (double)star.galaxy.habitableCount;
        float num20 = (float)(star.galaxy.starCount - star.index);
        float sunDistance = planet.sunDistance;
        float num21 = 1000f;
        float f2 = 1000f;
        if ((double)habitableRadius > 0.0 && (double)sunDistance > 0.0)
        {
          f2 = sunDistance / habitableRadius;
          num21 = Mathf.Abs(Mathf.Log(f2));
        }
        float num22 = Mathf.Clamp(Mathf.Sqrt(habitableRadius), 1f, 2f) - 0.04f;
        double num23 = (double)num20;
        float num24 = Mathf.Clamp(Mathf.Lerp((float)(num19 / num23), 0.35f, 0.5f), 0.08f, 0.8f);
        planet.habitableBias = num21 * num22;
        planet.temperatureBias = (float)(1.20000004768372 / ((double)f2 + 0.200000002980232) - 1.0);
        float num25 = Mathf.Pow(Mathf.Clamp01(planet.habitableBias / num24), num24 * 10f);
        if (num13 > (double)num25 && star.index > 0 || planet.orbitAround > 0 && planet.orbitIndex == 1 && star.index == 0)
        {
          planet.type = EPlanetType.Ocean;
          ++star.galaxy.habitableCount;
        }
        else if ((double)f2 < 0.833333015441895)
        {
          float num26 = Mathf.Max(0.15f, (float)((double)f2 * 2.5 - 0.850000023841858));
          planet.type = num14 >= (double)num26 ? EPlanetType.Vocano : EPlanetType.Desert;
        }
        else if ((double)f2 < 1.20000004768372)
        {
          planet.type = EPlanetType.Desert;
        }
        else
        {
          float num27 = (float)(0.899999976158142 / (double)f2 - 0.100000001490116);
          planet.type = num14 >= (double)num27 ? EPlanetType.Ice : EPlanetType.Desert;
        }
        planet.radius = 200f;
      }
      if (planet.type != EPlanetType.Gas && planet.type != EPlanetType.None)
      {
        planet.precision = 200;
        planet.segment = 5;
      }
      else
      {
        planet.precision = 64;
        planet.segment = 2;
      }
      planet.luminosity = Mathf.Pow(planet.star.lightBalanceRadius / (planet.sunDistance + 0.01f), 0.6f);
      if ((double)planet.luminosity > 1.0)
      {
        planet.luminosity = Mathf.Log(planet.luminosity) + 1f;
        planet.luminosity = Mathf.Log(planet.luminosity) + 1f;
        planet.luminosity = Mathf.Log(planet.luminosity) + 1f;
      }
      planet.luminosity = Mathf.Round(planet.luminosity * 100f) / 100f;
      PlanetGen.SetPlanetTheme(planet, themeIds, rand1, rand2, rand3, rand4, theme_seed);
      star.galaxy.astroPoses[planet.id].uRadius = planet.realRadius;
      planet.radius *= 2;
      __result = planet;
      return false;
    }

  }
}