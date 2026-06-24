using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PhylacteryUnbound), nameof(PhylacteryUnbound.BeforeCombatStart))]
    public static class PhylacteryUnboundPatch {
        static void Postfix(PhylacteryUnbound __instance, Task __result) {
            Count(__instance, __result, "StartOfCombat");
        }

        internal static void Count(PhylacteryUnbound relic, Task task, string key) {
            try {
                var amount = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(relic, key));
                if (amount <= 0) return;
                if (task == null) {
                    RelicTracker.AddAmount(relic, "Osty Summoned", amount);
                    return;
                }

                task.ContinueWith(t => {
                    try {
                        if (t.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(relic, "Osty Summoned", amount);
                    } catch { }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(PhylacteryUnbound), nameof(PhylacteryUnbound.AfterSideTurnStart))]
    public static class PhylacteryUnboundTurnPatch {
        static void Prefix(PhylacteryUnbound __instance, System.Collections.Generic.IReadOnlyList<MegaCrit.Sts2.Core.Entities.Creatures.Creature> participants, ref object __state) {
            try {
                if (__instance?.Owner?.Creature == null || participants == null || !participants.Contains(__instance.Owner.Creature)) return;
                __state = true;
            } catch { }
        }

        static void Postfix(PhylacteryUnbound __instance, Task __result, object __state) {
            if (__state is bool) PhylacteryUnboundPatch.Count(__instance, __result, "StartOfTurn");
        }
    }
}
