using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PetrifiedToad), nameof(PetrifiedToad.BeforeCombatStartLate))]
    public static class PetrifiedToadPatch {
        static readonly object Sync = new();
        static PetrifiedToad? activeRelic;

        internal static PetrifiedToad? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        static void Prefix(PetrifiedToad __instance) {
            lock (Sync) activeRelic = __instance;
        }

        static void Postfix(PetrifiedToad __instance, Task __result) {
            try {
                if (__result == null) {
                    Clear(__instance);
                    return;
                }

                __result.ContinueWith(_ => {
                    try { Clear(__instance); } catch { }
                });
            } catch { }
        }

        static void Clear(PetrifiedToad relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }
    }

    [HarmonyPatch(typeof(PotionCmd), nameof(PotionCmd.TryToProcure), new Type[] {
        typeof(PotionModel),
        typeof(Player),
        typeof(int)
    })]
    public static class PetrifiedToadPotionProcurePatch {
        class State {
            public PetrifiedToad? Relic { get; set; }
        }

        static void Prefix(Player player, ref object __state) {
            try {
                var relic = PetrifiedToadPatch.ActiveRelic;
                if (relic == null || player != relic.Owner) return;
                __state = new State { Relic = relic };
            } catch { }
        }

        static void Postfix(Task<PotionProcureResult> __result, object __state) {
            try {
                if (__state is not State state || state.Relic == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null || !task.Result.success) return;
                        RelicTracker.AddAmount(state.Relic, "Potions Obtained", 1);
                    } catch { }
                });
            } catch { }
        }
    }
}
