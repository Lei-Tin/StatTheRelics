using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(IntimidatingHelmet), nameof(IntimidatingHelmet.BeforeCardPlayed))]
    public static class IntimidatingHelmetPatch {
        class HelmetState {
            public int Block { get; set; }
        }

        static void Prefix(IntimidatingHelmet __instance, CardPlay cardPlay, ref object __state) {
            try {
                if (cardPlay == null) return;
                var card = cardPlay?.Card;
                if (__instance == null || card == null || card.Owner != __instance.Owner) return;
                if (cardPlay!.Resources.EnergyValue < ReflectionUtil.GetDynamicVarIntValue(__instance, "Energy", 2)) return;
                __state = new HelmetState {
                    Block = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Block", 4))
                };
            } catch { }
        }

        static void Postfix(IntimidatingHelmet __instance, Task __result, object __state) {
            try {
                var state = __state as HelmetState;
                if (state == null || state.Block <= 0) return;

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

        static void Count(IntimidatingHelmet relic, HelmetState state) {
            RelicTracker.AddAmount(relic, "Block Gained", state.Block);
        }
    }
}
