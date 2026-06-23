using System;
using System.Collections;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(BagOfMarbles), nameof(BagOfMarbles.BeforeSideTurnStart))]
    public static class BagOfMarblesPatch {
        class BagOfMarblesState {
            public int Vulnerable { get; set; }
        }

        static void Prefix(BagOfMarbles __instance, PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState, ref object __state) {
            try {
                if (__instance == null || combatState == null) return;
                if (side != __instance.Owner.Creature.Side) return;
                if (combatState.RoundNumber > 1) return;

                var enemies = ReflectionUtil.GetMemberValue(combatState, "HittableEnemies") as IEnumerable;
                var enemyCount = 0;
                if (enemies != null) {
                    foreach (var _ in enemies) enemyCount++;
                }

                var amount = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Vulnerable"));
                __state = new BagOfMarblesState {
                    Vulnerable = amount * enemyCount
                };
            } catch { }
        }

        static void Postfix(BagOfMarbles __instance, Task __result, object __state) {
            try {
                var state = __state as BagOfMarblesState;
                if (state == null || state.Vulnerable <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Vulnerable Applied", state.Vulnerable);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) {
                        RelicTracker.AddAmount(__instance, "Vulnerable Applied", state.Vulnerable);
                    }
                });
            } catch { }
        }
    }
}
