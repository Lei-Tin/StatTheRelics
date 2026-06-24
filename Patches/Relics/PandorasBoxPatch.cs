using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PandorasBox), nameof(PandorasBox.AfterObtained))]
    public static class PandorasBoxPatch {
        class State {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(PandorasBox __instance, ref object __state) {
            try {
                __state = new State {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(PandorasBox __instance, Task __result, object __state) {
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

        static void Count(PandorasBox relic, State state) {
            try {
                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var obtained = DeckUtil.FindAddedCards(state.BeforeDeck, after);

                if (obtained.Count > 0) RelicTracker.SetText(relic, "Cards Obtained", DeckUtil.JoinCardList(obtained));
            } catch { }
        }
    }
}
