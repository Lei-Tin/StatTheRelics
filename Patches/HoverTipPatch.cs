using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace StatTheRelics.Patches;

internal static class HoverTipRelicCollectionSuppressor {
    static readonly ConditionalWeakTable<AbstractModel, object> SuppressedModels = new();
    static readonly object Marker = new();

    public static void Update(HoverTip tip, AbstractModel model) {
        try {
            if (IsRelicModel(model) && IsRelicCollectionStack()) {
                if (!SuppressedModels.TryGetValue(model, out _)) SuppressedModels.Add(model, Marker);
            } else {
                SuppressedModels.Remove(model);
            }
        } catch { }
    }

    public static bool ShouldSuppress(HoverTip tip) {
        try {
            var model = tip.CanonicalModel;
            return (model != null && SuppressedModels.TryGetValue(model, out _)) || IsRelicCollectionStack();
        } catch {
            return false;
        }
    }

    static bool IsRelicModel(object? model) {
        try {
            var ns = model?.GetType().Namespace ?? string.Empty;
            return ns.IndexOf(".Relics", StringComparison.OrdinalIgnoreCase) >= 0;
        } catch {
            return false;
        }
    }

    static bool IsRelicCollectionStack() {
        try {
            var frames = new StackTrace().GetFrames();
            if (frames == null) return false;

            foreach (var frame in frames) {
                var typeName = frame.GetMethod()?.DeclaringType?.FullName;
                if (string.IsNullOrEmpty(typeName)) continue;
                if (typeName.IndexOf("Screens.RelicCollection", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (typeName.IndexOf("NRelicCollection", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
        } catch { }

        return false;
    }
}

[HarmonyPatch(typeof(HoverTip), "get_Description")]
public class HoverTipPatch {
    static void Postfix(HoverTip __instance, ref string __result) {
        try {
            if (HoverTipRelicCollectionSuppressor.ShouldSuppress(__instance)) return;

            var model = __instance.CanonicalModel;
            if (model == null) return;

            var extra = RelicTracker.FormatTooltipAppend(model);
            if (string.IsNullOrEmpty(extra)) return;

            var current = __result ?? string.Empty;
            var header = ModLog.RelicStatsHeader ?? string.Empty;
            var alreadyHasHeader = !string.IsNullOrEmpty(header) && current.Contains(header);
            var alreadyHasBody = !string.IsNullOrEmpty(extra) && current.Contains(extra);
            if (alreadyHasHeader || alreadyHasBody) return;

            __result = current + "\n\n" + extra;
        } catch (Exception ex) {
            ModLog.Exception("HoverTipPatch", ex);
        }
    }
}
