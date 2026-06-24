using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(GnarledHammer), nameof(GnarledHammer.AfterObtained))]
    public static class GnarledHammerPatch {
        internal const string GnarledHammerTypeName = "MegaCrit.Sts2.Core.Models.Relics.GnarledHammer";
        const string EnchantedCardsKey = "Enchanted Cards";

        static readonly object Sync = new();
        static readonly ConditionalWeakTable<CardModel, object> CountedCards = new();
        static GnarledHammer? activeRelic;

        internal static GnarledHammer? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        static void Prefix(GnarledHammer __instance) {
            try {
                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(GnarledHammer __instance, Task __result) {
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

        static void Clear(GnarledHammer relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static void CountCard(GnarledHammer relic, CardModel card) {
            try {
                if (relic == null || card == null || card.Owner != relic.Owner) return;

                lock (Sync) {
                    if (CountedCards.TryGetValue(card, out _)) return;
                    CountedCards.Add(card, new object());
                }

                var existing = RelicTracker.GetText(relic, EnchantedCardsKey);
                var name = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
                var cards = string.IsNullOrWhiteSpace(existing)
                    ? new List<string>()
                    : existing.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                cards.Add(name);
                RelicTracker.SetText(relic, EnchantedCardsKey, string.Join("\n", cards));
            } catch { }
        }

        internal static void CountPlayed(CardModel card) {
            try {
                if (card == null) return;
                if (!RelicTracker.HasTrackedRelicType(GnarledHammerTypeName)) return;
                if (!IsSharpAndActive(card)) return;

                var tracked = ParseCardList(RelicTracker.GetTextByType(GnarledHammerTypeName, EnchantedCardsKey));
                if (tracked.Count == 0) return;

                var cardName = DeckUtil.GetCardMatchName(card);
                if (string.IsNullOrWhiteSpace(cardName)) return;
                if (!tracked.TryGetValue(cardName, out var copies) || copies <= 0) return;

                RelicTracker.AddAmountByType(GnarledHammerTypeName, "Enchanted Cards Played", 1);
            } catch { }
        }

        static bool IsSharpAndActive(CardModel card) {
            try {
                var enchantment = ReflectionUtil.GetMemberValue(card, "Enchantment");
                if (enchantment == null) return false;
                if (!string.Equals(enchantment.GetType().Name, "Sharp", StringComparison.Ordinal)) return false;

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

    [HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Enchant), new Type[] {
        typeof(EnchantmentModel),
        typeof(CardModel),
        typeof(decimal)
    })]
    public static class GnarledHammerEnchantPatch {
        static void Postfix(EnchantmentModel enchantment, CardModel card, decimal amount) {
            try {
                var relic = GnarledHammerPatch.ActiveRelic;
                if (relic == null || card == null || enchantment == null) return;
                if (!enchantment.GetType().Name.Contains("Sharp", StringComparison.OrdinalIgnoreCase)) return;
                var expected = Math.Max(1, ReflectionUtil.GetDynamicVarIntValue(relic, "SharpAmount", 3));
                if (amount != expected) return;

                GnarledHammerPatch.CountCard(relic, card);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class GnarledHammerCardPlayPatch {
        static void Postfix(CardModel __instance) {
            GnarledHammerPatch.CountPlayed(__instance);
        }
    }
}
