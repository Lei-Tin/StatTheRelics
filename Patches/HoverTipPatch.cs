using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace StatTheRelics.Patches;

[HarmonyPatch(typeof(HoverTip), "get_Description")]
public class HoverTipPatch {
    static void Postfix(HoverTip __instance, ref string __result) {
        try {
            var model = __instance.CanonicalModel;
            if (model != null) {
                var extra = RelicTracker.FormatTooltipAppend(model);
                if (!string.IsNullOrEmpty(extra)) {
                    __result = __result + "\n\n" + extra;
                    ModLog.Info($"HoverTipPatch: appended stats for model {model.GetType().FullName}");
                }
            }
        } catch { }
    }
}
