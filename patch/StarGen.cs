using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DSPYurikoPlugin
{
  public class StarGenPatch
  {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StarGen), "CreateStar")]
    public static bool StarGenCreateStarPatch(
      ref StarData __result,
      GalaxyData galaxy,
      VectorLF3 pos,
      int id,
      int seed,
      EStarType needtype,
      ESpectrType needSpectr = ESpectrType.X
    )
    {
      StarData starData = new StarData()
      {
        galaxy = galaxy,
        index = id - 1
      };
      starData.level = galaxy.starCount <= 1 ? 0.0f : (float)starData.index / (float)(galaxy.starCount - 1);
      starData.id = id;
      starData.seed = seed;
      DotNet35Random dotNet35Random1 = new DotNet35Random(seed);
      int seed1 = dotNet35Random1.Next();
      int Seed = dotNet35Random1.Next();
      starData.position = pos;
      float num1 = (float)pos.magnitude / 32f;
      if ((double)num1 > 1.0)
        num1 = Mathf.Log(Mathf.Log(Mathf.Log(Mathf.Log(Mathf.Log(num1) + 1f) + 1f) + 1f) + 1f) + 1f;
      starData.resourceCoef = Mathf.Pow(7f, num1) * 0.6f;
      DotNet35Random dotNet35Random2 = new DotNet35Random(Seed);
      double r1 = dotNet35Random2.NextDouble();
      double r2 = dotNet35Random2.NextDouble();
      double num2 = dotNet35Random2.NextDouble();
      double rn = dotNet35Random2.NextDouble();
      double rt = dotNet35Random2.NextDouble();
      double num3 = (dotNet35Random2.NextDouble() - 0.5) * 0.2;
      double num4 = dotNet35Random2.NextDouble() * 0.2 + 0.9;
      double y = dotNet35Random2.NextDouble() * 0.4 - 0.2;
      double num5 = Math.Pow(2.0, y);
      float num6 = Mathf.Lerp(-0.98f, 0.88f, starData.level);
      float averageValue = (double)num6 >= 0.0 ? num6 + 0.65f : num6 - 0.65f;
      float standardDeviation = 0.33f;
      if (needtype == EStarType.GiantStar)
      {
        averageValue = y > -0.08 ? -1.5f : 1.6f;
        standardDeviation = 0.3f;
      }
      MethodInfo randNormal = typeof(StarGen).GetMethod("RandNormal", BindingFlags.NonPublic | BindingFlags.Static);
      float num7 = (float)randNormal.Invoke(null, new object[] { averageValue, standardDeviation, r1, r2 });
      switch (needSpectr)
      {
        case ESpectrType.M:
          num7 = -3f;
          break;
        case ESpectrType.O:
          num7 = 3f;
          break;
      }
      float p1 = (float)((double)Mathf.Clamp((double)num7 <= 0.0 ? num7 * 1f : num7 * 2f, -2.4f, 4.65f) + num3 + 1.0);
      switch (needtype)
      {
        case EStarType.WhiteDwarf:
          starData.mass = (float)(1.0 + r2 * 5.0);
          break;
        case EStarType.NeutronStar:
          starData.mass = (float)(7.0 + r1 * 11.0);
          break;
        case EStarType.BlackHole:
          starData.mass = (float)(18.0 + r1 * r2 * 30.0);
          break;
        default:
          starData.mass = Mathf.Pow(2f, p1);
          break;
      }
      double d = 5.0;
      if ((double)starData.mass < 2.0)
        d = 2.0 + 0.4 * (1.0 - (double)starData.mass);
      starData.lifetime = (float)(10000.0 * Math.Pow(0.1, Math.Log10((double)starData.mass * 0.5) / Math.Log10(d) + 1.0) * num4);
      switch (needtype)
      {
        case EStarType.GiantStar:
          starData.lifetime = (float)(10000.0 * Math.Pow(0.1, Math.Log10((double)starData.mass * 0.58) / Math.Log10(d) + 1.0) * num4);
          starData.age = (float)(num2 * 0.0399999991059303 + 0.959999978542328);
          break;
        case EStarType.WhiteDwarf:
        case EStarType.NeutronStar:
        case EStarType.BlackHole:
          starData.age = (float)(num2 * 0.400000005960464 + 1.0);
          if (needtype == EStarType.WhiteDwarf)
          {
            starData.lifetime += 10000f;
            break;
          }
          if (needtype == EStarType.NeutronStar)
          {
            starData.lifetime += 1000f;
            break;
          }
          break;
        default:
          starData.age = (double)starData.mass >= 0.5 ? ((double)starData.mass >= 0.8 ? (float)(num2 * 0.699999988079071 + 0.200000002980232) : (float)(num2 * 0.400000005960464 + 0.100000001490116)) : (float)(num2 * 0.119999997317791 + 0.0199999995529652);
          break;
      }
      float num8 = starData.lifetime * starData.age;
      if ((double)num8 > 5000.0)
        num8 = (float)(((double)Mathf.Log(num8 / 5000f) + 1.0) * 5000.0);
      if ((double)num8 > 8000.0)
        num8 = (float)(((double)Mathf.Log(Mathf.Log(Mathf.Log(num8 / 8000f) + 1f) + 1f) + 1.0) * 8000.0);
      starData.lifetime = num8 / starData.age;
      float f = (float)(1.0 - (double)Mathf.Pow(Mathf.Clamp01(starData.age), 20f) * 0.5) * starData.mass;
      starData.temperature = (float)(Math.Pow((double)f, 0.56 + 0.14 / (Math.Log10((double)f + 4.0) / Math.Log10(5.0))) * 4450.0 + 1300.0);
      double num9 = Math.Log10(((double)starData.temperature - 1300.0) / 4500.0) / Math.Log10(2.6) - 0.5;
      if (num9 < 0.0)
      {
        num9 *= 4.0;
      }
      if (num9 > 2.0)
        num9 = 2.0;
      else if (num9 < -4.0)
        num9 = -4.0;
      starData.spectr = (ESpectrType)Mathf.RoundToInt((float)num9 + 4f);
      starData.color = Mathf.Clamp01((float)((num9 + 3.5) * 0.200000002980232));
      starData.classFactor = (float)num9;
      starData.luminosity = Mathf.Pow(f, 0.7f);
      starData.radius = (float)(Math.Pow((double)starData.mass, 0.4) * num5);
      starData.acdiskRadius = 0.0f;
      float p2 = (float)num9 + 2f;
      starData.habitableRadius = Mathf.Pow(1.7f, p2) + 0.25f * Mathf.Min(1f, starData.orbitScaler);
      starData.lightBalanceRadius = Mathf.Pow(1.7f, p2);
      starData.orbitScaler = Mathf.Pow(1.35f, p2);
      if ((double)starData.orbitScaler < 1.0)
        starData.orbitScaler = Mathf.Lerp(starData.orbitScaler, 1f, 0.6f);
      StarGen.SetStarAge(starData, starData.age, rn, rt);
      starData.dysonRadius = starData.orbitScaler * 0.28f * 1.5f;
      if ((double)starData.dysonRadius * 40000.0 < (double)starData.physicsRadius * 1.5)
        starData.dysonRadius = (float)((double)starData.physicsRadius * 1.5 / 40000.0);
      starData.uPosition = starData.position * 2400000.0;
      starData.name = NameGen.RandomStarName(seed1, starData, galaxy);
      starData.overrideName = "";
      starData.radius *= 2;
      __result = starData;
      return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StarGen), "CreateStarPlanets")]
    public static bool StarGenCreateStarPlanetsPatch(
      GalaxyData galaxy,
      StarData star,
      GameDesc gameDesc,
      double[] ___pGas
    )
    {
      DotNet35Random dotNet35Random1 = new DotNet35Random(star.seed);
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      dotNet35Random1.Next();
      DotNet35Random dotNet35Random2 = new DotNet35Random(dotNet35Random1.Next());
      double num1 = dotNet35Random2.NextDouble();
      double num2 = dotNet35Random2.NextDouble();
      double num3 = dotNet35Random2.NextDouble();
      double num4 = dotNet35Random2.NextDouble();
      double num5 = dotNet35Random2.NextDouble();
      double num6 = dotNet35Random2.NextDouble() * 0.2 + 0.9;
      double num7 = dotNet35Random2.NextDouble() * 0.2 + 0.9;
      {
        Array.Clear((Array)___pGas, 0, ___pGas.Length);
        star.planetCount = num1 >= 0.1 ? (num1 >= 0.2 ? (num1 >= 0.7 ? (num1 >= 0.95 ? 9 : 8) : 7) : 6) : 5;
        ___pGas[0] = 0.1;
        ___pGas[1] = 0.2;
        ___pGas[2] = 0.25;
        ___pGas[3] = 0.3;
        ___pGas[4] = 0.32;
        ___pGas[5] = 0.35;
        ___pGas[6] = 0.38;
        ___pGas[7] = 0.40;
        ___pGas[8] = 0.42;
        star.planets = new PlanetData[star.planetCount];
        int num8 = 0;
        int num9 = 0;
        int orbitAround = 0;
        int num10 = 1;
        for (int index = 0; index < star.planetCount; ++index)
        {
          int info_seed = dotNet35Random2.Next();
          int gen_seed = dotNet35Random2.Next();
          double num11 = dotNet35Random2.NextDouble();
          double num12 = dotNet35Random2.NextDouble();
          bool gasGiant = false;
          if (orbitAround == 0)
          {
            ++num8;
            if (index < star.planetCount - 1 && num11 < ___pGas[index])
            {
              gasGiant = true;
              if (num10 < 3)
                num10 = 3;
            }
            for (; star.index != 0 || num10 != 3; ++num10)
            {
              int num13 = star.planetCount - index;
              int num14 = 9 - num10;
              if (num14 > num13)
              {
                float a = (float)num13 / (float)num14;
                float num15 = num10 <= 3 ? Mathf.Lerp(a, 1f, 0.15f) + 0.01f : Mathf.Lerp(a, 1f, 0.45f) + 0.01f;
                if (dotNet35Random2.NextDouble() < (double)num15)
                  goto label_62;
              }
              else
                goto label_62;
            }
            gasGiant = true;
          }
          else
          {
            ++num9;
            gasGiant = false;
          }
        label_62:
          star.planets[index] = PlanetGen.CreatePlanet(galaxy, star, gameDesc.savedThemeIds, index, orbitAround, orbitAround == 0 ? num10 : num9, orbitAround == 0 ? num8 : num9, gasGiant, info_seed, gen_seed);
          ++num10;
          if (gasGiant)
          {
            orbitAround = num8;
            num9 = 0;
          }
          if (num9 >= 1 && num12 < 0.8)
          {
            orbitAround = 0;
            num9 = 0;
          }
        }
      }
      int num16 = 0;
      int num17 = 0;
      int index1 = 0;
      for (int index2 = 0; index2 < star.planetCount; ++index2)
      {
        if (star.planets[index2].type == EPlanetType.Gas)
        {
          num16 = star.planets[index2].orbitIndex;
          break;
        }
      }
      for (int index3 = 0; index3 < star.planetCount; ++index3)
      {
        if (star.planets[index3].orbitAround == 0)
          num17 = star.planets[index3].orbitIndex;
      }
      if (num16 > 0)
      {
        int num18 = num16 - 1;
        bool flag = true;
        for (int index4 = 0; index4 < star.planetCount; ++index4)
        {
          if (star.planets[index4].orbitAround == 0 && star.planets[index4].orbitIndex == num16 - 1)
          {
            flag = false;
            break;
          }
        }
        if (flag && num4 < 0.2 + (double)num18 * 0.2)
          index1 = num18;
      }
      int index5 = num5 >= 0.2 ? (num5 >= 0.4 ? (num5 >= 0.8 ? 0 : num17 + 1) : num17 + 2) : num17 + 3;
      if (index5 != 0 && index5 < 5)
        index5 = 5;
      star.asterBelt1OrbitIndex = (float)index1;
      star.asterBelt2OrbitIndex = (float)index5;
      if (index1 > 0)
        star.asterBelt1Radius = StarGen.orbitRadius[index1] * (float)num6 * star.orbitScaler;
      if (index5 <= 0)
        return false;
      star.asterBelt2Radius = StarGen.orbitRadius[index5] * (float)num7 * star.orbitScaler;
      return false;
    }

  }
}