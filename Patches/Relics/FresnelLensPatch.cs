using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(CardReward), "OnSelect")]
    public static class FresnelLensPatch {
        class RewardState {
            public object? Owner { get; set; }
            public FresnelLens? Relic { get; set; }
            public HashSet<int> BeforeDeckCards { get; } = new();
            public Dictionary<string, int> ModifiedOffers { get; } = new(StringComparer.Ordinal);
        }

        static readonly object CountLock = new();
        static readonly HashSet<int> CountedCards = new();

        static void Prefix(CardReward __instance, ref object __state) {
            try {
                var state = new RewardState {
                    Owner = ReflectionUtil.GetMemberValue(__instance, "Player")
                };

                state.Relic = ReflectionUtil.FindRelic<FresnelLens>(state.Owner);
                if (state.Relic == null) return;

                foreach (var card in DeckUtil.EnumerateDeckCards(state.Owner)) {
                    state.BeforeDeckCards.Add(RuntimeHelpers.GetHashCode(card));
                }

                foreach (var creationResult in GetCreationResults(__instance)) {
                    if (!WasModifiedByFresnelLens(creationResult)) continue;
                    var name = DeckUtil.GetCardDisplayName(creationResult.Card, preferBaseTitle: true);
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    state.ModifiedOffers[name] = state.ModifiedOffers.TryGetValue(name, out var value) ? value + 1 : 1;
                }

                if (state.ModifiedOffers.Count > 0) __state = state;
            } catch { }
        }

        static void Postfix(Task<bool> __result, object __state) {
            try {
                var state = __state as RewardState;
                if (state?.Relic == null || state.ModifiedOffers.Count == 0) return;

                if (__result == null) {
                    CountSelectedCards(state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion && task.Result) {
                            CountSelectedCards(state);
                        }
                    } catch { }
                });
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

        static bool WasModifiedByFresnelLens(CardCreationResult creationResult) {
            try {
                return creationResult.ModifyingRelics?.OfType<FresnelLens>().Any() == true;
            } catch {
                return false;
            }
        }

        static void CountSelectedCards(RewardState state) {
            try {
                foreach (var card in DeckUtil.EnumerateDeckCards(state.Owner)) {
                    var cardKey = RuntimeHelpers.GetHashCode(card);
                    if (state.BeforeDeckCards.Contains(cardKey)) continue;

                    var name = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    if (!state.ModifiedOffers.TryGetValue(name, out var remaining) || remaining <= 0) continue;

                    lock (CountLock) {
                        if (!CountedCards.Add(cardKey)) continue;
                        state.ModifiedOffers[name] = remaining - 1;
                    }

                    RelicTracker.AddAmount(state.Relic!, "Cards Enchanted", 1);
                    ModLog.Info($"FresnelLensPatch: counted selected card reward {name}");
                }
            } catch { }
        }
    }
}
