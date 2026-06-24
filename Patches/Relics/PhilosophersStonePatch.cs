using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterPlayerTurnStart), new[] { typeof(PlayerChoiceContext), typeof(Player) })]
    public static class PhilosophersStoneEnergyPatch {
        static void Postfix(AbstractModel __instance, PlayerChoiceContext choiceContext, Player player) {
            try {
                if (__instance is not PhilosophersStone philosophersStone || player == null) return;
                if (philosophersStone.Owner != player) return;

                var energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(philosophersStone, "Energy", 1));
                if (energy <= 0) return;
                RelicTracker.AddAmount(philosophersStone, "Energy Given", energy);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(PhilosophersStone), nameof(PhilosophersStone.AfterCreatureAddedToCombat))]
    public static class PhilosophersStoneCreaturePatch {
        class State {
            public int Strength { get; set; }
        }

        static void Prefix(PhilosophersStone __instance, Creature creature, ref object __state) {
            try {
                if (__instance?.Owner?.Creature == null || creature == null) return;
                if (creature.Side == __instance.Owner.Creature.Side) return;
                __state = new State { Strength = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "StrengthPower", 1)) };
            } catch { }
        }

        static void Postfix(PhilosophersStone __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.Strength <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Strength Given", state.Strength);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Strength Given", state.Strength);
                    } catch { }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(PhilosophersStone), nameof(PhilosophersStone.AfterRoomEntered))]
    public static class PhilosophersStoneRoomPatch {
        class State {
            public int Strength { get; set; }
            public int Creatures { get; set; }
        }

        static void Prefix(PhilosophersStone __instance, AbstractRoom room, ref object __state) {
            try {
                if (room is not CombatRoom || __instance?.Owner?.Creature?.CombatState == null) return;
                var owner = __instance.Owner.Creature;
                var opponents = owner.CombatState.GetOpponentsOf(owner);
                var count = 0;
                foreach (var creature in opponents) {
                    if (creature?.IsAlive == true) count++;
                }

                __state = new State {
                    Strength = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "StrengthPower", 1)),
                    Creatures = count
                };
            } catch { }
        }

        static void Postfix(PhilosophersStone __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.Strength <= 0 || state.Creatures <= 0) return;
                var amount = state.Strength * state.Creatures;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Strength Given", amount);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Strength Given", amount);
                    } catch { }
                });
            } catch { }
        }
    }
}
