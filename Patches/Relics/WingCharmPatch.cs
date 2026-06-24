using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(WingCharm), nameof(WingCharm.TryModifyCardRewardOptionsLate))]
    public static class WingCharmPatch {
        sealed class RewardState {
            public object? Owner { get; set; }
            public WingCharm? Relic { get; set; }
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
            public Dictionary<string, int> ModifiedOffers { get; } = new(StringComparer.Ordinal);
        }

        static void Postfix(WingCharm __instance, Player player, List<CardCreationResult> cardRewards, CardCreationOptions options, bool __result) {
            try {
                _ = cardRewards;
                _ = options;
                _ = __result;
                _ = __instance;
                _ = player;
            } catch { }
        }

        internal static object? CaptureRewardState(CardReward reward) {
            try {
                var state = new RewardState {
                    Owner = ReflectionUtil.GetMemberValue(reward, "Player")
                };
                state.Relic = ReflectionUtil.FindRelic<WingCharm>(state.Owner);
                if (state.Relic == null) return null;

                state.BeforeDeck = DeckUtil.CaptureDeckHistogramFromOwner(state.Owner, preferBaseTitle: true);
                foreach (var name in GetModifiedOfferNames(GetCreationResults(reward))) {
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
                    RelicTracker.AddAmount(state.Relic, "Cards Enchanted", 1);
                    break;
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

        static List<string> GetModifiedOfferNames(IEnumerable<CardCreationResult> creationResults) {
            var names = new List<string>();
            try {
                foreach (var creationResult in creationResults) {
                    if (creationResult.ModifyingRelics?.OfType<WingCharm>().Any() != true) continue;
                    var name = DeckUtil.GetCardDisplayName(creationResult.Card, preferBaseTitle: true);
                    if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
                }
            } catch { }

            return names;
        }
    }

    [HarmonyPatch(typeof(CardReward), "OnSelect")]
    public static class WingCharmCardRewardPatch {
        static void Prefix(CardReward __instance, ref object __state) {
            var state = WingCharmPatch.CaptureRewardState(__instance);
            if (state != null) __state = state;
        }

        static void Postfix(Task<bool> __result, object __state) {
            try {
                if (__state == null) return;
                if (__result == null) {
                    WingCharmPatch.CountRewardState(__state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion && task.Result) WingCharmPatch.CountRewardState(__state);
                    } catch { }
                });
            } catch { }
        }
    }
}
