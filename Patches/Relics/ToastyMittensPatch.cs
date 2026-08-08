using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch]
    [PatchTargetAlternative(typeof(ToastyMittens), "AfterPlayerTurnStart")]
    [PatchTargetAlternative(typeof(ToastyMittens), "BeforeHandDraw")]
    public static class ToastyMittensPatch {
        static MethodBase TargetMethod() => PatchTargetResolver.RequireAny(
            typeof(ToastyMittens),
            new PatchTargetCandidate("AfterPlayerTurnStart"),
            new PatchTargetCandidate("BeforeHandDraw")
        );

        class State {
            public int ExhaustBefore { get; set; }
            public int Strength { get; set; }
            public Player? Player { get; set; }
        }

        static void Prefix(ToastyMittens __instance, object[] __args, ref object __state) {
            try {
                var player = Array.Find(__args, argument => argument is Player) as Player;
                if (__instance == null || player == null || __instance.Owner?.Creature?.Player != player) return;
                __state = new State {
                    ExhaustBefore = PileType.Exhaust.GetPile(player)?.Cards?.Count ?? 0,
                    Strength = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Strength", 1)),
                    Player = player
                };
            } catch { }
        }

        static void Postfix(ToastyMittens __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.Player == null) return;

                if (__result == null) {
                    Count(__instance, state.Player, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, state.Player, state);
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
