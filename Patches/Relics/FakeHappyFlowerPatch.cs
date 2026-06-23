using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(FakeHappyFlower), nameof(FakeHappyFlower.AfterSideTurnStart))]
    public static class FakeHappyFlowerPatch {
        class FakeHappyFlowerState {
            public int Energy { get; set; }
            public bool ShouldTrigger { get; set; }
        }

        static void Prefix(FakeHappyFlower __instance, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ref object __state) {
            try {
                var ownerCreature = __instance?.Owner?.Creature;
                if (__instance == null || ownerCreature == null || participants == null || !participants.Contains(ownerCreature)) return;

                var turns = Math.Max(1, ReflectionUtil.GetDynamicVarIntValue(__instance, "Turns", 5));
                var before = ReflectionUtil.GetIntMemberValue(__instance, "TurnsSeen", 0);
                __state = new FakeHappyFlowerState {
                    Energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Energy", 1)),
                    ShouldTrigger = ((before + 1) % turns) == 0
                };
            } catch { }
        }

        static void Postfix(FakeHappyFlower __instance, Task __result, object __state) {
            try {
                var state = __state as FakeHappyFlowerState;
                if (state == null || !state.ShouldTrigger || state.Energy <= 0) return;

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

        static void Count(FakeHappyFlower relic, FakeHappyFlowerState state) {
            try {
                RelicTracker.AddAmount(relic, "Energy Gained", state.Energy);
            } catch { }
        }
    }
}
