using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(SymbioticVirus), nameof(SymbioticVirus.AfterSideTurnStart))]
    public static class SymbioticVirusPatch {
        static void Prefix(SymbioticVirus __instance, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ref object __state) {
            try {
                _ = side;
                _ = combatState;
                if (__instance?.Owner?.Creature == null || participants == null) return;
                if (!participants.Contains(__instance.Owner.Creature)) return;
                if (__instance.Owner.PlayerCombatState?.TurnNumber > 1) return;

                var amount = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Dark", 1));
                if (amount > 0) __state = amount;
            } catch { }
        }

        static void Postfix(SymbioticVirus __instance, Task __result, object __state) {
            try {
                if (__state is not int amount || amount <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Dark Orbs Channeled", amount);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Dark Orbs Channeled", amount);
                    } catch { }
                });
            } catch { }
        }
    }
}
