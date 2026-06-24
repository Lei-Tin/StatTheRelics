using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(SparklingRouge), nameof(SparklingRouge.AfterBlockCleared))]
    public static class SparklingRougePatch {
        class TriggerState {
            public int Strength { get; set; }
            public int Dexterity { get; set; }
        }

        static void Prefix(SparklingRouge __instance, Creature creature, ref object __state) {
            try {
                if (__instance?.Owner?.Creature == null || creature != __instance.Owner.Creature) return;
                if (__instance.Owner.PlayerCombatState?.TurnNumber != 3) return;

                __state = new TriggerState {
                    Strength = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Strength", 1)),
                    Dexterity = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Dexterity", 1))
                };
            } catch { }
        }

        static void Postfix(SparklingRouge __instance, Task __result, object __state) {
            try {
                if (__state is not TriggerState state) return;

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

        static void Count(SparklingRouge relic, TriggerState state) {
            try {
                if (state.Strength > 0) RelicTracker.AddAmount(relic, "Strength Gained", state.Strength);
                if (state.Dexterity > 0) RelicTracker.AddAmount(relic, "Dexterity Gained", state.Dexterity);
            } catch { }
        }
    }
}
