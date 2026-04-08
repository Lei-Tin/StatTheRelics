using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Capture how much HP loss is prevented by Beating Remnant.
    [HarmonyPatch(typeof(BeatingRemnant), nameof(BeatingRemnant.ModifyHpLostAfterOsty))]
    public static class BeatingRemnantPatch {
        static void Postfix(BeatingRemnant __instance, decimal amount, ref decimal __result) {
            try {
                var mitigated = amount - __result; // pre-mitigated input minus post-mitigation output
                if (mitigated > 0m) {
                    RelicTracker.AddAmount(__instance, "Damage Mitigated", Convert.ToInt32(mitigated));
                }

                ModLog.Info($"BeatingRemnantPatch: preMitigation={amount}, postMitigation={__result}, mitigated={mitigated}");
            } catch { }
        }
    }
}
