using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(GamblingChip), nameof(GamblingChip.AfterPlayerTurnStart))]
    public static class GamblingChipPatch {
        static GamblingChip? Current;

        static void Prefix(GamblingChip __instance, PlayerChoiceContext choiceContext, Player player) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                if (__instance.Owner?.PlayerCombatState?.TurnNumber > 1) return;
                Current = __instance;
            } catch { }
        }

        static void Postfix(Task __result) {
            try {
                if (__result == null) {
                    Current = null;
                    return;
                }

                __result.ContinueWith(_ => Current = null);
            } catch {
                Current = null;
            }
        }

        internal static GamblingChip? Active => Current;
    }

    [HarmonyPatch(typeof(CardCmd), nameof(CardCmd.DiscardAndDraw), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(IEnumerable<CardModel>),
        typeof(int)
    })]
    public static class GamblingChipDiscardAndDrawPatch {
        static void Prefix(IEnumerable<CardModel> cardsToDiscard, int cardsToDraw, ref object __state) {
            try {
                var relic = GamblingChipPatch.Active;
                if (relic == null || cardsToDiscard == null) return;
                var count = cardsToDiscard.Count();
                if (count <= 0) return;
                __state = Tuple.Create(relic, count, cardsToDraw);
            } catch { }
        }

        static void Postfix(Task __result, object __state) {
            try {
                if (__state is not Tuple<GamblingChip, int, int> state) return;

                void Count() {
                    try {
                        RelicTracker.AddAmount(state.Item1, "Cards Discarded & Drawn", state.Item2);
                    } catch { }
                }

                if (__result == null) {
                    Count();
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count();
                    } catch { }
                });
            } catch { }
        }
    }
}
