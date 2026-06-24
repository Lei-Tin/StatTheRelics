using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PotionBelt), nameof(PotionBelt.AfterObtained))]
    public static class PotionBeltPatch {
        static void Prefix(PotionBelt __instance, ref object __state) {
            try {
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "PotionSlots", 2));
            } catch { }
        }

        static void Postfix(PotionBelt __instance, Task __result, object __state) {
            try {
                if (__state is not int slots || slots <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Potion Slots Gained", slots);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Potion Slots Gained", slots);
                    } catch { }
                });
            } catch { }
        }
    }
}
