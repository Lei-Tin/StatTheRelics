using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using StatTheRelics.RelicStats;

namespace StatTheRelics.Patches;

[HarmonyPatch(typeof(HoverTip), "SetCanonicalModel")]
public class HoverTipSetModelPatch {
    static void Postfix(HoverTip __instance, AbstractModel model) {
        try {
            if (model == null) return;

            // If we already left history view (flag cleared) and are back in non-history context (run inactive, not on history stack), restore live snapshot.
            if (!RelicStatsPersistence.HistoryViewActive && !RelicTracker.IsHistoryStack() && !RelicTracker.IsRunActive) {
                RelicStatsPersistence.RestoreSuspendedRunSnapshotIfAny();
            }
        } catch (Exception ex) {
            ModLog.Exception("HoverTipSetModelPatch", ex);
        }
    }
}
