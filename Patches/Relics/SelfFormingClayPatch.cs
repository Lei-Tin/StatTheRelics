using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(SelfFormingClay), nameof(SelfFormingClay.AfterDamageReceived))]
    public static class SelfFormingClayPatch {
        static void Prefix(SelfFormingClay __instance, Creature target, DamageResult result, ref object __state) {
            try {
                var ownerCreature = __instance?.Owner?.Creature;
                if (__instance == null || ownerCreature == null || target != ownerCreature || result == null) return;
                if (result.UnblockedDamage <= 0) return;

                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "BlockNextTurn", 3));
            } catch { }
        }

        static void Postfix(SelfFormingClay __instance, Task __result, object __state) {
            try {
                if (__state is not int amount || amount <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Next Turn Block Gained", amount);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Next Turn Block Gained", amount);
                    } catch { }
                });
            } catch { }
        }
    }
}
