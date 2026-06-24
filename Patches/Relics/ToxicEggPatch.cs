using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace StatTheRelics.Patches.Relics {
    public static class ToxicEggPatch {
        class RewardState {
            public object? Owner { get; set; }
            public ToxicEgg? Relic { get; set; }
            public HashSet<int> BeforeDeckCards { get; } = new();
            public Dictionary<string, int> ModifiedOffers { get; } = new(StringComparer.Ordinal);
        }

        static readonly object CountLock = new();
        static readonly HashSet<int> CountedCards = new();

        internal static void CountCard(ToxicEgg relic, object card) {
            try {
                var key = RuntimeHelpers.GetHashCode(card);
                lock (CountLock) {
                    if (!CountedCards.Add(key)) return;
                }

                RelicTracker.AddAmount(relic, "Cards Upgraded", 1);
            } catch { }
        }

        internal static object? CaptureRewardState(CardReward reward) {
            try {
                var state = new RewardState {
                    Owner = ReflectionUtil.GetMemberValue(reward, "Player")
                };
                state.Relic = ReflectionUtil.FindRelic<ToxicEgg>(state.Owner);
                if (state.Relic == null) return null;

                foreach (var card in DeckUtil.EnumerateDeckCards(state.Owner)) {
                    state.BeforeDeckCards.Add(RuntimeHelpers.GetHashCode(card));
                }

                foreach (var creationResult in GetCreationResults(reward)) {
                    if (creationResult.ModifyingRelics?.OfType<ToxicEgg>().Any() != true) continue;
                    var name = DeckUtil.GetCardDisplayName(creationResult.Card, preferBaseTitle: true);
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    state.ModifiedOffers[name] = state.ModifiedOffers.TryGetValue(name, out var value) ? value + 1 : 1;
                }

                return state.ModifiedOffers.Count > 0 ? state : null;
            } catch {
                return null;
            }
        }

        internal static void CountRewardState(object? rawState) {
            try {
                if (rawState is not RewardState state || state.Relic == null || state.ModifiedOffers.Count == 0) return;

                foreach (var card in DeckUtil.EnumerateDeckCards(state.Owner)) {
                    var key = RuntimeHelpers.GetHashCode(card);
                    if (state.BeforeDeckCards.Contains(key)) continue;

                    var name = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    if (!state.ModifiedOffers.TryGetValue(name, out var remaining) || remaining <= 0) continue;
                    state.ModifiedOffers[name] = remaining - 1;

                    CountCard(state.Relic, card);
                }
            } catch { }
        }

        static IEnumerable<CardCreationResult> GetCreationResults(CardReward reward) {
            try {
                return ReflectionUtil.GetMemberValue(reward, "_cards") as IEnumerable<CardCreationResult>
                    ?? Array.Empty<CardCreationResult>();
            } catch {
                return Array.Empty<CardCreationResult>();
            }
        }
    }

    [HarmonyPatch(typeof(ToxicEgg), nameof(ToxicEgg.TryModifyCardBeingAddedToDeck))]
    public static class ToxicEggCardBeingAddedPatch {
        static void Postfix(ToxicEgg __instance, bool __result, CardModel newCard) {
            try {
                if (__result && newCard != null) ToxicEggPatch.CountCard(__instance, newCard);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardReward), "OnSelect")]
    public static class ToxicEggCardRewardPatch {
        static void Prefix(CardReward __instance, ref object __state) {
            var state = ToxicEggPatch.CaptureRewardState(__instance);
            if (state != null) __state = state;
        }

        static void Postfix(Task<bool> __result, object __state) {
            try {
                if (__state == null) return;
                if (__result == null) {
                    ToxicEggPatch.CountRewardState(__state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion && task.Result) ToxicEggPatch.CountRewardState(__state);
                    } catch { }
                });
            } catch { }
        }
    }
}
