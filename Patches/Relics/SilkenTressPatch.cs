using System;
using System.Collections.Generic;
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
        sealed class RewardState {
            public object? Owner { get; set; }
            public SilkenTress? Relic { get; set; }
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
            public Dictionary<string, int> ModifiedOffers { get; } = new(StringComparer.Ordinal);
            public List<string> OfferNames { get; } = new();
        }

        static readonly object Sync = new();

        static void Postfix(SilkenTress __instance, Player player, List<CardCreationResult> cardRewards, CardCreationOptions options, bool __result) {
            try {
                if (!__result || __instance == null || player == null || __instance.Owner != player || cardRewards == null) return;

                var names = GetModifiedOfferNames(cardRewards);
                if (names.Count > 0) RelicTracker.SetText(__instance, "Card Rewards", DeckUtil.JoinCardList(names));
            } catch { }
        }

        internal static object? CaptureRewardState(CardReward reward) {
            try {
                var state = new RewardState {
                    Owner = ReflectionUtil.GetMemberValue(reward, "Player")
                };
                state.Relic = ReflectionUtil.FindRelic<SilkenTress>(state.Owner);
                if (state.Relic == null) return null;

                state.BeforeDeck = DeckUtil.CaptureDeckHistogramFromOwner(state.Owner, preferBaseTitle: true);
                foreach (var name in GetModifiedOfferNames(GetCreationResults(reward))) {
                    state.ModifiedOffers[name] = state.ModifiedOffers.TryGetValue(name, out var value) ? value + 1 : 1;
                    state.OfferNames.Add(name);
                }

                if (state.OfferNames.Count > 0) RelicTracker.SetTextByType(SilkenTressPatch.TypeName, "Card Rewards", DeckUtil.JoinCardList(state.OfferNames));
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
                    AppendText(state.Relic, "Card Selected", name);
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
                    if (creationResult.ModifyingRelics?.OfType<SilkenTress>().Any() != true) continue;
                    var card = creationResult.Card;
                    if (card == null) continue;
                    var name = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
                    if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
                }
            } catch { }

            return names;
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
            var state = SilkenTressCardRewardTracker.CaptureRewardState(__instance);
            if (state != null) __state = state;
        }

        static void Postfix(Task<bool> __result, object __state) {
            try {
                if (__state == null) return;
                if (__result == null) {
                    SilkenTressCardRewardTracker.CountRewardState(__state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion && task.Result) SilkenTressCardRewardTracker.CountRewardState(__state);
                    } catch { }
                });
            } catch { }
        }
    }
}
