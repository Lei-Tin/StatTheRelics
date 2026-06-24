using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(RingOfTheDrake), nameof(RingOfTheDrake.ModifyHandDraw))]
    public static class RingOfTheDrakePatch {
        static void Postfix(RingOfTheDrake __instance, Player player, decimal count, ref decimal __result) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                var extraDraw = __result - count;
                if (extraDraw > 0) RelicTracker.AddAmount(__instance, "Cards Drawn", Convert.ToInt32(extraDraw));
            } catch { }
        }
    }
}
