using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Pantograph), nameof(Pantograph.BeforeCombatStart))]
    public static class PantographPatch {
        class State {
            public int BeforeHp { get; set; }
        }

        static void Prefix(Pantograph __instance, ref object __state) {
            try {
                if (__instance?.Owner?.Creature == null) return;
                __state = new State { BeforeHp = __instance.Owner.Creature.CurrentHp };
            } catch { }
        }

        static void Postfix(Pantograph __instance, Task __result, object __state) {
            try {
                if (__state is not State state) return;
                if (__result == null) {
                    Count(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, state);
                    } catch { }
                });
            } catch { }
        }

        static void Count(Pantograph relic, State state) {
            try {
                var healed = Math.Max(0, relic.Owner.Creature.CurrentHp - state.BeforeHp);
                if (healed > 0) RelicTracker.AddAmount(relic, "HP Healed", healed);
            } catch { }
        }
    }
}
