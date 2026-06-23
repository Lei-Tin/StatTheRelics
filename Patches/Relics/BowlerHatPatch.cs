using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    // Track only the extra gold granted by Bowler Hat.
    [HarmonyPatch(typeof(BowlerHat), nameof(BowlerHat.ModifyGoldGained))]
    public static class BowlerHatPatch {
        static void Postfix(BowlerHat __instance, Player player, decimal amount, decimal __result) {
            if (__instance?.Owner == null || player != __instance.Owner) return;

            var bonusGold = __result - amount;
            if (bonusGold <= 0m) return;

            RelicTracker.AddAmount(__instance, "Gold Gained", decimal.ToInt32(bonusGold));
        }
    }
}
