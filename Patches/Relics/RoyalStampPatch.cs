using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(RoyalStamp), nameof(RoyalStamp.AfterObtained))]
    public static class RoyalStampPatch {
        internal const string TypeName = "MegaCrit.Sts2.Core.Models.Relics.RoyalStamp";
        const string EnchantedCardsKey = "Cards Enchanted";

        static readonly object Sync = new();
        static RoyalStamp? activeRelic;
        static readonly HashSet<int> countedCards = new();

        static void Prefix(RoyalStamp __instance) {
            try {
                lock (Sync) {
                    activeRelic = __instance;
                    countedCards.Clear();
                }
            } catch { }
        }

        static void Postfix(RoyalStamp __instance, Task __result) {
            try {
                if (__result == null) {
                    Clear(__instance);
                    return;
                }

                __result.ContinueWith(_ => {
                    try { Clear(__instance); } catch { }
                });
            } catch { }
        }

        static void Clear(RoyalStamp relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
                countedCards.Clear();
            }
        }

        internal static RoyalStamp? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        internal static void CountCard(RoyalStamp relic, CardModel card) {
            try {
                if (relic == null || card == null) return;
                lock (Sync) {
                    if (!countedCards.Add(card.GetHashCode())) return;
                }

                var name = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
                var current = RelicTracker.GetText(relic, EnchantedCardsKey);
                RelicTracker.SetText(relic, EnchantedCardsKey, string.IsNullOrWhiteSpace(current) ? name : current + "\n" + name);
            } catch { }
        }

        internal static void CountPlayed(CardModel card) {
            try {
                if (card == null) return;
                if (!RelicTracker.HasTrackedRelicType(TypeName)) return;
                var tracked = ParseCardList(RelicTracker.GetTextByType(TypeName, EnchantedCardsKey));
                if (tracked.Count == 0) return;

                var cardName = DeckUtil.GetCardMatchName(card);
                if (string.IsNullOrWhiteSpace(cardName)) return;
                if (!tracked.TryGetValue(cardName, out var copies) || copies <= 0) return;

                RelicTracker.AddAmountByType(TypeName, "Enchanted Cards Played", 1);
            } catch { }
        }

        static Dictionary<string, int> ParseCardList(string? raw) {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            if (string.IsNullOrWhiteSpace(raw)) return result;

            var lines = raw.Replace("\r", string.Empty).Split('\n');
            foreach (var line in lines) {
                var name = DeckUtil.NormalizeCardNameForMatching(line);
                if (string.IsNullOrWhiteSpace(name) || string.Equals(name, "None", StringComparison.OrdinalIgnoreCase)) continue;
                result[name] = result.TryGetValue(name, out var current) ? current + 1 : 1;
            }

            return result;
        }
    }

    [HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromDeckForEnchantment), new Type[] {
        typeof(Player),
        typeof(EnchantmentModel),
        typeof(int),
        typeof(CardSelectorPrefs)
    })]
    public static class RoyalStampSelectionPatch {
        static void Postfix(Player player, Task<IEnumerable<CardModel>> __result) {
            try {
                var relic = RoyalStampPatch.ActiveRelic;
                if (relic == null || player == null || relic.Owner != player || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
                        var card = task.Result.FirstOrDefault();
                        if (card == null) return;
                        RoyalStampPatch.CountCard(relic, card);
                    } catch { }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromDeckForEnchantment), new Type[] {
        typeof(IReadOnlyList<CardModel>),
        typeof(EnchantmentModel),
        typeof(int),
        typeof(CardSelectorPrefs)
    })]
    public static class RoyalStampDeckSelectionPatch {
        static void Postfix(Task<IEnumerable<CardModel>> __result) {
            try {
                var relic = RoyalStampPatch.ActiveRelic;
                if (relic == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
                        foreach (var card in task.Result) {
                            if (card == null) continue;
                            RoyalStampPatch.CountCard(relic, card);
                        }
                    } catch { }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class RoyalStampCardPlayPatch {
        static void Postfix(CardModel __instance) {
            RoyalStampPatch.CountPlayed(__instance);
        }
    }
}
