using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(DustyTome), nameof(DustyTome.AfterObtained))]
    public static class DustyTomePatch {
        class DustyTomeState {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(DustyTome __instance, ref object __state) {
            try {
                __state = new DustyTomeState {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(DustyTome __instance, Task __result, object __state) {
            try {
                var state = __state as DustyTomeState;
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

        static void Count(DustyTome relic, DustyTomeState? state) {
            try {
                if (state == null) return;
                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var added = DeckUtil.FindAddedCards(state.BeforeDeck, after);
                if (added.Count <= 0) return;

                RelicTracker.SetText(relic, "Card Obtained", DeckUtil.JoinCardList(added));
            } catch { }
        }
    }
}
