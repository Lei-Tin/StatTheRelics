using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using System;

namespace StatTheRelics.Patches;

// This getter runs on the actual relic instance before its tooltip is associated
// with the canonical model, so Owner reliably distinguishes obtained relics from previews.
[HarmonyPatch(typeof(RelicModel), "get_HoverTip")]
public class HoverTipPatch {
    static void Postfix(RelicModel __instance, ref HoverTip __result) {
        try {
            if (__instance?.Owner == null) return;

            var extra = RelicTracker.FormatTooltipAppend(__instance);
            if (string.IsNullOrEmpty(extra)) return;

            var current = __result.Description ?? string.Empty;
            var header = ModLog.RelicStatsHeader ?? string.Empty;
            var alreadyHasHeader = !string.IsNullOrEmpty(header) && current.Contains(header);
            var alreadyHasBody = current.Contains(extra);
            if (alreadyHasHeader || alreadyHasBody) return;

            var replacement = new HoverTip(__instance.Title, current + "\n\n" + extra, __result.Icon);
            if (__result.CanonicalModel != null) replacement.SetCanonicalModel(__result.CanonicalModel);
            __result = replacement;
        } catch (Exception ex) {
            ModLog.Exception("HoverTipPatch", ex);
        }
    }
}
