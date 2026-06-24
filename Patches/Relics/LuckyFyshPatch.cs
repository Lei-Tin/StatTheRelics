using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(LuckyFysh), nameof(LuckyFysh.AfterCardChangedPiles))]
    public static class LuckyFyshPatch {
        class CardAddedState {
            public int Gold { get; set; }
        }

        static void Prefix(LuckyFysh __instance, CardModel card, PileType oldPileType, AbstractModel clonedBy, ref object __state) {
            try {
                _ = oldPileType;
                _ = clonedBy;
                if (__instance == null || card == null) return;
                if (card.Pile == null || card.Pile.Type != PileType.Deck) return;
                if (card.Owner != __instance.Owner) return;

                __state = new CardAddedState {
                    Gold = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Gold", 15))
                };
            } catch { }
        }

        static void Postfix(LuckyFysh __instance, Task __result, object __state) {
            try {
                var state = __state as CardAddedState;
                if (state == null || state.Gold <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Gold Gained", state.Gold);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            RelicTracker.AddAmount(__instance, "Gold Gained", state.Gold);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
