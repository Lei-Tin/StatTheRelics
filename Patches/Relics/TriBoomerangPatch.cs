using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(TriBoomerang), nameof(TriBoomerang.AfterObtained))]
    public static class TriBoomerangPatch {
        class State {
            public Dictionary<int, bool> Enchanted { get; } = new();
        }

        const string TypeName = "MegaCrit.Sts2.Core.Models.Relics.TriBoomerang";
        const string CardsEnchantedKey = "Cards Enchanted";
        static readonly ConditionalWeakTable<CardModel, object> EnchantedCards = new();
        static readonly HashSet<string> EnchantedCardNames = new(StringComparer.Ordinal);
        static readonly object Marker = new();

        static void Prefix(TriBoomerang __instance, ref object __state) {
            try {
                var state = new State();
                foreach (var card in DeckUtil.EnumerateDeckCards(__instance.Owner)) {
                    state.Enchanted[RuntimeHelpers.GetHashCode(card)] = ReflectionUtil.GetMemberValue(card, "Enchantment") != null;
                }

                __state = state;
            } catch { }
        }

        static void Postfix(TriBoomerang __instance, Task __result, object __state) {
            try {
                if (__state is not State state) return;

                if (__result == null) {
                    Count(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, state);
                    } catch { }
                });
            } catch { }
        }

        static void Count(TriBoomerang relic, State state) {
            try {
                var names = new List<string>();
                foreach (var card in DeckUtil.EnumerateDeckCards(relic.Owner)) {
                    var key = RuntimeHelpers.GetHashCode(card);
                    if (!state.Enchanted.TryGetValue(key, out var wasEnchanted) || wasEnchanted) continue;
                    if (ReflectionUtil.GetMemberValue(card, "Enchantment") == null) continue;

                    var name = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
                    if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
                    if (card is CardModel cardModel && !EnchantedCards.TryGetValue(cardModel, out _)) EnchantedCards.Add(cardModel, Marker);
                    if (card is CardModel matchCard) EnchantedCardNames.Add(DeckUtil.GetCardMatchName(matchCard));
                }

                if (names.Count <= 0) return;
                names.Sort(StringComparer.OrdinalIgnoreCase);
                RelicTracker.SetText(relic, CardsEnchantedKey, DeckUtil.JoinCardList(names));
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
    public static class TriBoomerangCardPlayPatch {
        static void Postfix(CardModel __instance) {
            TriBoomerangPatch.CountPlayed(__instance);
        }
    }
}
