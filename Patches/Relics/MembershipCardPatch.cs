using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(MembershipCard), nameof(MembershipCard.ModifyMerchantPrice))]
    public static class MembershipCardPatch {
        static readonly object Sync = new();
        static readonly ConditionalWeakTable<MerchantEntry, DiscountRef> Discounts = new();

        internal class DiscountRef {
            public MembershipCard? Relic { get; set; }
            public int Saved { get; set; }
        }

        static void Postfix(MembershipCard __instance, Player player, MerchantEntry entry, decimal originalPrice, ref decimal __result) {
            try {
                if (__instance == null || player == null || entry == null || __instance.Owner != player) return;
                var saved = (int)Math.Max(0, Math.Round(originalPrice - __result, MidpointRounding.AwayFromZero));
                if (saved <= 0) return;

                lock (Sync) {
                    Discounts.Remove(entry);
                    Discounts.Add(entry, new DiscountRef { Relic = __instance, Saved = saved });
                }
            } catch { }
        }

        static internal DiscountRef? TakeDiscount(MerchantEntry entry) {
            try {
                lock (Sync) {
                    if (!Discounts.TryGetValue(entry, out var discount)) return null;
                    Discounts.Remove(entry);
                    return discount;
                }
            } catch {
                return null;
            }
        }
    }

    [HarmonyPatch(typeof(MerchantEntry), nameof(MerchantEntry.OnTryPurchaseWrapper), new Type[] {
        typeof(MerchantInventory),
        typeof(bool)
    })]
    public static class MembershipCardMerchantEntryPatch {
        class PurchaseState {
            public MerchantEntry? Entry { get; set; }
        }

        static void Prefix(MerchantEntry __instance, ref object __state) {
            try {
                __state = new PurchaseState { Entry = __instance };
            } catch { }
        }

        static void Postfix(Task<bool> __result, object __state) {
            try {
                var state = __state as PurchaseState;
                if (state?.Entry == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || !task.Result) return;
                        var discount = MembershipCardPatch.TakeDiscount(state.Entry);
                        if (discount?.Relic == null || discount.Saved <= 0) return;
                        RelicTracker.AddAmount(discount.Relic, "Gold Saved", discount.Saved);
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
    public static class MembershipCardRemovalEntryPatch {
        class PurchaseState {
            public MerchantEntry? Entry { get; set; }
        }

        static void Prefix(MerchantCardRemovalEntry __instance, ref object __state) {
            try {
                __state = new PurchaseState { Entry = __instance };
            } catch { }
        }

        static void Postfix(Task<bool> __result, object __state) {
            try {
                var state = __state as PurchaseState;
                if (state?.Entry == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || !task.Result) return;
                        var discount = MembershipCardPatch.TakeDiscount(state.Entry);
                        if (discount?.Relic == null || discount.Saved <= 0) return;
                        RelicTracker.AddAmount(discount.Relic, "Gold Saved", discount.Saved);
                    } catch { }
                });
            } catch { }
        }
    }
}
