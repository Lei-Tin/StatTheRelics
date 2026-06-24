using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(WhisperingEarring), nameof(WhisperingEarring.AfterAutoPrePlayPhaseEnteredLate))]
    public static class WhisperingEarringPatch {
        internal sealed class State {
            public WhisperingEarring? Relic { get; set; }
            public bool AutoPlayInvoked { get; set; }
        }

        static readonly object Sync = new();
        static State? activeState;

        internal static State? ActiveState {
            get {
                lock (Sync) return activeState;
            }
        }

        static void Prefix(WhisperingEarring __instance, Player player, ref object __state) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                if (__instance.Owner?.PlayerCombatState?.TurnNumber > 1) return;

                var state = new State { Relic = __instance };
                __state = state;
                lock (Sync) activeState = state;
            } catch { }
        }

        static void Postfix(WhisperingEarring __instance, Task __result, object __state) {
            try {
                var state = __state as State;
                if (__result == null) {
                    CountIfPlayed(state);
                    Clear(__instance);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) CountIfPlayed(state);
                        Clear(__instance);
                    } catch { }
                });
            } catch { }
        }

        static void Clear(WhisperingEarring relic) {
            lock (Sync) {
                if (ReferenceEquals(activeState?.Relic, relic)) activeState = null;
            }
        }

        internal static void MarkAutoPlayInvoked() {
            try {
                var state = ActiveState;
                if (state != null) state.AutoPlayInvoked = true;
            } catch { }
        }

        internal static void CountAutoPlayed(Task task) {
            try {
                var state = ActiveState;
                var relic = state?.Relic;
                if (relic == null) return;
                state!.AutoPlayInvoked = true;

                if (task == null) {
                    RelicTracker.AddAmount(relic, "Cards Auto Played", 1);
                    return;
                }

                task.ContinueWith(t => {
                    try {
                        if (t.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(relic, "Cards Auto Played", 1);
                    } catch { }
                });
            } catch { }
        }

        static void CountIfPlayed(State? state) {
            if (state?.Relic == null || !state.AutoPlayInvoked) return;
            RelicTracker.AddAmount(state.Relic, "Turns Vakuu Played", 1);
        }
    }

    [HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterPlayerTurnStart), new[] { typeof(PlayerChoiceContext), typeof(Player) })]
    public static class WhisperingEarringEnergyPatch {
        static void Postfix(AbstractModel __instance, PlayerChoiceContext choiceContext, Player player) {
            try {
                _ = choiceContext;
                if (__instance is not WhisperingEarring relic || player == null || relic.Owner != player) return;

                var energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(relic, "Energy", 1));
                if (energy > 0) RelicTracker.AddAmount(relic, "Energy Given", energy);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardCmd), nameof(CardCmd.AutoPlay), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(CardModel),
        typeof(Creature),
        typeof(AutoPlayType),
        typeof(bool),
        typeof(bool)
    })]
    public static class WhisperingEarringAutoPlayPatch {
        static void Postfix(Task __result) {
            WhisperingEarringPatch.CountAutoPlayed(__result);
        }
    }
}
