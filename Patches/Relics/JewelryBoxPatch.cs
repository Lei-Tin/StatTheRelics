using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(JewelryBox), nameof(JewelryBox.AfterObtained))]
    public static class JewelryBoxPatch {
        class PickupState {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(JewelryBox __instance, ref object __state) {
            try {
                __state = new PickupState {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(JewelryBox __instance, Task __result, object __state) {
            try {
                var state = __state as PickupState;
                if (state == null) return;

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

        static void Count(JewelryBox relic, PickupState state) {
            try {
                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var added = DeckUtil.FindAddedCards(state.BeforeDeck, after);
                if (added.Count <= 0) return;

                RelicTracker.SetText(relic, "Apotheosis Card", DeckUtil.NormalizeCardNameForMatching(added[0]));
            } catch { }
        }

        internal static void CountApotheosisPlayed(CardModel card) {
            try {
                if (card == null) return;
                if (!RelicTracker.HasTrackedRelicType("MegaCrit.Sts2.Core.Models.Relics.JewelryBox")) return;

                var trackedName = RelicTracker.GetTextByType("MegaCrit.Sts2.Core.Models.Relics.JewelryBox", "Apotheosis Card");
                if (string.IsNullOrWhiteSpace(trackedName)) return;

                var cardName = DeckUtil.GetCardMatchName(card);
                if (!string.Equals(cardName, trackedName, StringComparison.Ordinal)) return;

                RelicTracker.AddAmountByType("MegaCrit.Sts2.Core.Models.Relics.JewelryBox", "Apotheosis Played", 1);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class JewelryBoxCardPlayPatch {
        static void Postfix(CardModel __instance) {
            JewelryBoxPatch.CountApotheosisPlayed(__instance);
        }
    }
}
