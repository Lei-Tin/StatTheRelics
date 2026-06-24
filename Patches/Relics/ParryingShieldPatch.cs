using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ParryingShield), nameof(ParryingShield.AfterSideTurnEnd))]
    public static class ParryingShieldPatch {
        class State {
            public bool WillTrigger { get; set; }
        }

        [ThreadStatic] internal static ParryingShield? Current;

        static void Prefix(ParryingShield __instance, PlayerChoiceContext choiceContext, object side, IEnumerable<Creature> participants, ref object __state) {
            try {
                _ = choiceContext;
                _ = side;
                if (__instance?.Owner?.Creature == null || participants == null) return;
                if (!participants.Contains(__instance.Owner.Creature)) return;
                var blockNeeded = ReflectionUtil.GetDynamicVarIntValue(__instance, "Block", 10);
                if (__instance.Owner.Creature.Block < blockNeeded) return;

                Current = __instance;
                __state = new State { WillTrigger = true };
            } catch { }
        }

        static void Postfix(Task __result, object __state) {
            try {
                _ = __result;
                _ = __state;
                Current = null;
            } catch {
                Current = null;
            }
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Commands.CreatureCmd), nameof(MegaCrit.Sts2.Core.Commands.CreatureCmd.Damage), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(Creature),
        typeof(MegaCrit.Sts2.Core.Localization.DynamicVars.DamageVar),
        typeof(Creature)
    })]
    public static class ParryingShieldDamagePatch {
        class DamageState {
            public ParryingShield? Relic { get; set; }
        }

        static void Prefix(ref object __state) {
            try {
                if (ParryingShieldPatch.Current == null) return;
                __state = new DamageState { Relic = ParryingShieldPatch.Current };
            } catch { }
        }

        static void Postfix(Task<IEnumerable<DamageResult>> __result, object __state) {
            try {
                if (__state is not DamageState state || state.Relic == null || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
                        var total = 0;
                        foreach (var result in task.Result) {
                            if (result == null) continue;
                            total += Math.Max(0, result.TotalDamage);
                        }

                        RelicTracker.AddAmount(state.Relic, "Times Triggered", 1);
                        if (total > 0) RelicTracker.AddAmount(state.Relic, "Damage Dealt", total);
                    } catch { }
                });
            } catch { }
        }
    }
}
