using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    // Astrolabe transforms 3 chosen cards; infer which names left and entered the deck.
    [HarmonyPatch(typeof(Astrolabe), nameof(Astrolabe.AfterObtained))]
    public static class AstrolabePatch {
        static readonly ConcurrentDictionary<int, Dictionary<string, int>> beforeDeckByInstance = new();

        static void Prefix(Astrolabe __instance) {
            try {
                beforeDeckByInstance[__instance.GetHashCode()] = CaptureDeckHistogram(__instance);
            } catch { }
        }

        static void Postfix(Astrolabe __instance, Task __result) {
            try {
                if (__result == null) {
                    FinalizeCardTracking(__instance);
                    return;
                }

                __result.ContinueWith(_ => {
                    FinalizeCardTracking(__instance);
                });
            } catch { }
        }

        static void FinalizeCardTracking(Astrolabe relic) {
            try {
                var instanceKey = relic.GetHashCode();
                beforeDeckByInstance.TryRemove(instanceKey, out var before);
                before ??= new Dictionary<string, int>(StringComparer.Ordinal);

                var after = CaptureDeckHistogram(relic);
                var lostCards = FindRemovedCards(before, after);
                var obtainedCards = FindAddedCards(before, after);

                var lostText = JoinCardList(lostCards);
                var obtainedText = JoinCardList(obtainedCards);

                RelicTracker.SetText(relic, "Cards Lost", string.IsNullOrWhiteSpace(lostText) ? "Unknown" : lostText);
                RelicTracker.SetText(relic, "Cards Obtained", string.IsNullOrWhiteSpace(obtainedText) ? "Unknown" : obtainedText);

                ModLog.Info($"AstrolabePatch: inferred {lostCards.Count} lost and {obtainedCards.Count} obtained cards");
            } catch { }
        }

        static List<string> FindRemovedCards(Dictionary<string, int> before, Dictionary<string, int> after) {
            var lost = new List<string>();
            foreach (var kv in before) {
                var afterVal = after.TryGetValue(kv.Key, out var a) ? a : 0;
                var delta = kv.Value - afterVal;
                for (var i = 0; i < delta; i++) lost.Add(kv.Key);
            }
            lost.Sort(StringComparer.OrdinalIgnoreCase);
            return lost;
        }

        static List<string> FindAddedCards(Dictionary<string, int> before, Dictionary<string, int> after) {
            var obtained = new List<string>();
            foreach (var kv in after) {
                var beforeVal = before.TryGetValue(kv.Key, out var b) ? b : 0;
                var delta = kv.Value - beforeVal;
                for (var i = 0; i < delta; i++) obtained.Add(kv.Key);
            }
            obtained.Sort(StringComparer.OrdinalIgnoreCase);
            return obtained;
        }

        static string JoinCardList(IReadOnlyList<string> cards) {
            if (cards == null || cards.Count == 0) return string.Empty;
            return string.Join("\n", cards);
        }

        static Dictionary<string, int> CaptureDeckHistogram(Astrolabe relic) {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var card in EnumerateDeckCards(relic)) {
                var key = GetCardDisplayName(card);
                if (string.IsNullOrWhiteSpace(key)) continue;
                result[key] = result.TryGetValue(key, out var v) ? v + 1 : 1;
            }
            return result;
        }

        static IEnumerable<object> EnumerateDeckCards(Astrolabe relic) {
            var owner = ReflectionUtil.GetMemberValue(relic, "Owner");
            if (owner == null) yield break;

            var deck = ReflectionUtil.GetMemberValue(owner, "Deck");
            if (deck == null) yield break;

            var cardsContainer = ReflectionUtil.GetMemberValue(deck, "Cards") ?? deck;
            if (cardsContainer is not IEnumerable enumerable) yield break;

            foreach (var card in enumerable) {
                if (card != null) yield return card;
            }
        }

        static string GetCardDisplayName(object card) {
            var str = ReflectionUtil.GetCardTitle(card);
            if (!string.IsNullOrWhiteSpace(str)) return str;
            return card.GetType().Name;
        }
    }
}
