using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace StatTheRelics.Patches.Relics {
    public static class SilverCruciblePatch {
        sealed class RewardState {
            public object? Owner { get; set; }
            public SilverCrucible? Relic { get; set; }
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
            public Dictionary<string, int> ModifiedOffers { get; } = new(StringComparer.Ordinal);
        }

        internal static object? CaptureRewardState(CardReward reward) {
            try {
                var state = new RewardState {
                    Owner = ReflectionUtil.GetMemberValue(reward, "Player")
                };
                state.Relic = ReflectionUtil.FindRelic<SilverCrucible>(state.Owner);
                if (state.Relic == null) return null;

                state.BeforeDeck = DeckUtil.CaptureDeckHistogramFromOwner(state.Owner, preferBaseTitle: true);
                foreach (var creationResult in GetCreationResults(reward)) {
                    if (creationResult.ModifyingRelics?.OfType<SilverCrucible>().Any() != true) continue;
                    var card = creationResult.Card;
                    if (card == null) continue;
                    var name = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
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

                var after = DeckUtil.CaptureDeckHistogramFromOwner(state.Owner, preferBaseTitle: true);
                var added = DeckUtil.FindAddedCards(state.BeforeDeck, after);
                foreach (var name in added) {
                    if (!state.ModifiedOffers.TryGetValue(name, out var remaining) || remaining <= 0) continue;
                    state.ModifiedOffers[name] = remaining - 1;
                    AppendText(state.Relic, "Cards Added", name);
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

        internal static void AppendText(SilverCrucible relic, string key, string value) {
            if (string.IsNullOrWhiteSpace(value)) return;
            var current = RelicTracker.GetText(relic, key);
            RelicTracker.SetText(relic, key, string.IsNullOrWhiteSpace(current) ? value : current + "\n" + value);
        }
    }

    [HarmonyPatch(typeof(CardReward), "OnSelect")]
    public static class SilverCrucibleCardRewardPatch {
        static void Prefix(CardReward __instance, ref object __state) {
            var state = SilverCruciblePatch.CaptureRewardState(__instance);
            if (state != null) __state = state;
        }

        static void Postfix(Task<bool> __result, object __state) {
            try {
                if (__state == null) return;
                if (__result == null) {
                    SilverCruciblePatch.CountRewardState(__state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion && task.Result) SilverCruciblePatch.CountRewardState(__state);
                    } catch { }
                });
            } catch { }
        }
    }
}
