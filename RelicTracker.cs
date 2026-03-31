using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using HarmonyLib;
using System.Threading.Tasks;
using System.Threading;
using StatTheRelics.RelicStats;

public static class RelicTracker {
    public class RelicData {
        public readonly ConcurrentDictionary<string,int> Counters = new();
    }

    static ConcurrentDictionary<string, RelicData> dataByType = new();
    static ConcurrentDictionary<string, RelicData> historyDataByType = new();
    static volatile bool runActive;
    static volatile bool historyMode;
    static int runSessionId;
    static string bannerNote = string.Empty;

    public static RelicData? GetOrCreate(object? relic) {
        var typeKey = GetTypeKey(relic);
        if (typeKey == null) return null;
        return dataByType.GetOrAdd(typeKey, _ => {
            ModLog.Info($"RelicTracker: initialized counters for {typeKey}");
            return new RelicData();
        });
    }

    public static void AddAmount(object relic, string key, int amount) {
        try {
            MaybeRestoreLiveAfterHistory();
            if (relic == null) return;
            if (amount == 0) return;
            var typeKey = GetTypeKey(relic);
            if (typeKey == null) return;
            var d = GetOrCreate(relic);
            if (d == null) return;
            var newVal = d.Counters.AddOrUpdate(key, amount, (_, old) => old + amount);
            ModLog.Info($"RelicTracker: {typeKey} - {key} += {amount} => {newVal}");
        } catch { }
    }

    public static void StartNewRunSession(string reason = "start") {
        dataByType = new ConcurrentDictionary<string, RelicData>();
        historyDataByType = new ConcurrentDictionary<string, RelicData>();
        Interlocked.Increment(ref runSessionId);
        runActive = true;
        historyMode = false;
        bannerNote = string.Empty;
        RelicStatsPersistence.ClearSuspendedRunSnapshot("new run session");
        ModLog.Info($"RelicTracker: counters reset for new run ({reason}), session={runSessionId}");
    }

    public static void MarkOutOfRun(string reason = "inactive") {
        runActive = false;
        historyMode = false;
        RelicStatsPersistence.ClearSuspendedRunSnapshot("mark out of run");
        ModLog.Info($"RelicTracker: run marked inactive ({reason}), session={runSessionId}");
    }

    public static bool IsRunActive => runActive;

    public static string FormatTooltipAppend(object? relic) {
        try {
            // If the history UI is active but historyMode somehow isn't set, force history mode with the current history data to avoid showing live stats.
            if (RelicStatsPersistence.HistoryViewActive && !historyMode) {
                historyMode = true;
                runActive = false;
                ModLog.Info("RelicTracker: coerced into history mode because history view is active");
            }

            // If we left history view, try to restore live snapshot on first tooltip usage outside history stack.
            if (historyMode && !RelicStatsPersistence.HistoryViewActive && !IsHistoryStack()) {
                RelicStatsPersistence.RestoreSuspendedRunSnapshotIfAny();
                // Fallback: if still in historyMode with no suspended snapshot, drop back to live mode to avoid showing history data.
                if (historyMode && !RelicStatsPersistence.HistoryViewActive) {
                    historyMode = false;
                    runActive = true;
                    ModLog.Info("RelicTracker: exited history mode via tooltip fallback");
                }
            }

            if (!runActive && !historyMode) return string.Empty;
            var typeKey = GetTypeKey(relic);
            if (typeKey == null) return string.Empty;
            var d = historyMode
                ? historyDataByType.GetOrAdd(typeKey, _ => new RelicData())
                : dataByType.GetOrAdd(typeKey, _ => new RelicData());

            var def = RelicStatsRegistry.GetDefinition(typeKey);
            var body = def != null
                ? def.Format(d.Counters, historyMode, historyMode ? bannerNote : string.Empty)
                : BaseRelicStats.FormatDefault(RelicStatsRegistry.DefaultCounters, d.Counters, historyMode, historyMode ? bannerNote : string.Empty);

            var bodyText = (body ?? string.Empty).TrimEnd();
            if (string.IsNullOrEmpty(bodyText)) return string.Empty;

            var header = ModLog.RelicStatsHeader ?? string.Empty;
            if (string.IsNullOrWhiteSpace(header)) return bodyText;

            var sb = new StringBuilder();
            sb.AppendLine(header);
            sb.Append(bodyText);
            return sb.ToString();
        } catch { return string.Empty; }
    }

