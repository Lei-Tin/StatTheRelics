using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // BiiigHug removes selected cards on obtain; infer and record which card names were removed.
    [HarmonyPatch(typeof(BiiigHug), nameof(BiiigHug.AfterObtained))]
    public static class BiiigHugPatch {
        static readonly ConcurrentDictionary<int, Dictionary<string, int>> beforeDeckByInstance = new();

        static void Prefix(BiiigHug __instance) {
            try {
                beforeDeckByInstance[__instance.GetHashCode()] = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance);
            } catch { }
        }

        static void Postfix(BiiigHug __instance, Task __result) {
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

        static void FinalizeCardTracking(BiiigHug relic) {
            try {
                var instanceKey = relic.GetHashCode();
                beforeDeckByInstance.TryRemove(instanceKey, out var before);
                before ??= new Dictionary<string, int>(StringComparer.Ordinal);

                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic);
                var removedCards = DeckUtil.FindRemovedCards(before, after);
                var removedText = DeckUtil.JoinCardList(removedCards);

                RelicTracker.SetText(relic, "Cards Removed", string.IsNullOrWhiteSpace(removedText) ? "Unknown" : removedText);

                ModLog.Info($"BiiigHugPatch: inferred {removedCards.Count} removed cards");
            } catch { }
        }
    }

    [HarmonyPatch(typeof(BiiigHug), nameof(BiiigHug.AfterShuffle))]
    public static class BiiigHugAfterShufflePatch {
        [ThreadStatic] internal static BiiigHug? Current;

        static void Prefix(BiiigHug __instance, PlayerChoiceContext choiceContext, Player shuffler) {
            try {
                if (__instance == null || shuffler == null) return;
                if (__instance.Owner != shuffler) return;
                Current = __instance;
            } catch { }
        }

        static void Postfix() {
            Current = null;
        }
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.AddGeneratedCardToCombat), new Type[] {
        typeof(CardModel),
        typeof(PileType),
        typeof(Player),
        typeof(CardPilePosition)
    })]
    public static class BiiigHugGeneratedCardPatch {
        static void Postfix(Task<CardPileAddResult> __result) {
            try {
                var relic = BiiigHugAfterShufflePatch.Current;
                if (relic == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        var success = ReflectionUtil.GetMemberValue(task.Result, "success");
                        if (success is bool ok && ok) RelicTracker.AddAmount(relic, "Soots Added", 1);
                    } catch { }
                });
            } catch { }
        }
    }
}
