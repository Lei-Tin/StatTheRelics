using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

    public static RelicData GetOrCreate(object relic) {
        var typeKey = GetTypeKey(relic);
        if (typeKey == null) return null;
        return dataByType.GetOrAdd(typeKey, _ => {
            ModLog.Info($"RelicTracker: initialized counters for {typeKey}");
            return new RelicData();
        });
    }

    public static void Increment(object relic, string key) {
        try {
            if (relic == null) return;
            var typeKey = GetTypeKey(relic);
            if (typeKey == null) return;
            var d = GetOrCreate(relic);
            var newVal = d.Counters.AddOrUpdate(key, 1, (_, old) => old + 1);
            ModLog.Info($"RelicTracker: {typeKey} - {key} => {newVal}");
        } catch { }
    }

    public static void AddAmount(object relic, string key, int amount) {
        try {
            if (relic == null) return;
            if (amount == 0) return;
            var typeKey = GetTypeKey(relic);
            if (typeKey == null) return;
            var d = GetOrCreate(relic);
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
        ModLog.Info($"RelicTracker: counters reset for new run ({reason})");
    }

    public static void MarkOutOfRun(string reason = "inactive") {
        runActive = false;
        historyMode = false;
        ModLog.Info($"RelicTracker: run marked inactive ({reason})");
    }

    public static bool IsRunActive => runActive;

    // Legacy entry point
    public static void Reset() => StartNewRunSession("Reset()");

    public static string FormatTooltipAppend(object relic) {
        try {
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
    public static bool HasData(object relic) {
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

    public static void LoadSnapshot(IDictionary<string, Dictionary<string,int>> snapshot, string note, bool historyMode) {
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
        static readonly string[] obtainNames = new[] { "OnObtain", "OnPickup", "OnPickUp", "Obtain", "Pickup", "OnEquip", "Equip", ".ctor" };
        static readonly string[] tooltipHints = new[] { "Tooltip", "GetDescription", "GetHoverText", "GetText", "GetTooltip" };
        static readonly string[] effectHints = new[] { "Activate", "Use", "Trigger", "OnTrigger", "OnUse", "OnActivate", "OnPlayerTurnStart", "AtBattleStart", "OnAttack", "After", "Before", "Flash" };
        static readonly string[] setterHints = new[] { "set_UsedThisCombat", "set_UsedThisTurn", "set_WasUsed", "set_WasUsedThisCombat", "set_WasUsedThisTurn", "set_ShouldTrigger", "set_IsUsedUp", "set_Triggered" };

        public static void ApplyDynamicPatches(Harmony harmony) {
            try {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && (a.GetName().Name?.StartsWith("MegaCrit") == true || a.GetName().Name?.StartsWith("MegaCrit.Sts2") == true));

                foreach (var asm in assemblies) {
                    Type[] types;
                    try { types = asm.GetTypes(); } catch { continue; }
                    foreach (var t in types) {
                        if (t == null) continue;
                        if (t.Namespace == null || !t.Namespace.Contains("MegaCrit.Sts2.Core.Models.Relics")) continue;
                        if (t.IsAbstract) continue;

                        // Patch obtain-like methods/ctors to initialize counters
                        foreach (var on in obtainNames) {
                            MethodBase m = null;
                            if (on == ".ctor") {
                                var ctors = t.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                                foreach (var c in ctors) {
                                    try { harmony.Patch(c, postfix: new HarmonyMethod(typeof(RelicPatches).GetMethod(nameof(RelicObtainedPostfix), BindingFlags.Static | BindingFlags.NonPublic))); } catch { }
                                }
                                continue;
                            }
                            m = AccessTools.Method(t, on);
                            if (m != null) {
                                try { harmony.Patch(m, postfix: new HarmonyMethod(typeof(RelicPatches).GetMethod(nameof(RelicObtainedPostfix), BindingFlags.Static | BindingFlags.NonPublic))); } catch { }
                            }
                        }

                        // Patch tooltip-like methods that return string
                        var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        foreach (var m in methods) {
                            if (m.ReturnType == typeof(string) && tooltipHints.Any(h => m.Name.IndexOf(h, StringComparison.OrdinalIgnoreCase) >= 0)) {
                                try { harmony.Patch(m, postfix: new HarmonyMethod(typeof(RelicPatches).GetMethod(nameof(TooltipPostfix), BindingFlags.Static | BindingFlags.NonPublic))); } catch { }
                            }
                        }

                        // Patch effect methods to increment counters
                        foreach (var m in methods) {
                            var isFlash = m.Name.Equals("DoActivateVisuals", StringComparison.OrdinalIgnoreCase) || m.Name.IndexOf("Flash", StringComparison.OrdinalIgnoreCase) >= 0;
                            var nameMatch = isFlash || effectHints.Any(h => m.Name.IndexOf(h, StringComparison.OrdinalIgnoreCase) >= 0) || setterHints.Any(sh => m.Name.Equals(sh, StringComparison.OrdinalIgnoreCase));
                            var returnsVoid = m.ReturnType == typeof(void);
                            var returnsTask = m.ReturnType == typeof(Task) || (m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
                            var isSetter = m.IsSpecialName && m.Name.StartsWith("set_");
                            if (nameMatch && (returnsVoid || returnsTask || isSetter)) {
                                var targetPrefix = isSetter ? nameof(CountSetterPrefix) : isFlash ? nameof(CountFlashPrefix) : nameof(CountPrefix);
                                try { harmony.Patch(m, prefix: new HarmonyMethod(typeof(RelicPatches).GetMethod(targetPrefix, BindingFlags.Static | BindingFlags.NonPublic))); } catch { }
                            }
                        }
                    }
                }

                // Patch base RelicModel flash methods so every relic flash is counted, even when the flash is invoked in the base class
                var relicModelType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Models.RelicModel");
                if (relicModelType != null) {
                    var flashMethods = relicModelType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(m => m.Name.Equals("Flash", StringComparison.OrdinalIgnoreCase));
                    foreach (var fm in flashMethods) {
                        try { harmony.Patch(fm, prefix: new HarmonyMethod(typeof(RelicPatches).GetMethod(nameof(CountFlashPrefix), BindingFlags.Static | BindingFlags.NonPublic))); } catch { }
                    }
                }
            } catch { }
        }

        static void RelicObtainedPostfix(object __instance) {
            try {
                GetOrCreate(__instance);
                ModLog.Info($"RelicTracker: Relic obtained patch ran for {__instance?.GetType().FullName}");
            } catch { }
        }

        static void TooltipPostfix(object __instance, ref string __result) {
            try {
                var extra = FormatTooltipAppend(__instance);
                if (!string.IsNullOrEmpty(extra)) {
                    __result = __result + "\n\n" + extra;
                    ModLog.Info($"RelicTracker: appended tooltip stats for {__instance?.GetType().FullName}");
                }
            } catch { }
        }

        static void CountPrefix(object __instance, MethodBase __originalMethod) {
            try {
                var name = __originalMethod?.Name ?? "effect";
                Increment(__instance, name);
            } catch { }
        }

        static void CountFlashPrefix(object __instance, MethodBase __originalMethod) {
            try {
                Increment(__instance, "Flashes");
            } catch { }
        }

        static void CountSetterPrefix(object __instance, MethodBase __originalMethod) {
            try {
                var name = __originalMethod?.Name ?? "setter";
                Increment(__instance, name);
            } catch { }
        }

    }

    static string GetTypeKey(object relic) {
        try {
            return relic == null ? null : relic.GetType().FullName;
        } catch { return null; }
    }
}
