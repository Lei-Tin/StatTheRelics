using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Track all card deltas by diffing deck snapshots around relic resolution.
    [HarmonyPatch(typeof(ArcaneScroll), nameof(ArcaneScroll.AfterObtained))]
    public static class ArcaneScrollPatch {
        static readonly ConcurrentDictionary<int, Dictionary<string, int>> beforeDeckByInstance = new();

        static void Prefix(ArcaneScroll __instance) {
            try {
                beforeDeckByInstance[__instance.GetHashCode()] = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance);
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

                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic);
                var obtainedCards = DeckUtil.FindAddedCards(before, after);

                var obtainedText = DeckUtil.JoinCardList(obtainedCards);

                RelicTracker.SetText(relic, "Cards Obtained", string.IsNullOrWhiteSpace(obtainedText) ? "Unknown" : obtainedText);

                ModLog.Info($"ArcaneScrollPatch: inferred {obtainedCards.Count} obtained cards");
            } catch { }
        }
    }
}
