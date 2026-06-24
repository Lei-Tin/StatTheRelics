using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ToastyMittens), nameof(ToastyMittens.BeforeHandDraw))]
    public static class ToastyMittensPatch {
        class State {
            public int ExhaustBefore { get; set; }
            public int Strength { get; set; }
        }

        static void Prefix(ToastyMittens __instance, Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ref object __state) {
            try {
                _ = choiceContext;
                _ = combatState;
                if (__instance == null || player == null || __instance.Owner?.Creature?.Player != player) return;
                __state = new State {
                    ExhaustBefore = PileType.Exhaust.GetPile(player)?.Cards?.Count ?? 0,
                    Strength = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Strength", 1))
                };
            } catch { }
        }

        static void Postfix(ToastyMittens __instance, Player player, Task __result, object __state) {
            try {
                if (__state is not State state) return;

                if (__result == null) {
                    Count(__instance, player, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, player, state);
                    } catch { }
                });
            } catch { }
        }

        static void Count(ToastyMittens relic, Player player, State state) {
            try {
                if (state.Strength > 0) RelicTracker.AddAmount(relic, "Strength Gained", state.Strength);

                var exhaustAfter = PileType.Exhaust.GetPile(player)?.Cards?.Count ?? state.ExhaustBefore;
                var exhausted = Math.Max(0, exhaustAfter - state.ExhaustBefore);
                if (exhausted > 0) RelicTracker.AddAmount(relic, "Cards Exhausted", exhausted);
            } catch { }
        }
    }
}
