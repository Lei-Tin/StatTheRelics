using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ScreamingFlagon), nameof(ScreamingFlagon.BeforeSideTurnEnd))]
    public static class ScreamingFlagonPatch {
        static readonly object Sync = new();
        static ScreamingFlagon? activeRelic;

        static void Prefix(ScreamingFlagon __instance, IEnumerable<Creature> participants) {
            try {
                var owner = __instance?.Owner;
                if (__instance == null || owner == null || participants == null || !participants.Contains(owner.Creature)) return;
                var discardPile = PileType.Discard.GetPile(owner);
                if (discardPile == null || !discardPile.IsEmpty) return;
                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(ScreamingFlagon __instance, Task __result) {
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

        static void Clear(ScreamingFlagon relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static ScreamingFlagon? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(IEnumerable<Creature>),
        typeof(DamageVar),
        typeof(Creature)
    })]
    public static class ScreamingFlagonDamagePatch {
        class DamageState {
            public ScreamingFlagon? Relic { get; set; }
        }

        static void Prefix(Creature dealer, ref object __state) {
            try {
                var relic = ScreamingFlagonPatch.ActiveRelic ?? FindFromStack(dealer);
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

        static ScreamingFlagon? FindFromStack(Creature dealer) {
            try {
                if (dealer == null) return null;
                var relic = ReflectionUtil.FindRelic<ScreamingFlagon>(dealer)
                    ?? ReflectionUtil.FindRelic<ScreamingFlagon>(ReflectionUtil.GetMemberValue(dealer, "Owner"))
                    ?? ReflectionUtil.FindRelic<ScreamingFlagon>(ReflectionUtil.GetMemberValue(dealer, "Player"));
                if (relic == null || !IsScreamingFlagonStack()) return null;
                return relic;
            } catch {
                return null;
            }
        }

        static bool IsScreamingFlagonStack() {
            try {
                var frames = new StackTrace().GetFrames();
                if (frames == null) return false;
                foreach (var frame in frames) {
                    var typeName = frame.GetMethod()?.DeclaringType?.FullName;
                    if (typeName != null && typeName.IndexOf("ScreamingFlagon", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                }
            } catch { }

            return false;
        }
    }
}
