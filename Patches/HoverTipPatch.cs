using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace StatTheRelics.Patches;

[HarmonyPatch(typeof(HoverTip), "get_Description")]
public class HoverTipPatch {
    static void Postfix(HoverTip __instance, ref string __result) {
        try {
            var model = __instance.CanonicalModel;
            ModLog.Info($"HoverTipPatch: invoked for model type={model?.GetType().FullName ?? "null"}");
            if (model == null) return;

            var extra = RelicTracker.FormatTooltipAppend(model);
            if (string.IsNullOrEmpty(extra)) return;

            var current = __result ?? string.Empty;
            var header = ModLog.RelicStatsHeader ?? string.Empty;
            var alreadyHasHeader = !string.IsNullOrEmpty(header) && current.Contains(header);
            var alreadyHasBody = !string.IsNullOrEmpty(extra) && current.Contains(extra);
            if (alreadyHasHeader || alreadyHasBody) return;

            __result = current + "\n\n" + extra;
            ModLog.Info($"HoverTipPatch: appended stats for model {model.GetType().FullName}");
        } catch { }
    }
}
