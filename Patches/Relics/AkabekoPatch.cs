using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Akabeko), nameof(Akabeko.AfterSideTurnStart))]
    public static class AkabekoPatch {
        class AkabekoState {
            public int Vigor { get; set; }
        }

        static void Prefix(Akabeko __instance, CombatSide side, CombatState combatState, ref object __state) {
            try {
                if (__instance == null || combatState == null) return;
                if (side != __instance.Owner.Creature.Side) return;
                if (combatState.RoundNumber > 1) return;

                __state = new AkabekoState {
                    Vigor = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "VigorPower"))
                };
            } catch { }
        }

        static void Postfix(Akabeko __instance, Task __result, object __state) {
            try {
                var state = __state as AkabekoState;
                if (state == null || state.Vigor <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Vigor Given", state.Vigor);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) {
                        RelicTracker.AddAmount(__instance, "Vigor Given", state.Vigor);
                    }
                });
            } catch { }
        }
    }
}
