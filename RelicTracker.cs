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
        public readonly ConcurrentDictionary<string,string> TextStats = new();
    }

    static ConcurrentDictionary<string, RelicData> dataByType = new();
    static ConcurrentDictionary<string, RelicData> historyDataByType = new();
    static volatile bool runActive;
    static volatile bool historyMode;
    static int runSessionId;
    static string bannerNote = string.Empty;

    public static RelicData? GetOrCreate(object? relic) {
        var instanceKey = GetInstanceKey(relic);
        if (instanceKey == null) return null;
        return dataByType.GetOrAdd(instanceKey, _ => {
            var rd = new RelicData();
            var defaults = RelicStatsRegistry.GetDefaultCounters(instanceKey);
            if (defaults != null) {
                foreach (var key in defaults) {
                    if (!string.IsNullOrWhiteSpace(key)) rd.Counters.TryAdd(key, 0);
                }
            }
            ModLog.Info($"RelicTracker: initialized counters for {instanceKey}");
            return rd;
        });
    }

    public static void AddAmount(object relic, string key, int amount) {
        try {
            MaybeRestoreLiveAfterHistory();
            if (relic == null) return;
            if (amount == 0) return;
            var instanceKey = GetInstanceKey(relic);
            if (instanceKey == null) return;
            var d = GetOrCreate(relic);
            if (d == null) return;
            var newVal = d.Counters.AddOrUpdate(key, amount, (_, old) => old + amount);
            ModLog.Info($"RelicTracker: {instanceKey} - {key} += {amount} => {newVal}");
        } catch { }
    }

    public static void SetText(object relic, string key, string value) {
        try {
            MaybeRestoreLiveAfterHistory();
            if (relic == null) return;
            if (string.IsNullOrWhiteSpace(key)) return;
            if (string.IsNullOrWhiteSpace(value)) return;
            var instanceKey = GetInstanceKey(relic);
            if (instanceKey == null) return;
            var d = GetOrCreate(relic);
            if (d == null) return;
            d.TextStats[key] = value;
            ModLog.Info($"RelicTracker: {instanceKey} - {key} := {value}");
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
            RelicData? d;
            if (historyMode) {
                TryGetDataForRelic(historyDataByType, relic, typeKey, out d);
            } else {
                d = GetOrCreate(relic);
            }

            var def = RelicStatsRegistry.GetDefinition(typeKey);
            var counters = d?.Counters ?? new ConcurrentDictionary<string, int>();
            var textStats = d?.TextStats ?? new ConcurrentDictionary<string, string>();
            var body = def != null
                ? def.Format(counters, textStats, historyMode, historyMode ? bannerNote : string.Empty)
                : BaseRelicStats.FormatDefault(RelicStatsRegistry.DefaultCounters, counters, historyMode, historyMode ? bannerNote : string.Empty);

            var bodyText = (body ?? string.Empty).TrimEnd();
            var header = ModLog.RelicStatsHeader ?? string.Empty;
            if (string.IsNullOrWhiteSpace(header)) return bodyText;
            if (string.IsNullOrEmpty(bodyText)) return header;

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
            if (TryGetDataForRelic(dataByType, relic, typeKey, out var d) && d != null) {
                return d.Counters.Count > 0 || d.TextStats.Count > 0;
            }
            return false;
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
            var map = new Dictionary<string, int>(kv.Value.Counters);
            var defaults = RelicStatsRegistry.GetDefaultCounters(kv.Key);
            if (defaults != null) {
                foreach (var key in defaults) {
                    if (!string.IsNullOrWhiteSpace(key) && !map.ContainsKey(key)) map[key] = 0;
                }
            }
            result[kv.Key] = map;
        }
        return result;
    }

    public static Dictionary<string, Dictionary<string,string>> ExportTextSnapshot() {
        var result = new Dictionary<string, Dictionary<string, string>>();
        foreach (var kv in dataByType) {
            result[kv.Key] = new Dictionary<string, string>(kv.Value.TextStats);
        }
        return result;
    }

    public static void LoadSnapshot(
        IDictionary<string, Dictionary<string,int>>? snapshot,
        IDictionary<string, Dictionary<string,string>>? textSnapshot,
        string note,
        bool historyMode
    ) {
        try {
            bannerNote = historyMode ? (note ?? string.Empty) : string.Empty;
            var source = snapshot ?? new Dictionary<string, Dictionary<string,int>>();
            var textSource = textSnapshot ?? new Dictionary<string, Dictionary<string,string>>();
            if (historyMode) {
                historyDataByType = new ConcurrentDictionary<string, RelicData>(StringComparer.Ordinal);
                foreach (var kv in source) {
                    var rd = new RelicData();
                    foreach (var c in kv.Value) rd.Counters[c.Key] = c.Value;
                    if (textSource.TryGetValue(kv.Key, out var textMap)) {
                        foreach (var t in textMap) rd.TextStats[t.Key] = t.Value;
                    }
                    historyDataByType[kv.Key] = rd;
                }
                foreach (var tk in textSource) {
                    if (historyDataByType.ContainsKey(tk.Key)) continue;
                    var rd = new RelicData();
                    foreach (var t in tk.Value) rd.TextStats[t.Key] = t.Value;
                    historyDataByType[tk.Key] = rd;
                }
                RelicTracker.historyMode = true;
                runActive = false;
            } else {
                dataByType = new ConcurrentDictionary<string, RelicData>(StringComparer.Ordinal);
                foreach (var kv in source) {
                    var rd = new RelicData();
                    foreach (var c in kv.Value) rd.Counters[c.Key] = c.Value;
                    if (textSource.TryGetValue(kv.Key, out var textMap)) {
                        foreach (var t in textMap) rd.TextStats[t.Key] = t.Value;
                    }
                    dataByType[kv.Key] = rd;
                }
                foreach (var tk in textSource) {
                    if (dataByType.ContainsKey(tk.Key)) continue;
                    var rd = new RelicData();
                    foreach (var t in tk.Value) rd.TextStats[t.Key] = t.Value;
                    dataByType[tk.Key] = rd;
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

                    // Seed tracker entries when relics are obtained so 0-value counters can still render.
                    var afterObtained = AccessTools.Method(relicModelType, "AfterObtained");
                    if (afterObtained != null) {
                        try {
                            harmony.Patch(afterObtained, postfix: new HarmonyMethod(typeof(RelicPatches).GetMethod(nameof(AfterObtainedPostfix), BindingFlags.Static | BindingFlags.NonPublic)));
                            ModLog.Info("RelicTracker: patched RelicModel.AfterObtained for tracker seeding");
                        } catch (Exception ex) {
                            ModLog.Info($"RelicTracker: failed to patch RelicModel.AfterObtained - {ex.GetType().Name}: {ex.Message}");
                        }
                    } else {
                        ModLog.Info("RelicTracker: RelicModel.AfterObtained not found");
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

        static void AfterObtainedPostfix(object __instance) {
            try {
                var tk = GetTypeKey(__instance);
                if (tk == null) return;
                var created = GetOrCreate(__instance) != null;
                ModLog.Info($"RelicTracker: AfterObtained seed for {tk}, created={created}");
            } catch { }
        }

    }

    static readonly string relicNamespaceToken = ".Relics";
    static bool TryGetDataForRelic(ConcurrentDictionary<string, RelicData> source, object? relic, string typeKey, out RelicData? data) {
        data = null;
        return source.TryGetValue(typeKey, out data);
    }

    static string? GetInstanceKey(object? relic) {
        try {
            if (relic == null) return null;
            var typeKey = GetTypeKey(relic);
            if (typeKey == null) return null;
            return typeKey;
        } catch {
            return GetTypeKey(relic);
        }
    }

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
