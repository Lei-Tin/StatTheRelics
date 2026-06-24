using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(HornCleat), nameof(HornCleat.AfterBlockCleared))]
    public static class HornCleatPatch {
        class BlockState {
            public int Block { get; set; }
        }

        static void Prefix(HornCleat __instance, Creature creature, ref object __state) {
            try {
                if (__instance?.Owner?.Creature == null || creature != __instance.Owner.Creature) return;
                if (__instance.Owner.PlayerCombatState?.TurnNumber != 2) return;
                __state = new BlockState {
                    Block = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Block", 14))
                };
            } catch { }
        }

        static void Postfix(HornCleat __instance, Task __result, object __state) {
            try {
                var state = __state as BlockState;
                if (state == null || state.Block <= 0) return;

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

        static void Count(HornCleat relic, BlockState state) {
            RelicTracker.AddAmount(relic, "Block Gained", state.Block);
        }
    }
}
