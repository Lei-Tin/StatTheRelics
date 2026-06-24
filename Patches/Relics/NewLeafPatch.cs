using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(NewLeaf), nameof(NewLeaf.AfterObtained))]
    public static class NewLeafPatch {
        class State {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(NewLeaf __instance, ref object __state) {
            try {
                __state = new State {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(NewLeaf __instance, Task __result, object __state) {
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

        static void Count(NewLeaf relic, State state) {
            var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
            var removed = DeckUtil.FindRemovedCards(state.BeforeDeck, after);
            var added = DeckUtil.FindAddedCards(state.BeforeDeck, after);
            RelicTracker.SetText(relic, "Cards Transformed", DeckUtil.JoinCardList(removed));
            RelicTracker.SetText(relic, "Cards Obtained", DeckUtil.JoinCardList(added));
        }
    }
}
