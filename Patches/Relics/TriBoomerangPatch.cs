using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
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
        static readonly ConditionalWeakTable<Player, HashSet<string>> EnchantedCardNames = new();
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

                    var name = DeckUtil.GetCardStorageValue(card);
                    if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
                    if (card is CardModel cardModel && !EnchantedCards.TryGetValue(cardModel, out _)) EnchantedCards.Add(cardModel, Marker);
                    if (card is CardModel matchCard && relic.Owner != null) {
                        EnchantedCardNames.GetOrCreateValue(relic.Owner).Add(DeckUtil.GetCardCodeName(matchCard));
                    }
                }

                if (names.Count <= 0) return;
                names.Sort(StringComparer.OrdinalIgnoreCase);
                RelicTracker.SetText(relic, CardsEnchantedKey, DeckUtil.JoinCardList(names));
            } catch { }
        }

        internal static void CountPlayed(CardModel card) {
            try {
                if (card == null) return;
                var relic = ReflectionUtil.FindRelic<TriBoomerang>(card.Owner);
                if (relic == null) return;
                if (ReflectionUtil.GetMemberValue(card, "Enchantment") == null) return;
                var trackedCard = card.DeckVersion ?? card;
                var trackedByInstance = EnchantedCards.TryGetValue(trackedCard, out _);
                var trackedByOwner = card.Owner != null
                    && EnchantedCardNames.TryGetValue(card.Owner, out var names)
                    && names.Contains(DeckUtil.GetCardCodeName(card));
                if (!trackedByInstance && !trackedByOwner) return;
                RelicTracker.AddAmount(relic, "Enchanted Cards Played", 1);
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
