using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(IvoryTile), nameof(IvoryTile.AfterCardPlayed))]
    public static class IvoryTilePatch {
        class EnergyState {
            public int Energy { get; set; }
        }

        static void Prefix(IvoryTile __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref object __state) {
            try {
                if (cardPlay == null) return;
                var card = cardPlay?.Card;
                if (__instance == null || card == null || card.Owner != __instance.Owner) return;
                if (cardPlay!.Resources.EnergyValue < ReflectionUtil.GetDynamicVarIntValue(__instance, "EnergyThreshold", 3)) return;
                __state = new EnergyState {
                    Energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Energy", 1))
                };
            } catch { }
        }

        static void Postfix(IvoryTile __instance, Task __result, object __state) {
            try {
                var state = __state as EnergyState;
                if (state == null || state.Energy <= 0) return;

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

        static void Count(IvoryTile relic, EnergyState state) {
            RelicTracker.AddAmount(relic, "Energy Gained", state.Energy);
        }
    }
}
