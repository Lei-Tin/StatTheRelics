using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(SeaGlass), nameof(SeaGlass.AfterObtained))]
    public static class SeaGlassPatch {
        static readonly object Sync = new();
        static SeaGlass? activeRelic;

        class State {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(SeaGlass __instance, ref object __state) {
            try {
                lock (Sync) activeRelic = __instance;
                __state = new State {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(SeaGlass __instance, Task __result, object __state) {
            try {
                var state = __state as State;
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

        static void Clear(SeaGlass relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static SeaGlass? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        internal static void SetOfferedCards(SeaGlass relic, IEnumerable<CardCreationResult> rewards) {
            try {
                var names = rewards
                    .Select(result => ReflectionUtil.GetMemberValue(result, "Card"))
                    .Where(card => card != null)
                    .Select(card => DeckUtil.GetCardDisplayName(card!, preferBaseTitle: true))
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList();
                if (names.Count > 0) RelicTracker.SetText(relic, "Cards Offered", DeckUtil.JoinCardList(names));
            } catch { }
        }

        static void Count(SeaGlass relic, State state) {
            try {
                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var added = DeckUtil.FindAddedCards(state.BeforeDeck, after);
                if (added.Count > 0) RelicTracker.SetText(relic, "Cards Added", DeckUtil.JoinCardList(added));
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromSimpleGridForRewards), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(List<CardCreationResult>),
        typeof(Player),
        typeof(CardSelectorPrefs)
    })]
    public static class SeaGlassSimpleGridPatch {
        static void Prefix(List<CardCreationResult> cards, Player player) {
            try {
                var relic = SeaGlassPatch.ActiveRelic;
                if (relic == null || player == null || relic.Owner != player || cards == null) return;
                SeaGlassPatch.SetOfferedCards(relic, cards);
            } catch { }
        }
    }
}
