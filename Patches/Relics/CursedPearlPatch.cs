using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(CursedPearl), nameof(CursedPearl.AfterObtained))]
    public static class CursedPearlPatch {
        class CursedPearlState {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(CursedPearl __instance, ref object __state) {
            try {
                __state = new CursedPearlState {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(CursedPearl __instance, Task __result, object __state) {
            try {
                var state = __state as CursedPearlState;
                var gold = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Gold", 333));
                if (__result == null) {
                    Count(__instance, state, gold);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            Count(__instance, state, gold);
                        }
                    } catch { }
                });
            } catch { }
        }

        static void Count(CursedPearl relic, CursedPearlState? state, int gold) {
            try {
                if (gold > 0) RelicTracker.AddAmount(relic, "Gold Gained", gold);

                if (state == null) return;
                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var added = DeckUtil.FindAddedCards(state.BeforeDeck, after);
                var curse = added.Count > 0 ? string.Join("\n", added) : "Unknown";
                RelicTracker.SetText(relic, "Curse", curse);
            } catch { }
        }
    }
}
