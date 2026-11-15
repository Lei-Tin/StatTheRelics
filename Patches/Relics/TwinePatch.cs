using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Entities.FoolArrows;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Twine), nameof(Twine.AfterArrowDislodged))]
    public static class TwinePatch {
        static void Prefix(Twine __instance, ArrowSlot arrow, out int __state) {
            __state = 0;
            try {
                if (arrow.owner != __instance.Owner) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Block"));
            } catch { }
        }

        static void Postfix(Twine __instance, Task __result, int __state) {
            try {
                if (__state <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Block Gained", __state);
                    } catch { }
                });
            } catch { }
        }
    }
}
