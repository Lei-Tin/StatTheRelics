using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch]
    [PatchTargetAlternative(typeof(Fiddle), "ModifyHandDraw")]
    [PatchTargetAlternative(typeof(Fiddle), "ModifyHandDrawLate")]
    public static class FiddlePatch {
        static MethodBase TargetMethod() => PatchTargetResolver.RequireAny(
            typeof(Fiddle),
            new PatchTargetCandidate("ModifyHandDraw"),
            new PatchTargetCandidate("ModifyHandDrawLate")
        );

        static void Postfix(Fiddle __instance, Player player, decimal count, decimal __result) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                var extra = Math.Max(0, Convert.ToInt32(__result - count));
                if (extra > 0) RelicTracker.AddAmount(__instance, "Extra Draws", extra);
            } catch { }
        }
    }
}
