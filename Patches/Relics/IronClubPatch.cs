using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(IronClub), nameof(IronClub.AfterCardPlayed))]
    public static class IronClubPatch {
        class DrawState {
            public bool ShouldDraw { get; set; }
        }

        static void Prefix(IronClub __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref object __state) {
            try {
                var card = cardPlay?.Card;
                if (__instance == null || card == null || card.Owner != __instance.Owner) return;
                var cards = Math.Max(1, ReflectionUtil.GetDynamicVarIntValue(__instance, "Cards", 4));
                var before = ReflectionUtil.GetIntMemberValue(__instance, "CardsPlayed", 0);
                __state = new DrawState {
                    ShouldDraw = ((before + 1) % cards) == 0
                };
            } catch { }
        }

        static void Postfix(IronClub __instance, Task __result, object __state) {
            try {
                var state = __state as DrawState;
                if (state == null || !state.ShouldDraw) return;

                if (__result == null) {
                    Count(__instance);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance);
                    } catch { }
                });
            } catch { }
        }

        static void Count(IronClub relic) {
            RelicTracker.AddAmount(relic, "Cards Drawn", 1);
        }
    }
}
