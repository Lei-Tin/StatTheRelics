using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Arcane Scroll adds one rare card when obtained; track the concrete card name.
    [HarmonyPatch(typeof(ArcaneScroll), nameof(ArcaneScroll.AfterObtained))]
    public static class ArcaneScrollPatch {
        static readonly ConcurrentDictionary<int, Dictionary<string, int>> beforeDeckByInstance = new();

        static void Prefix(ArcaneScroll __instance) {
            try {
                beforeDeckByInstance[__instance.GetHashCode()] = CaptureDeckHistogram(__instance);
            } catch { }
        }

        static void Postfix(ArcaneScroll __instance, Task __result) {
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

        static void FinalizeCardTracking(ArcaneScroll relic) {
            try {
                var instanceKey = relic.GetHashCode();
                beforeDeckByInstance.TryRemove(instanceKey, out var before);
                before ??= new Dictionary<string, int>(StringComparer.Ordinal);

                var after = CaptureDeckHistogram(relic);
                var gained = FindAddedCard(before, after);
                if (!string.IsNullOrWhiteSpace(gained)) {
                    RelicTracker.SetText(relic, "Card Obtained", gained);
                    ModLog.Info($"ArcaneScrollPatch: obtained card '{gained}'");
                } else {
                    RelicTracker.SetText(relic, "Card Obtained", "Unknown");
                    ModLog.Info("ArcaneScrollPatch: could not infer obtained card from deck diff");
                }
            } catch { }
        }

        // We would expect if there was a difference, it should only differ by 1
        // But doing defensive programming here to basically pick the most suspiscious one
        // The one that differs the most
        static string? FindAddedCard(Dictionary<string, int> before, Dictionary<string, int> after) {
            string? best = null;
            var bestDelta = 0;
            foreach (var kv in after) {
                var beforeVal = before.TryGetValue(kv.Key, out var b) ? b : 0;
                var delta = kv.Value - beforeVal;
                if (delta > bestDelta) {
                    bestDelta = delta;
                    best = kv.Key;
                }
            }
            return best;
        }

        static Dictionary<string, int> CaptureDeckHistogram(ArcaneScroll relic) {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var card in EnumerateDeckCards(relic)) {
                var key = GetCardDisplayName(card);
                if (string.IsNullOrWhiteSpace(key)) continue;
                result[key] = result.TryGetValue(key, out var v) ? v + 1 : 1;
            }
            return result;
        }

        static IEnumerable<object> EnumerateDeckCards(ArcaneScroll relic) {
            var owner = ReflectionUtil.GetMemberValue(relic, "Owner");
            if (owner == null) yield break;

            var deck = ReflectionUtil.GetMemberValue(owner, "Deck");
            if (deck == null) yield break;

            // List of card models
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
