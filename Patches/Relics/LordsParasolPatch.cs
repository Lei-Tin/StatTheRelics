using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(LordsParasol), "PurchaseEverything")]
    public static class LordsParasolPatch {
        static readonly object Sync = new();
        static LordsParasol? activeRelic;

        static void Prefix(LordsParasol __instance) {
            try {
                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(LordsParasol __instance, Task __result) {
            try {
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Shops Visited", 1);
                    Clear(__instance);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            RelicTracker.AddAmount(__instance, "Shops Visited", 1);
                        }
                        Clear(__instance);
                    } catch { }
                });
            } catch {
                Clear(__instance);
            }
        }

        static void Clear(LordsParasol relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static LordsParasol? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        internal static void CountPurchase(LordsParasol relic, int cost) {
            try {
                if (relic == null || cost <= 0) return;
                RelicTracker.AddAmount(relic, "Gold Value Purchased", cost);
            } catch { }
        }

        internal static void CountRemoval(LordsParasol relic, int cost) {
            try {
                if (relic == null || cost <= 0) return;
                RelicTracker.AddAmount(relic, "Gold Value Purchased", cost);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(MerchantEntry), nameof(MerchantEntry.OnTryPurchaseWrapper), new Type[] {
        typeof(MerchantInventory),
        typeof(bool)
    })]
    public static class LordsParasolMerchantEntryPatch {
        class PurchaseState {
            public LordsParasol? Relic { get; set; }
            public int Cost { get; set; }
        }

        static void Prefix(MerchantEntry __instance, ref object __state) {
            try {
                var relic = LordsParasolPatch.ActiveRelic;
                if (relic == null) return;
                __state = new PurchaseState { Relic = relic, Cost = Math.Max(0, __instance.Cost) };
            } catch { }
        }

        static void Postfix(Task<bool> __result, object __state) {
            try {
                var state = __state as PurchaseState;
                if (state?.Relic == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion && task.Result) {
                            LordsParasolPatch.CountPurchase(state.Relic, state.Cost);
                        }
                    } catch { }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(MerchantCardRemovalEntry), nameof(MerchantCardRemovalEntry.OnTryPurchaseWrapper), new Type[] {
        typeof(MerchantInventory),
        typeof(bool),
        typeof(bool)
    })]
    public static class LordsParasolCardRemovalPatch {
        class PurchaseState {
            public LordsParasol? Relic { get; set; }
            public int Cost { get; set; }
        }

        static void Prefix(MerchantCardRemovalEntry __instance, ref object __state) {
            try {
                var relic = LordsParasolPatch.ActiveRelic;
                if (relic == null) return;
                __state = new PurchaseState { Relic = relic, Cost = Math.Max(0, __instance.Cost) };
            } catch { }
        }

        static void Postfix(Task<bool> __result, object __state) {
            try {
                var state = __state as PurchaseState;
                if (state?.Relic == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion && task.Result) {
                            LordsParasolPatch.CountRemoval(state.Relic, state.Cost);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
