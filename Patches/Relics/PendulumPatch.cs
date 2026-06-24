using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Pendulum), nameof(Pendulum.AfterPlayerTurnStart))]
    public static class PendulumPatch {
        class State {
            public bool WillTrigger { get; set; }
            public int Cards { get; set; }
        }

        static void Prefix(Pendulum __instance, PlayerChoiceContext choiceContext, Player player, ref object __state) {
            try {
                _ = choiceContext;
                if (__instance == null || player == null || __instance.Owner != player) return;
                var turns = Math.Max(1, ReflectionUtil.GetDynamicVarIntValue(__instance, "Turns", 3));
                var turnsSeen = ReflectionUtil.GetIntMemberValue(__instance, "TurnsSeen", 0);
                __state = new State {
                    WillTrigger = (turnsSeen + 1) % turns == 0,
                    Cards = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Cards", 1))
                };
            } catch { }
        }

        static void Postfix(Pendulum __instance, Task __result, object __state) {
            try {
                if (__state is not State state || !state.WillTrigger || state.Cards <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Cards Drawn", state.Cards);
                    } catch { }
                });
            } catch { }
        }
    }
}
