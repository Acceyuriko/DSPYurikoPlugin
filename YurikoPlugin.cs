using BepInEx;
using HarmonyLib;

namespace DSPYurikoPlugin
{
  [BepInPlugin("cc.acceyuriko.dsp", "YurikoPlugin", "1.0")]
  public class YurikoPlugin : BaseUnityPlugin
  {
    public void Start()
    {
      Harmony.CreateAndPatchAll(typeof(YurikoPlugin));
      Harmony.CreateAndPatchAll(typeof(GameMainPatch));
      Harmony.CreateAndPatchAll(typeof(GameHistoryDataPatch));
      Harmony.CreateAndPatchAll(typeof(StorageComponentPatch));
      Harmony.CreateAndPatchAll(typeof(CargoPathPatch));
      Harmony.CreateAndPatchAll(typeof(InserterComponentPatch));
      Harmony.CreateAndPatchAll(typeof(AssemblerComponentPatch));
      Harmony.CreateAndPatchAll(typeof(FractionateComponentPatch));
      Harmony.CreateAndPatchAll(typeof(LabComponentPatch));
      Harmony.CreateAndPatchAll(typeof(EjectorComponentPatch));
      Harmony.CreateAndPatchAll(typeof(SiloComponentPatch));
      Harmony.CreateAndPatchAll(typeof(StationComponentPatch));
      Harmony.CreateAndPatchAll(typeof(PowerGeneratorComponentPatch));
      Harmony.CreateAndPatchAll(typeof(DysonNodePatch));
      Harmony.CreateAndPatchAll(typeof(FactorySystemPatch));
      Harmony.CreateAndPatchAll(typeof(GameAbnormalityCheckPatch));
      Harmony.CreateAndPatchAll(typeof(PlanetGenPatch));
      Harmony.CreateAndPatchAll(typeof(StarGenPatch));
      Harmony.CreateAndPatchAll(typeof(UniverseGenPatch));
      Harmony.CreateAndPatchAll(typeof(PowerSystem));
      Harmony.CreateAndPatchAll(typeof(MechaPatch));
    }
  }
}
