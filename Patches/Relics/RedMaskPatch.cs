using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(RedMask), nameof(RedMask.BeforeSideTurnStart))]
    public static class RedMaskPatch {
        class State {
            public int Weak { get; set; }
        }

        static void Prefix(RedMask __instance, PlayerChoiceContext choiceContext, IReadOnlyList<Creature> participants, ICombatState combatState, ref object __state) {
            try {
                var owner = __instance?.Owner;
                var creature = owner?.Creature;
                if (owner == null || creature == null || participants == null || combatState == null) return;
                if (!participants.Contains(creature)) return;
                var turnNumber = ReflectionUtil.GetIntMemberValue(owner.PlayerCombatState, "TurnNumber", int.MaxValue);
                if (turnNumber > 1) return;

                var enemies = combatState.HittableEnemies as IEnumerable;
                var enemyCount = 0;
                if (enemies != null) {
                    foreach (var _ in enemies) enemyCount++;
                }

                var weakPerEnemy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "WeakPower", 1));
                __state = new State { Weak = weakPerEnemy * enemyCount };
            } catch { }
        }

        static void Postfix(RedMask __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.Weak <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Weak Applied", state.Weak);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Weak Applied", state.Weak);
                    } catch { }
                });
            } catch { }
        }
    }
}
