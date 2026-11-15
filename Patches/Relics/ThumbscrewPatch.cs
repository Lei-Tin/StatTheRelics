using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Thumbscrew), nameof(Thumbscrew.ModifyAttackHitCount))]
    public static class ThumbscrewPatch {
        static void Postfix(Thumbscrew __instance, int hitCount, int __result) {
            try {
                if (__result > hitCount) RelicTracker.AddAmount(__instance, "Extra Hits Added", __result - hitCount);
            } catch { }
        }
    }
}
