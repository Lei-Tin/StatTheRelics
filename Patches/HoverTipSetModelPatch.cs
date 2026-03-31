using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using StatTheRelics.RelicStats;

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

            // If we already left history view (flag cleared) and are back in non-history context (run inactive, not on history stack), restore live snapshot.
            if (!RelicStatsPersistence.HistoryViewActive && !RelicTracker.IsHistoryStack() && !RelicTracker.IsRunActive) {
                RelicStatsPersistence.RestoreSuspendedRunSnapshotIfAny();
            }
        } catch { }
    }
}
