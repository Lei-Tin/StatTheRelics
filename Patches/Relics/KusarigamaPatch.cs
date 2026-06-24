using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Kusarigama), nameof(Kusarigama.AfterCardPlayed))]
    public static class KusarigamaPatch {
        [ThreadStatic] internal static Kusarigama? Current;

        static void Prefix(Kusarigama __instance) {
            try {
                Current = __instance;
            } catch { }
        }

        static void Postfix() {
            Current = null;
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(Creature),
        typeof(DamageVar),
        typeof(Creature)
    })]
    public static class KusarigamaDamagePatch {
        class DamageState {
            public Kusarigama? Relic { get; set; }
        }

        static void Prefix(Creature dealer, ref object __state) {
            try {
                var relic = KusarigamaPatch.Current;
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
