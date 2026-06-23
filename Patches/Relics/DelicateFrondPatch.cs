using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(DelicateFrond), nameof(DelicateFrond.BeforeCombatStart))]
    public static class DelicateFrondPatch {
        static DelicateFrond? Current;

        static void Prefix(DelicateFrond __instance) {
            Current = __instance;
        }

        static void Postfix(Task __result) {
            try {
                if (__result == null) {
                    Current = null;
                    return;
                }

                __result.ContinueWith(_ => Current = null);
            } catch {
                Current = null;
            }
        }

        internal static DelicateFrond? Active => Current;
    }

    [HarmonyPatch(typeof(PotionCmd), nameof(PotionCmd.TryToProcure), new Type[] {
        typeof(PotionModel),
        typeof(Player),
        typeof(int)
    })]
    public static class DelicateFrondPotionPatch {
        class PotionState {
            public DelicateFrond? Relic { get; set; }
        }

        static void Prefix(PotionModel potion, Player player, ref object __state) {
            try {
                var relic = DelicateFrondPatch.Active;
                if (relic == null || player != relic.Owner) return;
                __state = new PotionState { Relic = relic };
            } catch { }
        }

        static void Postfix(Task<PotionProcureResult> __result, object __state) {
            try {
                var state = __state as PotionState;
                if (state?.Relic == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
                        if (!task.Result.success) return;

                        RelicTracker.AddAmount(state.Relic, "Potions Given", 1);
                        PotionNameUtil.AppendPotionName(state.Relic, "Potions", PotionNameUtil.GetPotionName(task.Result.potion));
                    } catch { }
                });
            } catch { }
        }
    }
}
