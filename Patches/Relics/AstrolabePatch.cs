using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Astrolabe transforms 3 chosen cards; infer which names left and entered the deck.
    [HarmonyPatch(typeof(Astrolabe), nameof(Astrolabe.AfterObtained))]
    public static class AstrolabePatch {
        static readonly ConcurrentDictionary<int, Dictionary<string, int>> beforeDeckByInstance = new();

        static void Prefix(Astrolabe __instance) {
            try {
                beforeDeckByInstance[__instance.GetHashCode()] = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance);
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

                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic);
                var lostCards = DeckUtil.FindRemovedCards(before, after);
                var obtainedCards = DeckUtil.FindAddedCards(before, after);

                var lostText = DeckUtil.JoinCardList(lostCards);
                var obtainedText = DeckUtil.JoinCardList(obtainedCards);

                RelicTracker.SetText(relic, "Cards Lost", string.IsNullOrWhiteSpace(lostText) ? "Unknown" : lostText);
                RelicTracker.SetText(relic, "Cards Obtained", string.IsNullOrWhiteSpace(obtainedText) ? "Unknown" : obtainedText);

            } catch { }
        }
    }
}
