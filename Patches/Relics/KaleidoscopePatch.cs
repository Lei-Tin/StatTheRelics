using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Kaleidoscope), nameof(Kaleidoscope.AfterObtained))]
    public static class KaleidoscopePatch {
        static readonly object Sync = new();
        static Kaleidoscope? activeRelic;

        class PickupState {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(Kaleidoscope __instance, ref object __state) {
            try {
                lock (Sync) activeRelic = __instance;
                __state = new PickupState {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(Kaleidoscope __instance, Task __result, object __state) {
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

        static void Clear(Kaleidoscope relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static Kaleidoscope? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        internal static void SetOfferedCards(Kaleidoscope relic, IEnumerable<Reward> rewards) {
            try {
                if (relic == null || rewards == null) return;
                var names = new List<string>();
                foreach (var reward in rewards.OfType<CardReward>()) {
                    foreach (var result in GetCreationResults(reward)) {
                        var name = DeckUtil.GetCardDisplayName(result.Card, preferBaseTitle: true);
                        if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
                    }
                }

                if (names.Count > 0) RelicTracker.SetText(relic, "Cards Offered", DeckUtil.JoinCardList(names));
            } catch { }
        }

        static void Count(Kaleidoscope relic, PickupState state) {
            try {
                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var added = DeckUtil.FindAddedCards(state.BeforeDeck, after);
                if (added.Count <= 0) return;

                RelicTracker.SetText(relic, "Cards Added", DeckUtil.JoinCardList(added));
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

    [HarmonyPatch(typeof(RewardsCmd), nameof(RewardsCmd.OfferCustom), new Type[] {
        typeof(Player),
        typeof(List<Reward>)
    })]
    public static class KaleidoscopeOfferCustomPatch {
        static void Prefix(Player player, List<Reward> rewards) {
            try {
                var relic = KaleidoscopePatch.ActiveRelic;
                if (relic == null || player == null || relic.Owner != player) return;
                KaleidoscopePatch.SetOfferedCards(relic, rewards);
            } catch { }
        }
    }
}
