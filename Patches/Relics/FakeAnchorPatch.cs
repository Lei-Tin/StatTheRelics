using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(FakeAnchor), nameof(FakeAnchor.BeforeCombatStart))]
    public static class FakeAnchorPatch {
        class BlockState {
            public object? Creature { get; set; }
            public int Before { get; set; }
        }

        static void Prefix(FakeAnchor __instance, ref object __state) {
            try {
                var creature = __instance?.Owner?.Creature;
                __state = new BlockState { Creature = creature, Before = GetBlock(creature) };
            } catch { }
        }

        static void Postfix(FakeAnchor __instance, Task __result, object __state) {
            try {
                var state = __state as BlockState;
                if (state == null) return;

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

        static void Count(FakeAnchor relic, BlockState state) {
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
