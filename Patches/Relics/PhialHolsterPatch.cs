using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PhialHolster), nameof(PhialHolster.AfterObtained))]
    public static class PhialHolsterPatch {
        static readonly object Sync = new();
        static PhialHolster? activeRelic;

        internal static PhialHolster? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        static void Prefix(PhialHolster __instance, ref object __state) {
            try {
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "PotionSlots", 1));
                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(PhialHolster __instance, Task __result, object __state) {
            try {
                if (__result == null) {
                    CountSlots(__instance, __state);
                    Clear(__instance);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) CountSlots(__instance, __state);
                        Clear(__instance);
                    } catch { }
                });
            } catch { }
        }

        static void CountSlots(PhialHolster relic, object state) {
            try {
                if (state is int slots && slots > 0) RelicTracker.AddAmount(relic, "Potion Slots Gained", slots);
            } catch { }
        }

        static void Clear(PhialHolster relic) {
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
    public static class PhialHolsterPotionProcurePatch {
        class State {
            public PhialHolster? Relic { get; set; }
        }

        static void Prefix(Player player, ref object __state) {
            try {
                var relic = PhialHolsterPatch.ActiveRelic;
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
                        PotionNameUtil.AppendPotionName(state.Relic, "Potions", PotionNameUtil.GetPotionName(task.Result.potion));
                    } catch { }
                });
            } catch { }
        }
    }
}
