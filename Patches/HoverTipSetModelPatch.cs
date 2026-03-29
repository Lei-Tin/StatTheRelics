using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace StatTheRelics.Patches;

[HarmonyPatch(typeof(HoverTip), "SetCanonicalModel")]
public class HoverTipSetModelPatch {
    static readonly FieldInfo DescField = AccessTools.Field(typeof(HoverTip), "<Description>k__BackingField");

    static void Postfix(HoverTip __instance, AbstractModel model) {
        try {
            if (model == null) {
                ModLog.Info("HoverTipSetModelPatch: SetCanonicalModel called with null model");
                return;
            }
            var has = RelicTracker.HasData(model);
            ModLog.Info($"HoverTipSetModelPatch: model set -> {model.GetType().FullName}, HasData={has}, Hash={model.GetHashCode()}");

            // Always append for relic models so we can see the block even with zero data
            if (model.GetType().FullName.Contains("Relics")) {
                var extra = RelicTracker.FormatTooltipAppend(model);
                if (!string.IsNullOrEmpty(extra)) {
                    var header = ModLog.RelicStatsHeader ?? string.Empty;
                    var description = __instance.Description ?? string.Empty;
                    var alreadyHasHeader = !string.IsNullOrEmpty(header) && description.Contains(header);
                    // avoid double-append if already present
                    if (!alreadyHasHeader) {
                        var updated = description + "\n\n" + extra;
                        if (DescField != null) {
                            DescField.SetValue(__instance, updated);
                            ModLog.Info($"HoverTipSetModelPatch: appended stats on SetCanonicalModel for {model.GetType().FullName}");
                        } else {
                            ModLog.Info("HoverTipSetModelPatch: DescField not found; cannot set description");
                        }
                    }
                }
            }
        } catch { }
    }
}
