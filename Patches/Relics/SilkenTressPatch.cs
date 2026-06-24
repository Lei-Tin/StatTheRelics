using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(SilkenTress), nameof(SilkenTress.AfterObtained))]
    public static class SilkenTressPatch {
        internal const string TypeName = "MegaCrit.Sts2.Core.Models.Relics.SilkenTress";

        static void Prefix(SilkenTress __instance, ref object __state) {
            try {
                __state = Math.Max(0, __instance?.Owner?.Gold ?? 0);
            } catch { }
        }

        static void Postfix(SilkenTress __instance, Task __result, object __state) {
            try {
                if (__state is not int gold || gold <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Gold Lost", gold);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Gold Lost", gold);
                    } catch { }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(SilkenTress), nameof(SilkenTress.TryModifyCardRewardOptionsLate))]
    public static class SilkenTressCardRewardTracker {
        sealed class OfferState {
            public object? Owner { get; set; }
            public SilkenTress? Relic { get; set; }
            public Dictionary<string, int> ModifiedOffers { get; } = new(StringComparer.Ordinal);
            public List<string> OfferNames { get; } = new();
        }

        sealed class SelectionState {
            public object? Owner { get; set; }
            public SilkenTress? Relic { get; set; }
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
            public Dictionary<string, int> ModifiedOffers { get; set; } = new(StringComparer.Ordinal);
        }

        static readonly object Sync = new();
        static readonly ConditionalWeakTable<List<CardCreationResult>, OfferState> OffersByCardList = new();

        static void Postfix(SilkenTress __instance, Player player, List<CardCreationResult> cardRewards, CardCreationOptions options, bool __result) {
            try {
                if (!__result || __instance == null || player == null || __instance.Owner != player || cardRewards == null) return;

                var state = new OfferState {
                    Owner = player,
                    Relic = __instance
                };

                foreach (var creationResult in cardRewards) {
                    if (creationResult.ModifyingRelics?.OfType<SilkenTress>().Any() != true) continue;
                    var card = creationResult.Card;
                    if (card == null) continue;
                    var name = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    state.ModifiedOffers[name] = state.ModifiedOffers.TryGetValue(name, out var value) ? value + 1 : 1;
                    state.OfferNames.Add(name);
                }

                if (state.OfferNames.Count == 0) return;
                RelicTracker.SetText(__instance, "Card Rewards", DeckUtil.JoinCardList(state.OfferNames));

                lock (Sync) {
                    OffersByCardList.Remove(cardRewards);
                    OffersByCardList.Add(cardRewards, state);
                }
            } catch { }
        }

        internal static object? CaptureSelectionState(CardReward reward) {
            try {
                var cards = ReflectionUtil.GetMemberValue(reward, "_cards") as List<CardCreationResult>;
                if (cards == null) return null;

                OfferState offerState;
                lock (Sync) {
                    if (!OffersByCardList.TryGetValue(cards, out offerState!)) return null;
                }

                if (offerState.Relic == null || offerState.ModifiedOffers.Count == 0) return null;

                return new SelectionState {
                    Owner = offerState.Owner,
                    Relic = offerState.Relic,
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromOwner(offerState.Owner, preferBaseTitle: true),
                    ModifiedOffers = new Dictionary<string, int>(offerState.ModifiedOffers, StringComparer.Ordinal)
                };
            } catch {
                return null;
            }
        }

        internal static void CountSelectionState(object? rawState) {
            try {
                if (rawState is not SelectionState state || state.Relic == null || state.ModifiedOffers.Count == 0) return;

                var after = DeckUtil.CaptureDeckHistogramFromOwner(state.Owner, preferBaseTitle: true);
                var added = DeckUtil.FindAddedCards(state.BeforeDeck, after);
                var selected = new List<string>();

                foreach (var name in added) {
                    if (!state.ModifiedOffers.TryGetValue(name, out var remaining) || remaining <= 0) continue;
                    state.ModifiedOffers[name] = remaining - 1;
                    selected.Add(name);
                }

                if (selected.Count == 0) return;
                foreach (var name in selected) AppendText(state.Relic, "Card Selected", name);
            } catch { }
        }

        internal static void AppendText(SilkenTress relic, string key, string value) {
            if (string.IsNullOrWhiteSpace(value)) return;
            lock (Sync) {
                var current = RelicTracker.GetTextByType(SilkenTressPatch.TypeName, key);
                RelicTracker.SetTextByType(SilkenTressPatch.TypeName, key, string.IsNullOrWhiteSpace(current) ? value : current + "\n" + value);
            }
        }
    }

    [HarmonyPatch(typeof(CardReward), "OnSelect")]
    public static class SilkenTressCardRewardPatch {
        static void Prefix(CardReward __instance, ref object __state) {
            var state = SilkenTressCardRewardTracker.CaptureSelectionState(__instance);
            if (state != null) __state = state;
        }

        static void Postfix(Task<bool> __result, object __state) {
            try {
                if (__state == null) return;
                if (__result == null) {
                    SilkenTressCardRewardTracker.CountSelectionState(__state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion && task.Result) SilkenTressCardRewardTracker.CountSelectionState(__state);
                    } catch { }
                });
            } catch { }
        }
    }
}
