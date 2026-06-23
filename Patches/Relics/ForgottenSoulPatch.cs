using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ForgottenSoul), nameof(ForgottenSoul.AfterCardExhausted))]
    public static class ForgottenSoulPatch {
        [ThreadStatic] internal static ForgottenSoul? Current;

        class ExhaustState {
            public bool Triggered { get; set; }
        }

        static void Prefix(ForgottenSoul __instance, CardModel card, ref object __state) {
            try {
                if (__instance == null || card?.Owner != __instance.Owner) return;
                Current = __instance;
                __state = new ExhaustState { Triggered = true };
            } catch { }
        }

        static void Postfix(ForgottenSoul __instance, Task __result, object __state) {
            try {
                Current = null;
                var state = __state as ExhaustState;
                if (state == null || !state.Triggered) return;

                _ = __result;
            } catch {
                Current = null;
            }
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(Creature),
        typeof(DamageVar),
        typeof(Creature)
    })]
    public static class ForgottenSoulDamagePatch {
        class DamageState {
            public ForgottenSoul? Relic { get; set; }
        }

        static void Prefix(ref object __state) {
            try {
                if (ForgottenSoulPatch.Current == null) return;
                __state = new DamageState { Relic = ForgottenSoulPatch.Current };
            } catch { }
        }

        static void Postfix(Task<IEnumerable<DamageResult>> __result, object __state) {
            try {
                var state = __state as DamageState;
                if (state?.Relic == null || __result == null) return;

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
