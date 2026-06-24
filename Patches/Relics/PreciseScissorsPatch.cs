using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PreciseScissors), nameof(PreciseScissors.AfterObtained))]
    public static class PreciseScissorsPatch {
        class State {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(PreciseScissors __instance, ref object __state) {
            try {
                __state = new State {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(PreciseScissors __instance, Task __result, object __state) {
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

        static void Count(PreciseScissors relic, State state) {
            try {
                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var removed = DeckUtil.FindRemovedCards(state.BeforeDeck, after);
                if (removed.Count <= 0) return;

                RelicTracker.AddAmount(relic, "Cards Removed", removed.Count);
                RelicTracker.SetText(relic, "Cards Removed", DeckUtil.JoinCardList(removed));
            } catch { }
        }
    }
}
