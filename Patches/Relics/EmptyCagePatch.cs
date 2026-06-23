using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(EmptyCage), nameof(EmptyCage.AfterObtained))]
    public static class EmptyCagePatch {
        class EmptyCageState {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(EmptyCage __instance, ref object __state) {
            try {
                __state = new EmptyCageState {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(EmptyCage __instance, Task __result, object __state) {
            try {
                var state = __state as EmptyCageState;
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

        static void Count(EmptyCage relic, EmptyCageState? state) {
            try {
                if (state == null) return;
                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var removed = DeckUtil.FindRemovedCards(state.BeforeDeck, after);
                if (removed.Count <= 0) return;

                RelicTracker.AddAmount(relic, "Cards Removed", removed.Count);
                RelicTracker.SetText(relic, "Cards Removed", DeckUtil.JoinCardList(removed));
            } catch { }
        }
    }
}
