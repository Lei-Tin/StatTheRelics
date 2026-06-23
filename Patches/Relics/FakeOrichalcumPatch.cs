using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(FakeOrichalcum), nameof(FakeOrichalcum.BeforeSideTurnEnd))]
    public static class FakeOrichalcumPatch {
        class BlockState {
            public object? Creature { get; set; }
            public int Before { get; set; }
            public bool ShouldTrigger { get; set; }
        }

        static void Prefix(FakeOrichalcum __instance, PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ref object __state) {
            try {
                var creature = __instance?.Owner?.Creature;
                if (__instance == null || creature == null) return;
                __state = new BlockState {
                    Creature = creature,
                    Before = GetBlock(creature),
                    ShouldTrigger = ReflectionUtil.GetMemberValue(__instance, "ShouldTrigger") is bool shouldTrigger && shouldTrigger
                };
            } catch { }
        }

        static void Postfix(FakeOrichalcum __instance, Task __result, object __state) {
            try {
                var state = __state as BlockState;
                if (state == null || !state.ShouldTrigger) return;

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

        static void Count(FakeOrichalcum relic, BlockState state) {
            try {
                var creature = state.Creature ?? relic.Owner?.Creature;
                var gained = Math.Max(0, GetBlock(creature) - state.Before);
                if (gained > 0) RelicTracker.AddAmount(relic, "Block Gained", gained);
            } catch { }
        }

        static int GetBlock(object? creature) {
            try {
                var block = ReflectionUtil.GetMemberValue(creature, "Block")
                    ?? ReflectionUtil.GetMemberValue(creature, "CurrentBlock");
                return block == null ? 0 : Convert.ToInt32(block);
            } catch {
                return 0;
            }
        }
    }
}
