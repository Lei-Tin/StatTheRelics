using System;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(HelicalDart), nameof(HelicalDart.AfterCardPlayed))]
    public static class HelicalDartPatch {
        class DartState {
            public int Dexterity { get; set; }
        }

        static void Prefix(HelicalDart __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref object __state) {
            try {
                var card = cardPlay?.Card;
                if (__instance == null || card == null || card.Owner != __instance.Owner) return;
                if (card.Tags == null || !card.Tags.Any(tag => Convert.ToInt32(tag) == 5)) return;

                __state = new DartState {
                    Dexterity = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Dexterity", 1))
                };
            } catch { }
        }

        static void Postfix(HelicalDart __instance, Task __result, object __state) {
            try {
                var state = __state as DartState;
                if (state == null || state.Dexterity <= 0) return;

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

        static void Count(HelicalDart relic, DartState state) {
            RelicTracker.AddAmount(relic, "Dexterity Gained", state.Dexterity);
        }
    }
}
