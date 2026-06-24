using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(NutritiousSoup), nameof(NutritiousSoup.AfterObtained))]
    public static class NutritiousSoupPatch {
        class State {
            public Dictionary<int, bool> Enchanted { get; } = new();
            public Dictionary<int, string> Names { get; } = new();
        }

        const string TypeName = "MegaCrit.Sts2.Core.Models.Relics.NutritiousSoup";
        static readonly ConditionalWeakTable<CardModel, object> SoupStrikes = new();
        static readonly object Marker = new();

        static void Prefix(NutritiousSoup __instance, ref object __state) {
            try {
                var state = new State();
                foreach (var card in DeckUtil.EnumerateDeckCards(__instance.Owner)) {
                    var key = RuntimeHelpers.GetHashCode(card);
                    state.Enchanted[key] = ReflectionUtil.GetMemberValue(card, "Enchantment") != null;
                    state.Names[key] = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
                }

                __state = state;
            } catch { }
        }

        static void Postfix(NutritiousSoup __instance, object __state) {
            try {
                if (__state is not State state) return;
                var count = 0;

                foreach (var card in DeckUtil.EnumerateDeckCards(__instance.Owner)) {
                    if (card is not CardModel cardModel) continue;
                    var key = RuntimeHelpers.GetHashCode(card);
                    if (!state.Enchanted.TryGetValue(key, out var wasEnchanted) || wasEnchanted) continue;
                    if (ReflectionUtil.GetMemberValue(card, "Enchantment") == null) continue;
                    if (!IsBasicStrike(cardModel)) continue;

                    if (!SoupStrikes.TryGetValue(cardModel, out _)) SoupStrikes.Add(cardModel, Marker);
                    count++;
                }

                if (count > 0) RelicTracker.AddAmount(__instance, "Strikes Enchanted", count);
            } catch { }
        }

        internal static void CountPlayed(CardModel card) {
            try {
                if (card == null) return;
                if (!RelicTracker.HasTrackedRelicType(TypeName)) return;
                if (!IsBasicStrike(card)) return;
                if (!IsEnchantedAndActive(card)) return;
                if (!SoupStrikes.TryGetValue(card, out _) && !CanUseReloadFallback()) return;

                RelicTracker.AddAmountByType(TypeName, "Enchanted Strikes Played", 1);
            } catch { }
        }

        static bool CanUseReloadFallback() {
            return RelicTracker.GetCounterByType(TypeName, "Strikes Enchanted") > 0;
        }

        static bool IsBasicStrike(CardModel card) {
            try {
                if (Convert.ToInt32(card.Rarity) != 1) return false;
                return card.Tags != null && card.Tags.Contains((CardTag)1);
            } catch {
                return false;
            }
        }

        static bool IsEnchantedAndActive(CardModel card) {
            try {
                var enchantment = ReflectionUtil.GetMemberValue(card, "Enchantment");
                if (enchantment == null) return false;

                var status = ReflectionUtil.GetMemberValue(enchantment, "Status");
                return status == null || string.Equals(status.ToString(), "Normal", StringComparison.OrdinalIgnoreCase);
            } catch {
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class NutritiousSoupCardPlayPatch {
        static void Postfix(CardModel __instance) {
            NutritiousSoupPatch.CountPlayed(__instance);
        }
    }
}
