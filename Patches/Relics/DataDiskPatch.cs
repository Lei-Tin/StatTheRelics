using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(DataDisk), nameof(DataDisk.AfterRoomEntered))]
    public static class DataDiskPatch {
        static void Prefix(DataDisk __instance, AbstractRoom room, ref int __state) {
            try {
                if (__instance == null || room is not CombatRoom) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "FocusPower", 1));
            } catch { }
        }

        static void Postfix(DataDisk __instance, Task __result, int __state) {
            try {
                if (__state <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Focus Gained", __state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            RelicTracker.AddAmount(__instance, "Focus Gained", __state);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
