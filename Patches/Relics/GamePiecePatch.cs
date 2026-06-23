using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(GamePiece), nameof(GamePiece.AfterCardPlayed))]
    public static class GamePiecePatch {
        class PlayState {
            public int CardsDrawn { get; set; }
        }

        static void Prefix(GamePiece __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref object __state) {
            try {
                var card = cardPlay?.Card;
                if (__instance == null || card?.Owner != __instance.Owner) return;
                if (CombatManager.Instance?.IsInProgress != true) return;
                if (Convert.ToInt32(card.Type) != 3) return;
                __state = new PlayState {
                    CardsDrawn = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Cards", 1))
                };
            } catch { }
        }

        static void Postfix(GamePiece __instance, Task __result, object __state) {
            try {
                var state = __state as PlayState;
                if (state == null) return;

                if (__result == null) {
                    Count(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, state);
                    } catch { }
                });
            } catch { }
        }

        static void Count(GamePiece relic, PlayState state) {
            try {
                RelicTracker.AddAmount(relic, "Cards Drawn", state.CardsDrawn);
            } catch { }
        }
    }
}
