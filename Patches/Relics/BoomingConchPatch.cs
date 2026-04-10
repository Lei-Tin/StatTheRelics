using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Capture opening elite hand draw bonus from Booming Conch.
    [HarmonyPatch(typeof(BoomingConch), nameof(BoomingConch.ModifyHandDraw))]
    public static class BoomingConchPatch {
        static void Postfix(BoomingConch __instance, Player player, decimal count, ref decimal __result) {
            try {
                var extraDraw = __result - count; // net cards added by the relic
                if (extraDraw > 0) {
                    RelicTracker.AddAmount(__instance, "Cards Drawn", Convert.ToInt32(extraDraw));
                }

                ModLog.Info($"BoomingConchPatch: player={player?.GetType().FullName ?? "null"}, baseCount={count}, result={__result}, extra={extraDraw}");
            } catch { }
        }
    }
}
