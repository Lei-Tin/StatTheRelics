using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Entities.Players;

namespace StatTheRelics.Patches.Relics {
    // Capture the modified hand draw
    [HarmonyPatch(typeof(RingOfTheSnake), nameof(RingOfTheSnake.ModifyHandDraw))]
    public static class RingOfTheSnakePatch {
        static void Postfix(RingOfTheSnake __instance, Player player, decimal count, ref decimal __result) {
            try {
                var extraDraw = __result - count; // net cards added by the relic
                if (extraDraw > 0) {
                    RelicTracker.AddAmount(__instance, "Cards Drawn", Convert.ToInt32(extraDraw));
                }
            } catch { }
        }
    }
}
