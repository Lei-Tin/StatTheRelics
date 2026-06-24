using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(LizardTail), nameof(LizardTail.AfterPreventingDeath))]
    public static class LizardTailPatch {
        const string TypeName = "MegaCrit.Sts2.Core.Models.Relics.LizardTail";

        class TriggerState {
            public int BeforeHp { get; set; }
        }

        static void Prefix(LizardTail __instance, Creature creature, ref object __state) {
            try {
                if (__instance == null || creature == null || __instance.Owner?.Creature != creature) return;
                __state = new TriggerState { BeforeHp = creature.CurrentHp };
            } catch { }
        }

        static void Postfix(LizardTail __instance, Creature creature, Task __result, object __state) {
            try {
                var state = __state as TriggerState;
                if (state == null) return;

                if (__result == null) {
                    Count(creature, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(creature, state);
                    } catch { }
                });
            } catch { }
        }

        static void Count(Creature creature, TriggerState state) {
            try {
                var healed = Math.Max(0, creature.CurrentHp - state.BeforeHp);
                if (healed > 0) RelicTracker.AddAmountByType(TypeName, "HP Healed", healed);
            } catch { }
        }
    }
}
