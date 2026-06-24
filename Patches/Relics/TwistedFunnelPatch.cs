using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(TwistedFunnel), nameof(TwistedFunnel.BeforeSideTurnStart))]
    public static class TwistedFunnelPatch {
        class State {
            public int Poison { get; set; }
        }

        static void Prefix(TwistedFunnel __instance, PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ref object __state) {
            try {
                _ = choiceContext;
                _ = side;
                _ = combatState;
                if (__instance?.Owner?.Creature == null || participants == null) return;
                if (!participants.Contains(__instance.Owner.Creature)) return;
                if (__instance.Owner.PlayerCombatState?.TurnNumber > 1) return;

                var enemies = __instance.Owner.Creature.CombatState?.HittableEnemies;
                var count = enemies?.Count() ?? 0;
                var poisonPerEnemy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "PoisonPower", 4));
                if (count <= 0 || poisonPerEnemy <= 0) return;

                __state = new State { Poison = count * poisonPerEnemy };
            } catch { }
        }

        static void Postfix(TwistedFunnel __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.Poison <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Poison Applied", state.Poison);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Poison Applied", state.Poison);
                    } catch { }
                });
            } catch { }
        }
    }
}
