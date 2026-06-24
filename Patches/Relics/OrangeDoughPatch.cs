using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(OrangeDough), nameof(OrangeDough.AfterSideTurnStart))]
    public static class OrangeDoughPatch {
        static void Prefix(OrangeDough __instance, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ref object __state) {
            try {
                _ = side;
                _ = combatState;
                if (__instance == null || participants == null || !participants.Contains(__instance.Owner.Creature)) return;
                if (__instance.Owner.PlayerCombatState?.TurnNumber > 1) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Cards", 2));
            } catch { }
        }

        static void Postfix(OrangeDough __instance, Task __result, object __state) {
            try {
                var amount = __state is int value ? value : 0;
                if (amount <= 0 || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Cards Generated", amount);
                    } catch { }
                });
            } catch { }
        }
    }
}
