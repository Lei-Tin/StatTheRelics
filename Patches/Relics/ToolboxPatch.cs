using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Toolbox), nameof(Toolbox.BeforeHandDraw))]
    public static class ToolboxPatch {
        class State {
            public bool Triggered { get; set; }
        }

        static void Prefix(Toolbox __instance, Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ref object __state) {
            try {
                _ = choiceContext;
                _ = combatState;
                if (__instance == null || player == null || __instance.Owner != player) return;
                if (player.PlayerCombatState?.TurnNumber != 1) return;

                __state = new State {
                    Triggered = true
                };
            } catch { }
        }

        static void Postfix(Toolbox __instance, Player player, Task __result, object __state) {
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

        static void Count(Toolbox relic, Player player, State state) {
            try {
                _ = player;
                if (state.Triggered) RelicTracker.AddAmount(relic, "Cards Added", 1);
            } catch { }
        }
    }
}
