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
    [HarmonyPatch(typeof(RoyalPoison), nameof(RoyalPoison.AfterPlayerTurnStart))]
    public static class RoyalPoisonTurnStartPatch {
        static readonly object Sync = new();
        static RoyalPoison? activeRelic;

        static void Prefix(RoyalPoison __instance) {
            try {
                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(RoyalPoison __instance, Task __result) {
            try {
                if (__result == null) {
                    Clear(__instance);
                    return;
                }

                __result.ContinueWith(_ => {
                    try { Clear(__instance); } catch { }
                });
            } catch {
                Clear(__instance);
            }
        }

        static void Clear(RoyalPoison relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static RoyalPoison? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(Creature),
        typeof(DamageVar),
        typeof(Creature),
        typeof(CardModel)
    })]
    public static class RoyalPoisonDamagePatch {
        class DamageState {
            public RoyalPoison? Relic { get; set; }
        }

        static void Prefix(Creature target, ref object __state) {
            try {
                var relic = RoyalPoisonTurnStartPatch.ActiveRelic;
                if (relic == null || target == null || relic.Owner?.Creature != target) return;
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

                        if (total > 0) RelicTracker.AddAmount(state.Relic, "Damage Taken", total);
                    } catch { }
                });
            } catch { }
        }
    }
}
