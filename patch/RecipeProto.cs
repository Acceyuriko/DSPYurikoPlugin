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
  }
}