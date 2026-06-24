using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(LoomingFruit), nameof(LoomingFruit.AfterObtained))]
    public static class LoomingFruitPatch {
        class PickupState {
            public int BeforeMaxHp { get; set; }
        }

        static void Prefix(LoomingFruit __instance, ref object __state) {
            try {
                __state = new PickupState {
                    BeforeMaxHp = GetMaxHp(__instance.Owner?.Creature)
                };
            } catch { }
        }

        static void Postfix(LoomingFruit __instance, Task __result, object __state) {
            try {
                var state = __state as PickupState;
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

        static void Count(LoomingFruit relic, PickupState state) {
            try {
                var gained = Math.Max(0, GetMaxHp(relic.Owner?.Creature) - state.BeforeMaxHp);
                if (gained > 0) RelicTracker.AddAmount(relic, "Max HP Gained", gained);
            } catch { }
        }

        static int GetMaxHp(object? creature) {
            try {
                var raw = ReflectionUtil.GetMemberValue(creature, "MaxHp")
                    ?? ReflectionUtil.GetMemberValue(creature, "MaxHealth");
                return raw == null ? 0 : Convert.ToInt32(raw);
            } catch {
                return 0;
            }
        }
    }
}
