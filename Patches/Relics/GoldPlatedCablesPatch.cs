using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(GoldPlatedCables), nameof(GoldPlatedCables.ModifyOrbPassiveTriggerCounts))]
    public static class GoldPlatedCablesPatch {
        static void Postfix(GoldPlatedCables __instance, OrbModel orb, int triggerCount, int __result) {
            try {
                var extra = __result - triggerCount;
                if (extra > 0) RelicTracker.AddAmount(__instance, "Extra Orb Passive Triggers", extra);
            } catch { }
        }
    }
}
