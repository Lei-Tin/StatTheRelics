using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Kifuda), nameof(Kifuda.AfterObtained))]
    public static class KifudaPatch {
        internal const string TypeName = "MegaCrit.Sts2.Core.Models.Relics.Kifuda";
        const string EnchantedCardsKey = "Cards Enchanted";

        static readonly object Sync = new();
        static Kifuda? activeRelic;

        static void Prefix(Kifuda __instance) {
            lock (Sync) activeRelic = __instance;
        }

        static void Postfix(Kifuda __instance, Task __result) {
            try {
                if (__result == null) {
                    Clear(__instance);
                    return;
                }

                __result.ContinueWith(_ => {
                    try { Clear(__instance); } catch { }
                });
            } catch {
                Clear(__instance);
            }
        }

        static void Clear(Kifuda relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static Kifuda? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        internal static void CountPlayed(CardModel card) {
            try {
                if (card == null) return;
                if (!RelicTracker.HasTrackedRelicType(TypeName)) return;
                if (!IsAdroitAndActive(card)) return;

                var tracked = ParseCardList(RelicTracker.GetTextByType(TypeName, EnchantedCardsKey));
                if (tracked.Count == 0) return;

                var cardName = DeckUtil.GetCardMatchName(card);
                if (string.IsNullOrWhiteSpace(cardName)) return;
                if (!tracked.TryGetValue(cardName, out var copies) || copies <= 0) return;

                RelicTracker.AddAmountByType(TypeName, "Enchanted Cards Played", 1);
            } catch { }
        }

        static bool IsAdroitAndActive(CardModel card) {
            try {
                var enchantment = ReflectionUtil.GetMemberValue(card, "Enchantment");
                if (enchantment == null) return false;
                if (!string.Equals(enchantment.GetType().Name, "Adroit", StringComparison.Ordinal)) return false;

                var status = ReflectionUtil.GetMemberValue(enchantment, "Status");
                return status == null || string.Equals(status.ToString(), "Normal", StringComparison.OrdinalIgnoreCase);
            } catch {
                return false;
            }
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
    public static class KifudaSelectionPatch {
        static void Postfix(Player player, Task<IEnumerable<CardModel>> __result) {
            try {
                var relic = KifudaPatch.ActiveRelic;
                if (relic == null || player == null || relic.Owner != player || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
                        var names = task.Result
                            .Select(card => DeckUtil.GetCardDisplayName(card, preferBaseTitle: true))
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .ToList();
                        if (names.Count <= 0) return;

                        RelicTracker.SetText(relic, "Cards Enchanted", DeckUtil.JoinCardList(names));
                    } catch { }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class KifudaCardPlayPatch {
        static void Postfix(CardModel __instance) {
            KifudaPatch.CountPlayed(__instance);
        }
    }
}
