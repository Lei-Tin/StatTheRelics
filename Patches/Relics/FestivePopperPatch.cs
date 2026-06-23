using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(FestivePopper), nameof(FestivePopper.AfterPlayerTurnStart))]
    public static class FestivePopperPatch {
        [ThreadStatic] internal static FestivePopper? Current;

        class TriggerState {
            public bool Triggered { get; set; }
        }

        static void Prefix(FestivePopper __instance, PlayerChoiceContext choiceContext, Player player, ref object __state) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                if (__instance.Owner.PlayerCombatState?.TurnNumber != 1) return;

                Current = __instance;
                __state = new TriggerState { Triggered = true };
            } catch { }
        }

        static void Postfix(FestivePopper __instance, Task __result, object __state) {
            try {
                Current = null;
                var state = __state as TriggerState;
                if (state == null || !state.Triggered) return;

                _ = __result;
            } catch {
                Current = null;
            }
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(IEnumerable<Creature>),
        typeof(DamageVar),
        typeof(Creature)
    })]
    public static class FestivePopperDamagePatch {
        class DamageState {
            public FestivePopper? Relic { get; set; }
        }

        static void Prefix(ref object __state) {
            try {
                if (FestivePopperPatch.Current == null) return;
                __state = new DamageState { Relic = FestivePopperPatch.Current };
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
