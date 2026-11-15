using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ExerciseMat), nameof(ExerciseMat.AfterSideTurnStart))]
    public static class ExerciseMatPatch {
        static void Prefix(ExerciseMat __instance, IReadOnlyList<Creature> participants, out int __state) {
            __state = 0;
            try {
                if (!participants.Contains(__instance.Owner.Creature) || (__instance.Owner.PlayerCombatState?.TurnNumber ?? int.MaxValue) > 1) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "StretchPower"));
            } catch { }
        }

        static void Postfix(ExerciseMat __instance, Task __result, int __state) {
            try {
                if (__state <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Stretch Gained", __state);
                    } catch { }
                });
            } catch { }
        }
    }
}
