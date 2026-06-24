using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PaelsTooth), nameof(PaelsTooth.AfterObtained))]
    public static class PaelsToothPatch {
        class State {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(PaelsTooth __instance, ref object __state) {
            try {
                __state = new State {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(PaelsTooth __instance, Task __result, object __state) {
            try {
                if (__state is not State state) return;
                if (__result == null) {
                    CountStored(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) CountStored(__instance, state);
                    } catch { }
                });
            } catch { }
        }

        static void CountStored(PaelsTooth relic, State state) {
            try {
                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var stored = DeckUtil.FindRemovedCards(state.BeforeDeck, after);
                if (stored.Count > 0) RelicTracker.SetText(relic, "Cards Chosen", DeckUtil.JoinCardList(stored));
            } catch { }
        }
    }
}
