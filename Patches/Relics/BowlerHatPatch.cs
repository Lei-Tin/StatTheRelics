using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Track bonus gold granted by Bowler Hat from pending bonus state.
    [HarmonyPatch(typeof(BowlerHat), nameof(BowlerHat.AfterGoldGained))]
    public static class BowlerHatPatch {
        class BowlerHatState {
            public bool IsOwner { get; set; }
            public decimal PendingBonusGold { get; set; }
        }

        static void Prefix(BowlerHat __instance, Player player, ref object __state) {
            try {
                var pendingRaw = ReflectionUtil.GetMemberValue(__instance, "_pendingBonusGold");
                var pendingBonusGold = pendingRaw == null ? 0m : Convert.ToDecimal(pendingRaw);

                __state = new BowlerHatState {
                    IsOwner = __instance?.Owner != null && player == __instance.Owner,
                    PendingBonusGold = pendingBonusGold
                };

                ModLog.Info($"BowlerHatPatch: Prefix pendingBonusGold={pendingBonusGold}");
            } catch { }
        }

        static void Postfix(BowlerHat __instance, object __state) {
            try {
                var state = __state as BowlerHatState;
                if (state == null) return;
                if (!state.IsOwner) return;
                if (state.PendingBonusGold <= 0m) return;

                RelicTracker.AddAmount(__instance, "Gold Gained", Convert.ToInt32(state.PendingBonusGold));
                ModLog.Info($"BowlerHatPatch: Postfix added Gold Gained={state.PendingBonusGold}");
            } catch { }
        }
    }
}
