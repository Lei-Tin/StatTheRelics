using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(SturdyClamp), nameof(SturdyClamp.AfterPreventingBlockClear))]
    public static class SturdyClampPatch {
        static void Prefix(SturdyClamp __instance, AbstractModel preventer, Creature creature, ref object __state) {
            try {
                if (__instance == null || preventer != __instance || creature != __instance.Owner?.Creature) return;
                var block = Math.Max(0, creature.Block);
                if (block <= 0) return;

                __state = Math.Min(block, 10);
            } catch { }
        }

        static void Postfix(SturdyClamp __instance, Task __result, object __state) {
            try {
                if (__state is not int retained || retained <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Block Retained", retained);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Block Retained", retained);
                    } catch { }
                });
            } catch { }
        }
    }
}
