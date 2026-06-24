using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Lantern), nameof(Lantern.AfterSideTurnStart))]
    public static class LanternPatch {
        class LanternState {
            public int Energy { get; set; }
        }

        static void Prefix(Lantern __instance, IReadOnlyList<Creature> participants, ref object __state) {
            try {
                if (__instance == null || participants == null) return;
                if (!participants.Contains(__instance.Owner?.Creature)) return;
                if (__instance.Owner?.PlayerCombatState?.TurnNumber > 1) return;

                var energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Energy", 1));
                if (energy <= 0) return;
                __state = new LanternState { Energy = energy };
            } catch { }
        }

        static void Postfix(Lantern __instance, Task __result, object __state) {
            try {
                var state = __state as LanternState;
                if (state == null || state.Energy <= 0) return;

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
