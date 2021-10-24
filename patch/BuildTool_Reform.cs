using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace DSPYurikoPlugin
{
  public class BuildTool_ReformPatch
  {
    [HarmonyPostfix]
    [HarmonyPatch(typeof(BuildTool_Reform), "_OnInit")]
    public static void _OnInit(ref BuildTool_Reform __instance) {
      __instance.cursorIndices = new int[maxBrushSize * maxBrushSize];
      __instance.cursorPoints = new Vector3[maxBrushSize * maxBrushSize];
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuildTool_Reform), "ReformAction")]
    public static bool ReformAction(ref BuildTool_Reform __instance)
    {
      bool flag1 = false;
      int[] consumeRegister = GameMain.statistics.production.factoryStatPool[__instance.factory.index].consumeRegister;
      bool drawing = __instance.drawing;
      if (__instance.brushSize < 1)
        __instance.brushSize = 1;
      else if (__instance.brushSize > maxBrushSize)
        __instance.brushSize = maxBrushSize;
      if ((double)(__instance.reformCenterPoint - __instance.player.position).sqrMagnitude > (double)__instance.player.mecha.buildArea * (double)__instance.player.mecha.buildArea)
      {
        if (!VFInput.onGUIOperate)
        {
          __instance.actionBuild.model.cursorText = "目标超出范围".Translate();
          __instance.actionBuild.model.cursorState = -1;
          UICursor.SetCursor(ECursor.Ban);
        }
      }
      else
      {
        if (!VFInput.onGUIOperate)
          UICursor.SetCursor(ECursor.Reform);
        bool flag2 = false;
        if (VFInput._cursorPlusKey.onDown)
        {
          if (__instance.brushSize < maxBrushSize)
          {
            ++__instance.brushSize;
            flag2 = true;
            for (int index = 0; index < __instance.brushSize * __instance.brushSize; ++index)
              __instance.cursorIndices[index] = -1;
          }
        }
        else if (VFInput._cursorMinusKey.onDown && __instance.brushSize > 1)
        {
          --__instance.brushSize;
          flag2 = true;
        }
        float radius = 0.990946f * (float)__instance.brushSize;
        int flattenTerrainReform = __instance.factory.ComputeFlattenTerrainReform(__instance.cursorPoints, __instance.reformCenterPoint, radius, __instance.cursorPointCount);
        if (__instance.cursorValid && !VFInput.onGUIOperate)
        {
          if (flattenTerrainReform > 0)
            __instance.actionBuild.model.cursorText = "沙土消耗".Translate() + " " + flattenTerrainReform.ToString() + " " + "个沙土".Translate() + "\n" + "改造大小".Translate() + __instance.brushSize.ToString() + "x" + __instance.brushSize.ToString();
          else if (flattenTerrainReform == 0)
          {
            __instance.actionBuild.model.cursorText = "改造大小".Translate() + __instance.brushSize.ToString() + "x" + __instance.brushSize.ToString();
          }
          else
          {
            int num = -flattenTerrainReform;
            __instance.actionBuild.model.cursorText = "沙土获得".Translate() + " " + num.ToString() + " " + "个沙土".Translate() + "\n" + "改造大小".Translate() + __instance.brushSize.ToString() + "x" + __instance.brushSize.ToString();
          }

          FieldInfo fieldLastReformPoint = typeof(BuildTool_Reform).GetField("lastReformPoint", BindingFlags.NonPublic | BindingFlags.Instance);

          if (VFInput._buildConfirm.pressing)
          {
            bool onDown = VFInput._buildConfirm.onDown;
            if (onDown)
              __instance.drawing = true;
            if (__instance.drawing)
            {
              flag1 = true;
              Vector3 lastReformPoint = (Vector3)fieldLastReformPoint.GetValue(__instance);
              if (
                (
                  (
                    (double)lastReformPoint.x != (double)__instance.reformCenterPoint.x || (double)lastReformPoint.y != (double)__instance.reformCenterPoint.y
                      ? 1
                      : (
                        (double)lastReformPoint.z != (double)__instance.reformCenterPoint.z
                          ? 1
                          : 0
                      )
                  ) | (flag2 ? 1 : 0)
                ) != 0 && !VFInput.onGUI
              )
              {
                int newSandCount = __instance.player.sandCount - flattenTerrainReform;
                if (newSandCount >= 0)
                {
                  __instance.factory.FlattenTerrainReform(__instance.reformCenterPoint, radius, __instance.brushSize, __instance.buryVeins);
                  VFAudio.Create("reform-terrain", (Transform)null, __instance.reformCenterPoint, true, 4);
                  __instance.player.SetSandCount(newSandCount);
                  for (int index = 0; index < __instance.brushSize * __instance.brushSize; ++index)
                  {
                    int cursorIndex = __instance.cursorIndices[index];
                    PlatformSystem platformSystem = __instance.factory.platformSystem;
                    if (cursorIndex >= 0)
                    {
                      int reformType = platformSystem.GetReformType(cursorIndex);
                      int reformColor = platformSystem.GetReformColor(cursorIndex);
                      if (reformType != __instance.brushType || reformColor != __instance.brushColor)
                      {
                        __instance.factory.platformSystem.SetReformType(cursorIndex, __instance.brushType);
                        __instance.factory.platformSystem.SetReformColor(cursorIndex, __instance.brushColor);
                      }
                    }
                  }
                  int id1 = __instance.handItem.ID;
                  consumeRegister[id1] += __instance.cursorPointCount;
                  int id2 = __instance.handItem.ID;
                  GameMain.gameScenario.NotifyOnBuild(__instance.planet.id, id2, 0);
                  GameMain.achievementLogic.NotifyOnBuild(__instance.planet.id, id2, 0);
                  if (__instance.cursorPointCount > 0)
                    __instance.gameData.milestoneSystem.milestoneLogic.useFoundationDeterminator.ManualUnlock();
                  GameMain.history.MarkItemBuilt(id2, __instance.cursorPointCount);
                }
                else if (onDown)
                  UIRealtimeTip.Popup("沙土不足".Translate());
              }
              fieldLastReformPoint.SetValue(__instance, __instance.reformCenterPoint);
            }
            else
              fieldLastReformPoint.SetValue(__instance, Vector3.zero);
          }
          else
          {
            __instance.drawing = false;
            fieldLastReformPoint.SetValue(__instance, Vector3.zero);
          }
        }
      }
      if (!flag1)
        __instance.drawing = flag1;
      if (drawing == __instance.drawing)
        return false;
      if (GameMain.gameScenario != null)
        GameMain.gameScenario.NotifyOnDoReformOpt(drawing ? 2 : 1);
      if (GameMain.achievementLogic == null)
        return false;
      GameMain.achievementLogic.NotifyOnDoReformOpt(drawing ? 2 : 1);
      return false;
    }

    private static int maxBrushSize = 20;
  }
}