using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(RainbowRing), nameof(RainbowRing.AfterCardPlayed))]
    public static class RainbowRingPatch {
        class State {
            public int ActivationCount { get; set; }
        }

        static void Prefix(RainbowRing __instance, ref object __state) {
            try {
                __state = new State {
                    ActivationCount = ReflectionUtil.GetIntMemberValue(__instance, "ActivationCountThisTurn")
                };
            } catch { }
        }

        static void Postfix(RainbowRing __instance, Task __result, object __state) {
            try {
                if (__state is not State state) return;
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

        static void Count(RainbowRing relic, State state) {
            try {
                var after = ReflectionUtil.GetIntMemberValue(relic, "ActivationCountThisTurn", state.ActivationCount);
                if (after <= state.ActivationCount) return;

                var activations = after - state.ActivationCount;
                RelicTracker.AddAmount(relic, "Times Activated", activations);
            } catch { }
        }
    }
}
