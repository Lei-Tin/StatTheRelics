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
    [HarmonyPatch(typeof(Metronome), nameof(Metronome.AfterOrbChanneled))]
    public static class MetronomePatch {
        [ThreadStatic] internal static Metronome? Current;

        static void Prefix(Metronome __instance, PlayerChoiceContext choiceContext, Player player, OrbModel orb) {
            try {
                _ = choiceContext;
                _ = orb;
                if (__instance == null || player == null || __instance.Owner != player) return;

                var orbCount = Math.Max(1, ReflectionUtil.GetDynamicVarIntValue(__instance, "OrbCount", 7));
                var orbsChanneled = ReflectionUtil.GetIntMemberValue(__instance, "_orbsChanneled");
                if (orbsChanneled + 1 != orbCount) return;

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
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(IEnumerable<Creature>),
        typeof(DamageVar),
        typeof(Creature)
    })]
    public static class MetronomeDamagePatch {
        class DamageState {
            public Metronome? Relic { get; set; }
        }

        static void Prefix(ref object __state) {
            try {
                if (MetronomePatch.Current == null) return;
                __state = new DamageState { Relic = MetronomePatch.Current };
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
