using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PollinousCore), nameof(PollinousCore.AfterModifyingHandDraw))]
    public static class PollinousCorePatch {
        static void Postfix(PollinousCore __instance, Task __result) {
            try {
                var cards = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Cards", 2));
                if (cards <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Cards Drawn", cards);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Cards Drawn", cards);
                    } catch { }
                });
            } catch { }
        }
    }
}