    // Check if we already have data for this instance without creating new entry
    public static bool HasData(object? relic) {
        try {
            var typeKey = GetTypeKey(relic);
            if (typeKey == null) return false;
            return dataByType.TryGetValue(typeKey, out var d) && d.Counters.Count > 0;
        } catch { return false; }
    }

    internal static bool IsHistoryStack() {
        try {
            var st = new StackTrace();
            var frames = st.GetFrames();
            if (frames == null) return false;
            foreach (var f in frames) {
                var tn = f.GetMethod()?.DeclaringType?.FullName;
                if (tn != null && tn.IndexOf("history", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
        } catch { }
        return false;
    }

    public static Dictionary<string, Dictionary<string,int>> ExportSnapshot() {
        var result = new Dictionary<string, Dictionary<string, int>>();
        foreach (var kv in dataByType) {
            result[kv.Key] = new Dictionary<string, int>(kv.Value.Counters);
        }
        return result;
    }

    public static void LoadSnapshot(IDictionary<string, Dictionary<string,int>>? snapshot, string note, bool historyMode) {
        try {
            bannerNote = historyMode ? (note ?? string.Empty) : string.Empty;
            var source = snapshot ?? new Dictionary<string, Dictionary<string,int>>();
            if (historyMode) {
                historyDataByType = new ConcurrentDictionary<string, RelicData>(StringComparer.Ordinal);
                foreach (var kv in source) {
                    var rd = new RelicData();
                    foreach (var c in kv.Value) rd.Counters[c.Key] = c.Value;
                    historyDataByType[kv.Key] = rd;
                }
                RelicTracker.historyMode = true;
                runActive = false;
            } else {
                dataByType = new ConcurrentDictionary<string, RelicData>(StringComparer.Ordinal);
                foreach (var kv in source) {
                    var rd = new RelicData();
                    foreach (var c in kv.Value) rd.Counters[c.Key] = c.Value;
                    dataByType[kv.Key] = rd;
                }
                runActive = true;
                RelicTracker.historyMode = false;
            }
            ModLog.Info($"RelicTracker: loaded snapshot (historyMode={historyMode}, note='{bannerNote}')");
        } catch { }
    }

    public static class RelicPatches {
        public static void ApplyDynamicPatches(Harmony harmony) {
            try {
                // Patch base RelicModel flash methods so every relic flash is counted, even when the flash is invoked in the base class.
                var relicModelType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Models.RelicModel");
                if (relicModelType != null) {
                    var flashMethods = relicModelType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(m => m.Name.Equals("Flash", StringComparison.OrdinalIgnoreCase));
                    foreach (var fm in flashMethods) {
                        try { harmony.Patch(fm, prefix: new HarmonyMethod(typeof(RelicPatches).GetMethod(nameof(CountFlashPrefix), BindingFlags.Static | BindingFlags.NonPublic))); } catch { }
                    }
                }

                ModLog.Info("RelicTracker: dynamic relic scan skipped; only base RelicModel flash patched");
            } catch { }
        }

        static void CountFlashPrefix(object __instance, MethodBase __originalMethod) {
            try {
                var tk = GetTypeKey(__instance);
                if (tk == null) return;
                var def = RelicStatsRegistry.GetDefinition(tk);
                if (def != null) {
                    var hasFlash = def.DefaultCounters?.Any(c => string.Equals(c, "Flashes", StringComparison.OrdinalIgnoreCase)) == true;
                    if (!hasFlash) return;
                }
                AddAmount(__instance, "Flashes", 1);
            } catch { }
        }

    }

    static readonly string relicNamespaceToken = ".Relics";
    static string? GetTypeKey(object? relic) {
        try {
            if (relic == null) return null;
            var type = relic.GetType();
            var ns = type.Namespace ?? string.Empty;
            if (ns.IndexOf(relicNamespaceToken, StringComparison.OrdinalIgnoreCase) < 0) return null;
            return type.FullName;
        } catch { return null; }
    }

    static void MaybeRestoreLiveAfterHistory() {
        try {
            // Restore after a history snapshot has taken over (historyMode true) once we're out of the history UI call stack and view.
            if (historyMode && !RelicStatsPersistence.HistoryViewActive && !IsHistoryStack()) {
                RelicStatsPersistence.RestoreSuspendedRunSnapshotIfAny();
                if (historyMode && !RelicStatsPersistence.HistoryViewActive) {
                    historyMode = false;
                    runActive = true;
                    ModLog.Info("RelicTracker: exited history mode and resumed live counters (counter fallback)");
                }
            }
        } catch { }
    }
}
