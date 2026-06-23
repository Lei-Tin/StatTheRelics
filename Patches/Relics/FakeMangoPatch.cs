using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(FakeMango), nameof(FakeMango.AfterObtained))]
    public static class FakeMangoPatch {
        class MaxHpState {
            public object? Creature { get; set; }
            public int Before { get; set; }
            public int Expected { get; set; }
        }

        static void Prefix(FakeMango __instance, ref object __state) {
            try {
                var creature = __instance?.Owner?.Creature;
                __state = new MaxHpState {
                    Creature = creature,
                    Before = GetMaxHp(creature),
                    Expected = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "MaxHp", 3))
                };
            } catch { }
        }

        static void Postfix(FakeMango __instance, Task __result, object __state) {
            try {
                var state = __state as MaxHpState;
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

        static void Count(FakeMango relic, MaxHpState state) {
            try {
                var creature = state.Creature ?? relic.Owner?.Creature;
                var gained = Math.Max(0, GetMaxHp(creature) - state.Before);
                if (gained <= 0) gained = state.Expected;
                if (gained > 0) RelicTracker.AddAmount(relic, "Max HP Gained", gained);
            } catch { }
        }

        static int GetMaxHp(object? creature) {
            try {
                var maxHp = ReflectionUtil.GetMemberValue(creature, "MaxHp");
                return maxHp == null ? 0 : Convert.ToInt32(maxHp);
            } catch {
                return 0;
            }
        }
    }
}
