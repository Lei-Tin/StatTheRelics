using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PaelsClaw), nameof(PaelsClaw.AfterObtained))]
    public static class PaelsClawPatch {
        class State {
            public Dictionary<int, bool> Enchanted { get; } = new();
        }

        const string TypeName = "MegaCrit.Sts2.Core.Models.Relics.PaelsClaw";
        static readonly ConditionalWeakTable<CardModel, object> EnchantedCards = new();
        static readonly HashSet<string> EnchantedCardNames = new(System.StringComparer.Ordinal);
        static readonly object Marker = new();

        static void Prefix(PaelsClaw __instance, ref object __state) {
            try {
                var state = new State();
                foreach (var card in DeckUtil.EnumerateDeckCards(__instance.Owner)) {
                    state.Enchanted[RuntimeHelpers.GetHashCode(card)] = ReflectionUtil.GetMemberValue(card, "Enchantment") != null;
                }
                __state = state;
            } catch { }
        }

        static void Postfix(PaelsClaw __instance, object __state) {
            try {
                if (__state is not State state) return;
                var count = 0;
                foreach (var card in DeckUtil.EnumerateDeckCards(__instance.Owner)) {
                    if (card is not CardModel cardModel) continue;
                    var key = RuntimeHelpers.GetHashCode(card);
                    if (!state.Enchanted.TryGetValue(key, out var wasEnchanted) || wasEnchanted) continue;
                    if (ReflectionUtil.GetMemberValue(card, "Enchantment") == null) continue;

                    count++;
                    if (!EnchantedCards.TryGetValue(cardModel, out _)) EnchantedCards.Add(cardModel, Marker);
                    EnchantedCardNames.Add(DeckUtil.GetCardMatchName(cardModel));
                }

                if (count > 0) RelicTracker.AddAmount(__instance, "Cards Enchanted", count);
            } catch { }
        }

        internal static void CountPlayed(CardModel card) {
            try {
                if (card == null) return;
                if (!RelicTracker.HasTrackedRelicType(TypeName)) return;
                if (ReflectionUtil.GetMemberValue(card, "Enchantment") == null) return;
                if (!EnchantedCards.TryGetValue(card, out _) && !EnchantedCardNames.Contains(DeckUtil.GetCardMatchName(card))) return;
                RelicTracker.AddAmountByType(TypeName, "Enchanted Cards Played", 1);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class PaelsClawCardPlayPatch {
        static void Postfix(CardModel __instance) {
            PaelsClawPatch.CountPlayed(__instance);
        }
    }
}
