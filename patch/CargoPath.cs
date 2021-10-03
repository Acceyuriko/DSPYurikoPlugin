using System;
using HarmonyLib;

namespace DSPYurikoPlugin
{
  public class CargoPathPatch
  {
    // 传送带加速
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CargoPath), "Update")]
    public static bool CargoPathUpdatePatch(CargoPath __instance, int ___bufferLength, ref int ___updateLen, int ___chunkCount)
    {
      if (__instance.outputPath != null)
      {
        byte[] numArray = __instance.id > __instance.outputPath.id ? __instance.buffer : __instance.outputPath.buffer;
        lock (__instance.id < __instance.outputPath.id ? __instance.buffer : __instance.outputPath.buffer)
        {
          lock (numArray)
          {
            int index = ___bufferLength - 5 - 1;
            if (__instance.buffer[index] == (byte)250)
            {
              int cargoId = (int)__instance.buffer[index + 1] - 1 + ((int)__instance.buffer[index + 2] - 1) * 100 + ((int)__instance.buffer[index + 3] - 1) * 10000 + ((int)__instance.buffer[index + 4] - 1) * 1000000;
              if (__instance.closed)
              {
                if (__instance.outputPath.TryInsertCargoNoSqueeze(__instance.outputIndex, cargoId))
                {
                  Array.Clear((Array)__instance.buffer, index - 4, 10);
                  ___updateLen = ___bufferLength;
                }
              }
              else if (__instance.outputPath.TryInsertCargo(__instance.outputIndex, cargoId))
              {
                Array.Clear((Array)__instance.buffer, index - 4, 10);
                ___updateLen = ___bufferLength;
              }
            }
          }
        }
      }
      else if (___bufferLength <= 10)
      {
        return false;
      }
      lock (__instance.buffer)
      {
        for (int index = ___updateLen - 1; index >= 0 && __instance.buffer[index] != (byte)0; --index)
          --___updateLen;
        if (___updateLen == 0)
        {
          return false;
        }
        int num1 = ___updateLen;
        for (int index1 = ___chunkCount - 1; index1 >= 0; --index1)
        {
          int index2 = __instance.chunks[index1 * 3];
          int speed = __instance.chunks[index1 * 3 + 2] * 2; // 倍速
          if (index2 < num1)
          {
            if (__instance.buffer[index2] != (byte)0)
            {
              for (int index3 = index2 - 5; index3 < index2 + 4; ++index3)
              {
                if (index3 >= 0 && __instance.buffer[index3] == (byte)250)
                {
                  index2 = index3 >= index2 ? index3 - 4 : index3 + 5 + 1;
                  break;
                }
              }
            }
            int num2 = 0;
          label_41:
            while (num2 < speed)
            {
              int num3 = num1 - index2;
              if (num3 >= 10)
              {
                int length = 0;
                for (int index4 = 0; index4 < speed - num2 && __instance.buffer[num1 - 1 - index4] == (byte)0; ++index4)
                  ++length;
                if (length > 0)
                {
                  Array.Copy((Array)__instance.buffer, index2, (Array)__instance.buffer, index2 + length, num3 - length);
                  Array.Clear((Array)__instance.buffer, index2, length);
                  num2 += length;
                }
                int index5 = num1 - 1;
                while (true)
                {
                  if (index5 >= 0 && __instance.buffer[index5] != (byte)0)
                  {
                    --num1;
                    --index5;
                  }
                  else
                    goto label_41;
                }
              }
              else
                break;
            }
            int num4 = index2 + (num2 == 0 ? 1 : num2);
            if (num1 > num4)
              num1 = num4;
          }
        }
      }
      return false;
    }

  }
}