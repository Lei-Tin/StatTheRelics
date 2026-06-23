using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Candelabra), nameof(Candelabra.AfterSideTurnStart))]
    public static class CandelabraPatch {
        class CandelabraState {
            public int Energy { get; set; }
        }

        static void Prefix(Candelabra __instance, IReadOnlyList<Creature> participants, ref object __state) {
            try {
                var owner = __instance?.Owner;
                var ownerCreature = owner?.Creature;
                if (__instance == null || owner == null || ownerCreature == null || participants == null) return;
                if (!participants.Contains(ownerCreature)) return;
                if (owner.PlayerCombatState == null || owner.PlayerCombatState.TurnNumber != 2) return;

                __state = new CandelabraState {
                    Energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Energy"))
                };
            } catch { }
        }

        static void Postfix(Candelabra __instance, Task __result, object __state) {
            try {
                var state = __state as CandelabraState;
                if (state == null || state.Energy <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Energy Gained", state.Energy);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) {
                        RelicTracker.AddAmount(__instance, "Energy Gained", state.Energy);
                    }
                });
            } catch { }
        }
    }
}
