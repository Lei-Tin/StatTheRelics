using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    public static class LavaLampPatch {
        sealed class Mark {
            public LavaLamp Relic { get; }

            public Mark(LavaLamp relic) {
                Relic = relic;
            }
        }

        static readonly object MarkLock = new();
        static readonly ConditionalWeakTable<CardModel, Mark> MarkedCards = new();
        static readonly ConditionalWeakTable<CardModel, Mark> CountedCards = new();
        static int pendingMarks;

        internal static void MarkFromCreationResult(CardCreationResult result) {
            try {
                if (result == null) return;
                var relic = result.ModifyingRelics?.OfType<LavaLamp>().FirstOrDefault();
                if (relic == null) return;
                MarkCard(result.Card, relic);
            } catch { }
        }

        internal static void CountWhenAdded(Task<CardPileAddResult> task, CardModel fallbackCard) {
            try {
                if (task == null) return;
                lock (MarkLock) {
                    if (pendingMarks <= 0) return;
                }

                task.ContinueWith(t => {
                    try {
                        if (t.Status == TaskStatus.RanToCompletion) CountWhenAdded(t.Result, fallbackCard);
                    } catch { }
                });
            } catch { }
        }

        internal static void CountWhenAdded(Task<IReadOnlyList<CardPileAddResult>> task, IEnumerable<CardModel> fallbackCards) {
            try {
                if (task == null) return;
                lock (MarkLock) {
                    if (pendingMarks <= 0) return;
                }

                var fallbackList = fallbackCards?.ToList() ?? new List<CardModel>();
                task.ContinueWith(t => {
                    try {
                        if (t.Status != TaskStatus.RanToCompletion || t.Result == null) return;
                        for (var i = 0; i < t.Result.Count; i++) {
                            var fallback = i < fallbackList.Count ? fallbackList[i] : null;
                            CountWhenAdded(t.Result[i], fallback);
                        }
                    } catch { }
                });
            } catch { }
        }

        static void MarkCard(CardModel card, LavaLamp relic) {
            try {
                if (card == null || relic == null) return;
                lock (MarkLock) {
                    if (MarkedCards.TryGetValue(card, out _)) return;
                    MarkedCards.Add(card, new Mark(relic));
                    pendingMarks++;
                }
            } catch { }
        }

        static void CountWhenAdded(CardPileAddResult result, CardModel? fallbackCard) {
            try {
                if (!WasAdded(result)) return;
                var card = ReflectionUtil.GetMemberValue(result, "cardAdded") as CardModel ?? fallbackCard;
                if (card == null) return;

                Mark? mark;
                lock (MarkLock) {
                    if (!MarkedCards.TryGetValue(card, out mark)) return;
                    MarkedCards.Remove(card);
                    if (pendingMarks > 0) pendingMarks--;
                    if (CountedCards.TryGetValue(card, out _)) return;
                    CountedCards.Add(card, mark);
                }

                RelicTracker.AddAmount(mark.Relic, "Added Cards Taken", 1);
            } catch { }
        }

        static bool WasAdded(CardPileAddResult result) {
            try {
                var success = ReflectionUtil.GetMemberValue(result, "success");
                return success is bool value && value;
            } catch {
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(CardCreationResult), nameof(CardCreationResult.ModifyCard), new Type[] {
        typeof(CardModel),
        typeof(RelicModel)
    })]
    public static class LavaLampCardCreationResultPatch {
        static void Postfix(CardCreationResult __instance) {
            LavaLampPatch.MarkFromCreationResult(__instance);
        }
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Add), new Type[] {
        typeof(CardModel),
        typeof(PileType),
        typeof(CardPilePosition),
        typeof(AbstractModel),
        typeof(bool)
    })]
    public static class LavaLampCardPileAddByTypePatch {
        static void Postfix(CardModel card, Task<CardPileAddResult> __result) {
            LavaLampPatch.CountWhenAdded(__result, card);
        }
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Add), new Type[] {
        typeof(CardModel),
        typeof(CardPile),
        typeof(CardPilePosition),
        typeof(AbstractModel),
        typeof(bool)
    })]
    public static class LavaLampCardPileAddByPilePatch {
        static void Postfix(CardModel card, Task<CardPileAddResult> __result) {
            LavaLampPatch.CountWhenAdded(__result, card);
        }
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Add), new Type[] {
        typeof(IEnumerable<CardModel>),
        typeof(PileType),
        typeof(CardPilePosition),
        typeof(AbstractModel),
        typeof(bool)
    })]
    public static class LavaLampCardPileAddManyByTypePatch {
        static void Postfix(IEnumerable<CardModel> cards, Task<IReadOnlyList<CardPileAddResult>> __result) {
            LavaLampPatch.CountWhenAdded(__result, cards);
        }
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Add), new Type[] {
        typeof(IEnumerable<CardModel>),
        typeof(CardPile),
        typeof(CardPilePosition),
        typeof(AbstractModel),
        typeof(bool)
    })]
    public static class LavaLampCardPileAddManyByPilePatch {
        static void Postfix(IEnumerable<CardModel> cards, Task<IReadOnlyList<CardPileAddResult>> __result) {
            LavaLampPatch.CountWhenAdded(__result, cards);
        }
    }
}
