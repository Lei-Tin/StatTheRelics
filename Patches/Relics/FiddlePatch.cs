using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Fiddle), nameof(Fiddle.ModifyHandDrawLate))]
    public static class FiddlePatch {
        static void Postfix(Fiddle __instance, Player player, decimal count, decimal __result) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                var extra = Math.Max(0, Convert.ToInt32(__result - count));
                if (extra > 0) RelicTracker.AddAmount(__instance, "Extra Draws", extra);
            } catch { }
        }
    }
}
