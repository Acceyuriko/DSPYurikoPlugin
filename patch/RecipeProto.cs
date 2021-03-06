using HarmonyLib;

namespace DSPYurikoPlugin {
  public class RecipeProtoPatch {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RecipeProto), "Preload")]
    public static void Preload(ref RecipeProto __instance) {
      if (
        __instance.Type == ERecipeType.Smelt ||
        __instance.Type == ERecipeType.Chemical ||
        __instance.Type == ERecipeType.Refine ||
        __instance.Type == ERecipeType.Assemble ||
        __instance.Type == ERecipeType.Particle ||
        __instance.Type == ERecipeType.Research
      ) {
        __instance.TimeSpend /= YurikoConstants.RECIPE_TIME_SPEND_RATIO;
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RecipeProto), "InitFractionatorNeeds")]
    public static void InitFractionatorNeeds() {
      for (int i = 0; i < RecipeProto.fractionatorRecipes.Length; i++) {
        RecipeProto.fractionatorRecipes[i].ResultCounts[0] *= YurikoConstants.RECIPE_FRACTIONATE_RATIO;
      }
    }
  }
}