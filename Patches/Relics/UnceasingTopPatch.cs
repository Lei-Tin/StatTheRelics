using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(UnceasingTop), nameof(UnceasingTop.AfterHandEmptied))]
    public static class UnceasingTopPatch {
        class State {
            public bool Triggered { get; set; }
        }

        static void Prefix(UnceasingTop __instance, PlayerChoiceContext choiceContext, Player player, ref object __state) {
            try {
                _ = choiceContext;
                if (__instance == null || player == null || __instance.Owner != player) return;

                var phase = Convert.ToInt32(player.PlayerCombatState?.Phase);
                if (phase < 2 || phase > 4) return;

                __state = new State { Triggered = true };
            } catch { }
        }

        static void Postfix(UnceasingTop __instance, Task __result, object __state) {
            try {
                if (__state is not State state || !state.Triggered) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Cards Drawn", 1);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Cards Drawn", 1);
                    } catch { }
                });
            } catch { }
        }
    }
}
