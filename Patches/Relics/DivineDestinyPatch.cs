using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(DivineDestiny), nameof(DivineDestiny.AfterSideTurnStart))]
    public static class DivineDestinyPatch {
        class DivineDestinyState {
            public int Stars { get; set; }
        }

        static void Prefix(DivineDestiny __instance, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ref object __state) {
            try {
                var owner = __instance?.Owner;
                var ownerCreature = owner?.Creature;
                if (__instance == null || owner == null || ownerCreature == null || participants == null) return;
                if (!participants.Contains(ownerCreature)) return;
                if (owner.PlayerCombatState == null || owner.PlayerCombatState.TurnNumber > 1) return;

                __state = new DivineDestinyState {
                    Stars = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Stars", 6))
                };
            } catch { }
        }

        static void Postfix(DivineDestiny __instance, Task __result, object __state) {
            try {
                var state = __state as DivineDestinyState;
                if (state == null || state.Stars <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Stars Gained", state.Stars);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            RelicTracker.AddAmount(__instance, "Stars Gained", state.Stars);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
