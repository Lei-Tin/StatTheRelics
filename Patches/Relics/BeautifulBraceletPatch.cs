using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    static class BeautifulBraceletSwiftTracker {
        internal const string BeautifulBraceletTypeName = "MegaCrit.Sts2.Core.Models.Relics.BeautifulBracelet";
        const string TrackedSwiftCardsDisplayKey = "Swift Cards Enchanted";

        static readonly HashSet<int> BeforeSwiftCards = new();
        static readonly ConditionalWeakTable<CardModel, object> TrackedCards = new();
        static readonly object Marker = new();

        public static void CaptureBefore(BeautifulBracelet relic) {
            try {
                BeforeSwiftCards.Clear();
                var swiftAmount = GetSwiftAmount(relic);
                foreach (var card in DeckUtil.EnumerateDeckCards(relic.Owner)) {
                    if (!IsSwift(card, swiftAmount)) continue;
                    BeforeSwiftCards.Add(RuntimeHelpers.GetHashCode(card));
                }
            } catch { }
        }

        public static void CaptureAfter(BeautifulBracelet relic) {
            try {
                var swiftAmount = GetSwiftAmount(relic);
                var stored = RelicTracker.GetStoredText(relic, TrackedSwiftCardsDisplayKey);
                var changed = false;
                var occurrences = new Dictionary<string, int>(StringComparer.Ordinal);

                foreach (var card in DeckUtil.EnumerateDeckCards(relic.Owner)) {
                    var typeName = DeckUtil.GetCardCodeName(card);
                    var occurrence = occurrences.TryGetValue(typeName, out var currentOccurrence) ? currentOccurrence : 0;
                    occurrences[typeName] = occurrence + 1;

                    if (card is not CardModel cardModel || !IsSwift(cardModel, swiftAmount)) continue;
                    if (BeforeSwiftCards.Contains(RuntimeHelpers.GetHashCode(cardModel))) continue;

                    if (!TrackedCards.TryGetValue(cardModel, out _)) TrackedCards.Add(cardModel, Marker);
                    stored = DeckUtil.AppendCardList(stored, cardModel, occurrence);
                    changed = true;
                }

                if (changed && !string.IsNullOrWhiteSpace(stored)) {
                    RelicTracker.SetText(relic, TrackedSwiftCardsDisplayKey, stored);
                }
            } catch { }
        }

        public static bool TryCountTrackedSwiftCardPlay(CardModel card) {
            try {
                if (card == null) return false;
                var relic = ReflectionUtil.FindRelic<BeautifulBracelet>(card.Owner);
                if (relic == null || !IsSwiftAndActive(card, GetSwiftAmount(relic))) return false;

                var trackedCard = card.DeckVersion ?? card;
                var trackedThisSession = TrackedCards.TryGetValue(trackedCard, out _);
                if (!trackedThisSession) {
                    var stored = RelicTracker.GetStoredTextByType(BeautifulBraceletTypeName, TrackedSwiftCardsDisplayKey);
                    if (!DeckUtil.StoredCardListContains(stored, card, matchOccurrence: true)) return false;
                }

                RelicTracker.AddAmountByType(BeautifulBraceletTypeName, "Swift Cards Played", 1);
                return true;
            } catch {
                return false;
            }
        }

        static int GetSwiftAmount(BeautifulBracelet relic) {
            return Math.Max(1, ReflectionUtil.GetDynamicVarIntValue(relic, "Swift", 3));
        }

        static bool IsSwift(object card, int amount) {
            var enchantment = ReflectionUtil.GetMemberValue(card, "Enchantment");
            if (enchantment == null || !string.Equals(enchantment.GetType().Name, "Swift", StringComparison.Ordinal)) return false;
            return ReflectionUtil.GetIntMemberValue(enchantment, "Amount", 0) == amount;
        }

        static bool IsSwiftAndActive(CardModel card, int amount) {
            try {
                if (!IsSwift(card, amount)) return false;
                var status = ReflectionUtil.GetMemberValue(card.Enchantment, "Status");
                return status is EnchantmentStatus enchantmentStatus && enchantmentStatus == EnchantmentStatus.Normal;
            } catch {
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(BeautifulBracelet), nameof(BeautifulBracelet.AfterObtained))]
    public static class BeautifulBraceletPatch {
        static void Prefix(BeautifulBracelet __instance) {
            try {
                if (__instance == null) return;
                BeautifulBraceletSwiftTracker.CaptureBefore(__instance);
            } catch { }
        }

        static void Postfix(BeautifulBracelet __instance, Task __result) {
            try {
                if (__instance == null) return;
                if (__result == null) {
                    BeautifulBraceletSwiftTracker.CaptureAfter(__instance);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) BeautifulBraceletSwiftTracker.CaptureAfter(__instance);
                    } catch { }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(Swift), nameof(Swift.OnPlay))]
    public static class BeautifulBraceletCardPlayPatch {
        static void Prefix(Swift __instance) {
            try {
                var card = __instance?.Card;
                if (card == null) return;
                if (!RelicTracker.HasTrackedRelicType(BeautifulBraceletSwiftTracker.BeautifulBraceletTypeName)) return;
                BeautifulBraceletSwiftTracker.TryCountTrackedSwiftCardPlay(card);
            } catch { }
        }
    }
}
