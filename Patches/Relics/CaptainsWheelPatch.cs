using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(CaptainsWheel), nameof(CaptainsWheel.AfterBlockCleared))]
    public static class CaptainsWheelPatch {
        class CaptainsWheelState {
            public int Block { get; set; }
        }

        static void Prefix(CaptainsWheel __instance, Creature creature, ref object __state) {
            try {
                var owner = __instance?.Owner;
                if (__instance == null || owner?.Creature == null || creature != owner.Creature) return;
                if (owner.PlayerCombatState == null || owner.PlayerCombatState.TurnNumber != 3) return;

                __state = new CaptainsWheelState {
                    Block = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Block"))
                };
            } catch { }
        }

        static void Postfix(CaptainsWheel __instance, Task __result, object __state) {
            try {
                var state = __state as CaptainsWheelState;
                if (state == null || state.Block <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Block Gained", state.Block);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) {
                        RelicTracker.AddAmount(__instance, "Block Gained", state.Block);
                    }
                });
            } catch { }
        }
    }
}
