using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(VeryHotCocoa), nameof(VeryHotCocoa.AfterSideTurnStart))]
    public static class VeryHotCocoaPatch {
        sealed class State {
            public int Energy { get; set; }
        }

        static void Prefix(VeryHotCocoa __instance, IReadOnlyList<Creature> participants, ref object __state) {
            try {
                if (__instance == null || participants == null) return;
                if (!participants.Contains(__instance.Owner?.Creature)) return;
                if (__instance.Owner?.PlayerCombatState?.TurnNumber > 1) return;

                var energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Energy", 4));
                if (energy > 0) __state = new State { Energy = energy };
            } catch { }
        }

        static void Postfix(VeryHotCocoa __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.Energy <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Energy Gained", state.Energy);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Energy Gained", state.Energy);
                    } catch { }
                });
            } catch { }
        }
    }
}
