using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PaelsBlood), nameof(PaelsBlood.ModifyHandDraw))]
    public static class PaelsBloodPatch {
        static void Postfix(PaelsBlood __instance, Player player, decimal count, ref decimal __result) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                var extra = __result - count;
                if (extra > 0) RelicTracker.AddAmount(__instance, "Cards Drawn", Convert.ToInt32(extra));
            } catch { }
        }
    }
}
