using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(CharonsAshes), nameof(CharonsAshes.AfterCardExhausted))]
    public static class CharonsAshesPatch {
        [ThreadStatic] internal static CharonsAshes? Current;

        class CharonsAshesState {
            public bool Triggered { get; set; }
        }

        static void Prefix(CharonsAshes __instance, CardModel card, ref object __state) {
            try {
                if (__instance == null || card?.Owner != __instance.Owner) return;
                Current = __instance;
                __state = new CharonsAshesState { Triggered = true };
            } catch { }
        }

        static void Postfix(CharonsAshes __instance, Task __result, object __state) {
            try {
                Current = null;
                var state = __state as CharonsAshesState;
                if (state == null || !state.Triggered) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Cards Exhausted", 1);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) {
                        RelicTracker.AddAmount(__instance, "Cards Exhausted", 1);
                    }
                });
            } catch {
                Current = null;
            }
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(IEnumerable<Creature>),
        typeof(decimal),
        typeof(ValueProp),
        typeof(Creature),
        typeof(CardModel)
    })]
    public static class CharonsAshesDamagePatch {
        class CharonsAshesDamageState {
            public CharonsAshes? Relic { get; set; }
        }

        static void Prefix(ref object __state) {
            try {
                if (CharonsAshesPatch.Current == null) return;
                __state = new CharonsAshesDamageState { Relic = CharonsAshesPatch.Current };
            } catch { }
        }

        static void Postfix(Task<IEnumerable<DamageResult>> __result, object __state) {
            try {
                var state = __state as CharonsAshesDamageState;
                if (state?.Relic == null) return;
                if (__result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;

                        var total = 0;
                        foreach (var result in task.Result) {
                            if (result == null) continue;
                            total += Math.Max(0, result.TotalDamage - result.OverkillDamage);
                        }

                        if (total > 0) RelicTracker.AddAmount(state.Relic, "Damage Dealt", total);
                    } catch { }
                });
            } catch { }
        }
    }
}
