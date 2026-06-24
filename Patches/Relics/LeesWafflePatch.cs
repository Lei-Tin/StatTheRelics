using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(LeesWaffle), nameof(LeesWaffle.AfterObtained))]
    public static class LeesWafflePatch {
        class PickupState {
            public int BeforeHp { get; set; }
            public int BeforeMaxHp { get; set; }
        }

        static void Prefix(LeesWaffle __instance, ref object __state) {
            try {
                var creature = __instance.Owner?.Creature;
                __state = new PickupState {
                    BeforeHp = GetCurrentHp(creature),
                    BeforeMaxHp = GetMaxHp(creature)
                };
            } catch { }
        }

        static void Postfix(LeesWaffle __instance, Task __result, object __state) {
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

        static void Count(LeesWaffle relic, PickupState state) {
            try {
                var creature = relic.Owner?.Creature;
                var maxHpGained = Math.Max(0, GetMaxHp(creature) - state.BeforeMaxHp);
                var hpHealed = Math.Max(0, GetCurrentHp(creature) - state.BeforeHp);

                if (maxHpGained > 0) RelicTracker.AddAmount(relic, "Max HP Gained", maxHpGained);
                if (hpHealed > 0) RelicTracker.AddAmount(relic, "HP Healed", hpHealed);
            } catch { }
        }

        static int GetCurrentHp(object? creature) {
            try {
                var raw = ReflectionUtil.GetMemberValue(creature, "CurrentHp")
                    ?? ReflectionUtil.GetMemberValue(creature, "Hp")
                    ?? ReflectionUtil.GetMemberValue(creature, "Health");
                return raw == null ? 0 : Convert.ToInt32(raw);
            } catch {
                return 0;
            }
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
