using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(MeatOnTheBone), nameof(MeatOnTheBone.AfterCombatVictoryEarly))]
    public static class MeatOnTheBonePatch {
        [ThreadStatic] internal static MeatOnTheBone? Current;

        static void Prefix(MeatOnTheBone __instance, CombatRoom _) {
            try {
                if (__instance?.Owner?.Creature == null) return;
                if (__instance.Owner.Creature.IsDead) return;
                if (!WillHealOnCombatFinished(__instance)) return;
                Current = __instance;
            } catch { }
        }

        static void Postfix(Task __result) {
            try {
                _ = __result;
                Current = null;
            } catch {
                Current = null;
            }
        }

        static bool WillHealOnCombatFinished(MeatOnTheBone relic) {
            try {
                var creature = relic.Owner?.Creature;
                if (creature == null) return false;
                var threshold = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(relic, "HpThreshold", 50));
                var hpLimit = (int)(creature.MaxHp * threshold / 100m);
                return creature.CurrentHp <= hpLimit;
            } catch {
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Heal), new Type[] {
        typeof(Creature),
        typeof(decimal),
        typeof(bool)
    })]
    public static class MeatOnTheBoneHealPatch {
        class HealState {
            public MeatOnTheBone? Relic { get; set; }
            public int ExpectedHeal { get; set; }
        }

        static void Prefix(Creature creature, decimal amount, ref object __state) {
            try {
                var relic = MeatOnTheBonePatch.Current;
                if (relic == null || creature == null || relic.Owner?.Creature != creature) return;
                __state = new HealState {
                    Relic = relic,
                    ExpectedHeal = Math.Max(0, Math.Min((int)amount, creature.MaxHp - creature.CurrentHp))
                };
            } catch { }
        }

        static void Postfix(Task __result, object __state) {
            try {
                var state = __state as HealState;
                if (state?.Relic == null || state.ExpectedHeal <= 0 || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            RelicTracker.AddAmount(state.Relic, "HP Healed", state.ExpectedHeal);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
