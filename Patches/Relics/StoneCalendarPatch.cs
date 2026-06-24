using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(StoneCalendar), nameof(StoneCalendar.BeforeSideTurnEnd))]
    public static class StoneCalendarPatch {
        [ThreadStatic] internal static StoneCalendar? Current;

        static void Prefix(StoneCalendar __instance, IEnumerable<Creature> participants) {
            try {
                if (__instance?.Owner?.Creature == null || participants == null) return;

                var containsOwner = false;
                foreach (var participant in participants) {
                    if (participant == __instance.Owner.Creature) {
                        containsOwner = true;
                        break;
                    }
                }

                if (!containsOwner) return;

                var damageTurn = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "DamageTurn", 7));
                if (__instance.Owner.PlayerCombatState?.TurnNumber != damageTurn) return;

                Current = __instance;
            } catch { }
        }

        static void Postfix() {
            Current = null;
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(IEnumerable<Creature>),
        typeof(DamageVar),
        typeof(Creature)
    })]
    public static class StoneCalendarDamagePatch {
        class DamageState {
            public StoneCalendar? Relic { get; set; }
        }

        static void Prefix(Creature dealer, ref object __state) {
            try {
                var relic = StoneCalendarPatch.Current;
                if (relic == null || dealer == null || relic.Owner?.Creature != dealer) return;
                __state = new DamageState { Relic = relic };
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
                            total += Math.Max(0, result.TotalDamage);
                        }

                        if (total > 0) RelicTracker.AddAmount(state.Relic, "Damage Dealt", total);
                    } catch { }
                });
            } catch { }
        }
    }
}
