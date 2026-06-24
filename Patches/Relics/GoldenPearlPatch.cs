using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(GoldenPearl), nameof(GoldenPearl.AfterObtained))]
    public static class GoldenPearlPatch {
        class GoldState {
            public int Gold { get; set; }
        }

        static void Prefix(GoldenPearl __instance, ref object __state) {
            try {
                __state = new GoldState {
                    Gold = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Gold", 150))
                };
            } catch { }
        }

        static void Postfix(GoldenPearl __instance, Task __result, object __state) {
            try {
                var state = __state as GoldState;
                if (state == null || state.Gold <= 0) return;

                if (__result == null) {
                    Count(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, state);
                    } catch { }
                });
            } catch { }
        }

        static void Count(GoldenPearl relic, GoldState state) {
            RelicTracker.AddAmount(relic, "Gold Gained", state.Gold);
        }
    }
}
