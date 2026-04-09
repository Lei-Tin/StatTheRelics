using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Capture the modified hand draw decrease.
    [HarmonyPatch(typeof(BigMushroom), nameof(BigMushroom.ModifyHandDraw))]
    public static class BigMushroomPatch {
        static void Postfix(BigMushroom __instance, Player player, decimal cardsToDraw, ref decimal __result) {
            try {
                var reducedDraw = cardsToDraw - __result; // net cards removed by the relic
                if (reducedDraw > 0) {
                    RelicTracker.AddAmount(__instance, "Card Drawn Reduced", Convert.ToInt32(reducedDraw));
                }

                ModLog.Info($"BigMushroomPatch: player={player?.GetType().FullName ?? "null"}, baseCount={cardsToDraw}, result={__result}, reduced={reducedDraw}");
            } catch { }
        }
    }
}