using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(BlessedAntler), nameof(BlessedAntler.BeforeHandDraw))]
    public static class BlessedAntlerPatch {
        class State {
            public int DazedGiven { get; set; }
        }

        static void Prefix(BlessedAntler __instance, Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ref object __state) {
            try {
                _ = choiceContext;
                _ = combatState;
                if (__instance == null || player == null || combatState == null) return;
                if (__instance.Owner != player) return;
                var owner = __instance.Owner;
                var playerCombatState = owner?.PlayerCombatState;
                if (playerCombatState == null || playerCombatState.TurnNumber != 1) return;

                __state = new State {
                    DazedGiven = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Cards", 3))
                };
            } catch { }
        }

        static void Postfix(BlessedAntler __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.DazedGiven <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Dazed Given", state.DazedGiven);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        RelicTracker.AddAmount(__instance, "Dazed Given", state.DazedGiven);
                    } catch { }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterPlayerTurnStart), new[] { typeof(PlayerChoiceContext), typeof(Player) })]
    public static class BlessedAntlerEnergyPatch {
        static void Postfix(AbstractModel __instance, PlayerChoiceContext choiceContext, Player player) {
            try {
                if (__instance is not BlessedAntler blessedAntler || player == null) return;
                if (blessedAntler.Owner != player) return;

                var energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(blessedAntler, "Energy"));
                if (energy <= 0) return;

                RelicTracker.AddAmount(blessedAntler, "Energy Given", energy);
            } catch { }
        }
    }
}
