using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Track bonus relic rewards added by Black Star.
    [HarmonyPatch(typeof(BlackStar), nameof(BlackStar.TryModifyRewards))]
    public static class BlackStarPatch {
        static void Postfix(BlackStar __instance, bool __result) {
            try {
                if (!__result) return;
                if (__instance == null) return;

                RelicTracker.AddAmount(__instance, "Relics Given", 1);
                ModLog.Info($"BlackStarPatch: incremented Relics Given for {__instance.GetType().FullName}");
            } catch { }
        }
    }
}