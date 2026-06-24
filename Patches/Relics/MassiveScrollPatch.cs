using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(MassiveScroll), nameof(MassiveScroll.AfterObtained))]
    public static class MassiveScrollPatch {
        static readonly object Sync = new();
        static MassiveScroll? activeRelic;

        class PickupState {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(MassiveScroll __instance, ref object __state) {
            try {
                lock (Sync) activeRelic = __instance;
                __state = new PickupState {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(MassiveScroll __instance, Task __result, object __state) {
            try {
                var state = __state as PickupState;
                if (state == null) return;

                if (__result == null) {
                    Count(__instance, state);
                    Clear(__instance);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, state);
                        Clear(__instance);
                    } catch { }
                });
            } catch { }
        }

        static void Clear(MassiveScroll relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static MassiveScroll? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        internal static void SetOfferedCards(MassiveScroll relic, IReadOnlyList<CardModel> cards) {
            try {
                if (relic == null || cards == null || cards.Count <= 0) return;
                var names = new List<string>();
                foreach (var card in cards) {
                    var name = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
                    if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
                }

                if (names.Count > 0) RelicTracker.SetText(relic, "Cards Offered", DeckUtil.JoinCardList(names));
            } catch { }
        }

        static void Count(MassiveScroll relic, PickupState state) {
            try {
                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var added = DeckUtil.FindAddedCards(state.BeforeDeck, after);
                if (added.Count <= 0) return;

                RelicTracker.SetText(relic, "Card Added", DeckUtil.JoinCardList(added));
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromChooseACardScreen), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(IReadOnlyList<CardModel>),
        typeof(Player),
        typeof(bool)
    })]
    public static class MassiveScrollChooseCardPatch {
        static void Prefix(IReadOnlyList<CardModel> cards, Player player) {
            try {
                var relic = MassiveScrollPatch.ActiveRelic;
                if (relic == null || player == null || relic.Owner != player) return;
                MassiveScrollPatch.SetOfferedCards(relic, cards);
            } catch { }
        }
    }
}
