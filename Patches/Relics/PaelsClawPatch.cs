using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
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
        static readonly ConditionalWeakTable<Player, HashSet<string>> EnchantedCardNames = new();
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
                    if (__instance.Owner != null) {
                        EnchantedCardNames.GetOrCreateValue(__instance.Owner).Add(DeckUtil.GetCardCodeName(cardModel));
                    }
                }

                if (count > 0) RelicTracker.AddAmount(__instance, "Cards Enchanted", count);
            } catch { }
        }

        internal static void CountPlayed(CardModel card) {
            try {
                if (card == null) return;
                var relic = ReflectionUtil.FindRelic<PaelsClaw>(card.Owner);
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
    public static class PaelsClawCardPlayPatch {
        static void Postfix(CardModel __instance) {
            PaelsClawPatch.CountPlayed(__instance);
        }
    }
}
