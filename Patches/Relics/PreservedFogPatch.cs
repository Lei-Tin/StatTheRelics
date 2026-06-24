using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PreservedFog), nameof(PreservedFog.AfterObtained))]
    public static class PreservedFogPatch {
        class State {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(PreservedFog __instance, ref object __state) {
            try {
                __state = new State {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(PreservedFog __instance, Task __result, object __state) {
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

        static void Count(PreservedFog relic, State state) {
            try {
                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var removed = DeckUtil.FindRemovedCards(state.BeforeDeck, after);
                if (removed.Count > 0) {
                    RelicTracker.AddAmount(relic, "Cards Removed", removed.Count);
                    RelicTracker.SetText(relic, "Cards Removed", DeckUtil.JoinCardList(removed));
                }

                var added = DeckUtil.FindAddedCards(state.BeforeDeck, after);
                var curse = added.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(curse)) RelicTracker.SetText(relic, "Curse Added", curse);
            } catch { }
        }
    }
}
