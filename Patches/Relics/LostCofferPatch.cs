using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(LostCoffer), nameof(LostCoffer.AfterObtained))]
    public static class LostCofferPatch {
        static readonly object Sync = new();
        static LostCoffer? activeRelic;

        class PickupState {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(LostCoffer __instance, ref object __state) {
            try {
                lock (Sync) activeRelic = __instance;
                __state = new PickupState {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(LostCoffer __instance, Task __result, object __state) {
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

        static void Clear(LostCoffer relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static LostCoffer? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        internal static void SetOfferedRewards(LostCoffer relic, IEnumerable<Reward> rewards) {
            try {
                if (relic == null || rewards == null) return;
                var names = new List<string>();
                foreach (var reward in rewards.OfType<CardReward>()) {
                    names.AddRange(GetCardNames(reward));
                }

                if (names.Count > 0) RelicTracker.SetText(relic, "Cards Offered", DeckUtil.JoinCardList(names));
            } catch { }
        }

        internal static void SetOfferedCardReward(LostCoffer relic, CardReward reward) {
            try {
                if (relic == null || reward == null) return;
                var names = GetCardNames(reward);
                if (names.Count > 0) RelicTracker.SetText(relic, "Cards Offered", DeckUtil.JoinCardList(names));
            } catch { }
        }

        static void Count(LostCoffer relic, PickupState state) {
            try {
                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var added = DeckUtil.FindAddedCards(state.BeforeDeck, after);
                if (added.Count <= 0) return;

                RelicTracker.SetText(relic, "Card Added", DeckUtil.JoinCardList(added));
            } catch { }
        }

        static List<string> GetCardNames(CardReward reward) {
            var names = new List<string>();
            try {
                foreach (var result in GetCreationResults(reward)) {
                    var name = DeckUtil.GetCardDisplayName(result.Card, preferBaseTitle: true);
                    if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
                }
            } catch { }

            return names;
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

    [HarmonyPatch(typeof(RewardsCmd), nameof(RewardsCmd.OfferCustom), new Type[] {
        typeof(Player),
        typeof(List<Reward>)
    })]
    public static class LostCofferOfferCustomPatch {
        static void Prefix(Player player, List<Reward> rewards) {
            try {
                var relic = LostCofferPatch.ActiveRelic;
                if (relic == null || player == null || relic.Owner != player) return;

                LostCofferPatch.SetOfferedRewards(relic, rewards);
                foreach (var reward in rewards.OfType<CardReward>()) {
                    LostCofferCardRewardPatch.Track(reward, relic);
                }

                foreach (var reward in rewards.OfType<PotionReward>()) {
                    LostCofferPotionRewardPatch.Track(reward, relic);
                }
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardReward), nameof(CardReward.Populate))]
    public static class LostCofferCardRewardPatch {
        static readonly ConditionalWeakTable<CardReward, LostCofferRef> RewardSources = new();

        class LostCofferRef {
            public LostCoffer? Relic { get; set; }
        }

        internal static void Track(CardReward reward, LostCoffer relic) {
            try {
                RewardSources.Remove(reward);
                RewardSources.Add(reward, new LostCofferRef { Relic = relic });
            } catch { }
        }

        static void Postfix(CardReward __instance) {
            try {
                if (__instance == null) return;
                if (!RewardSources.TryGetValue(__instance, out var source)) return;
                if (source.Relic == null) return;

                LostCofferPatch.SetOfferedCardReward(source.Relic, __instance);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(PotionReward), nameof(PotionReward.Populate))]
    public static class LostCofferPotionRewardPatch {
        static readonly ConditionalWeakTable<PotionReward, LostCofferRef> RewardSources = new();

        class LostCofferRef {
            public LostCoffer? Relic { get; set; }
        }

        internal static void Track(PotionReward reward, LostCoffer relic) {
            try {
                RewardSources.Remove(reward);
                RewardSources.Add(reward, new LostCofferRef { Relic = relic });
            } catch { }
        }

        static void Postfix(PotionReward __instance) {
            try {
                if (__instance == null) return;
                if (!RewardSources.TryGetValue(__instance, out var source)) return;
                if (source.Relic == null || __instance.Potion == null) return;

                LostCofferPotionProcurePatch.Track(__instance.Potion, source.Relic);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(PotionCmd), nameof(PotionCmd.TryToProcure), new Type[] {
        typeof(PotionModel),
        typeof(Player),
        typeof(int)
    })]
    public static class LostCofferPotionProcurePatch {
        static readonly ConditionalWeakTable<PotionModel, LostCofferRef> PotionSources = new();

        class LostCofferRef {
            public LostCoffer? Relic { get; set; }
        }

        class PotionState {
            public LostCoffer? Relic { get; set; }
        }

        internal static void Track(PotionModel potion, LostCoffer relic) {
            try {
                PotionSources.Remove(potion);
                PotionSources.Add(potion, new LostCofferRef { Relic = relic });
            } catch { }
        }

        static void Prefix(PotionModel potion, Player player, ref object __state) {
            try {
                if (potion == null) return;
                if (!PotionSources.TryGetValue(potion, out var source)) return;
                if (source.Relic == null || player != source.Relic.Owner) return;

                __state = new PotionState { Relic = source.Relic };
            } catch { }
        }

        static void Postfix(PotionModel potion, Task<PotionProcureResult> __result, object __state) {
            try {
                var state = __state as PotionState;
                if (potion != null) PotionSources.Remove(potion);
                if (state?.Relic == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
                        if (!task.Result.success) return;

                        RelicTracker.SetText(state.Relic, "Potion Obtained", PotionNameUtil.GetPotionName(task.Result.potion));
                    } catch { }
                });
            } catch { }
        }
    }
}
