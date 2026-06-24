using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Orrery), nameof(Orrery.AfterObtained))]
    public static class OrreryPatch {
        class PickupState {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(Orrery __instance, ref object __state) {
            try {
                __state = new PickupState {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(Orrery __instance, Task __result, object __state) {
            try {
                if (__state is not PickupState state) return;
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

        static void Count(Orrery relic, PickupState state) {
            try {
                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var added = DeckUtil.FindAddedCards(state.BeforeDeck, after);
                if (added.Count > 0) RelicTracker.SetText(relic, "Cards Added", DeckUtil.JoinCardList(added));
            } catch { }
        }
    }
}
