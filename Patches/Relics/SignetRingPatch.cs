using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(SignetRing), nameof(SignetRing.AfterObtained))]
    public static class SignetRingPatch {
        static void Prefix(SignetRing __instance, ref object __state) {
            try {
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Gold", 999));
            } catch { }
        }

        static void Postfix(SignetRing __instance, Task __result, object __state) {
            try {
                if (__state is not int gold || gold <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Gold Gained", gold);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Gold Gained", gold);
                    } catch { }
                });
            } catch { }
        }
    }
}
