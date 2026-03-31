using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace StatTheRelics.RelicStats {
    // Handles saving/loading relic counter snapshots alongside run saves and run history files.
    internal static class RelicStatsPersistence {
        class SnapshotEnvelope {
            public Dictionary<string, Dictionary<string, int>> Counters { get; set; } = new();
            public string Note { get; set; } = string.Empty;
        }

        static readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = false };

        static SnapshotEnvelope? pendingRunSnapshot; // staged after load, applied when run initializes
        static SnapshotEnvelope? pendingHistorySnapshot; // staged after history load, shown in UI
        static SnapshotEnvelope? suspendedRunSnapshot; // active run snapshot saved while viewing history
        static volatile bool historyViewActive;

        public static void SaveSnapshot(string basePath) {
            try {
                ModLog.Info($"RelicStatsPersistence: SaveSnapshot invoked for {basePath}");
                var snapshot = RelicTracker.ExportSnapshot();
                var envelope = new SnapshotEnvelope { Counters = snapshot, Note = "" };
                var path = SidecarPath(basePath);
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                var json = JsonSerializer.Serialize(envelope, jsonOptions);
                File.WriteAllText(path, json);
                ModLog.Info($"RelicStatsPersistence: saved sidecar {path}");
            } catch (Exception ex) {
                ModLog.Info($"RelicStatsPersistence: failed to save sidecar for {basePath} - {ex.Message}");
            }
        }

        public static void StageRunSnapshot(string basePath) {
            ModLog.Info($"RelicStatsPersistence: StageRunSnapshot invoked for {basePath}");
            pendingRunSnapshot = LoadEnvelope(basePath, "run");
            if (pendingRunSnapshot != null) {
                ModLog.Info($"RelicStatsPersistence: staged run snapshot from {basePath}, hasData={(pendingRunSnapshot.Counters?.Count ?? 0) > 0}");
            } else {
                ModLog.Info("RelicStatsPersistence: no run snapshot found; proceeding without counters");
            }
        }

        public static void ApplyStagedRunSnapshot() {
            ModLog.Info("RelicStatsPersistence: ApplyStagedRunSnapshot invoked");
            if (pendingRunSnapshot == null) return;
            ApplySnapshot(pendingRunSnapshot, historyMode: false);
            pendingRunSnapshot = null;
        }

        public static void StageHistorySnapshot(string basePath) {
            ModLog.Info($"RelicStatsPersistence: StageHistorySnapshot invoked for {basePath}");
            EnterHistoryView("load-history");

            pendingHistorySnapshot = LoadEnvelope(basePath, "history");
            if (pendingHistorySnapshot == null) {
                pendingHistorySnapshot = new SnapshotEnvelope {
                    Note = "Stats unavailable for this run"
                };
            }
            ApplySnapshot(pendingHistorySnapshot, historyMode: true);
            pendingHistorySnapshot = null;
        }

        public static void ClearSuspendedRunSnapshot(string reason = "reset") {
            if (suspendedRunSnapshot != null) {
                suspendedRunSnapshot = null;
                ModLog.Info($"RelicStatsPersistence: cleared suspended run snapshot ({reason})");
            }
            historyViewActive = false;
        }

        // Used when we know the history UI is being closed even if no restoration occurs (e.g., no suspended snapshot).
        public static void ForceExitHistoryView(string reason = "force-exit") {
            historyViewActive = false;
            ModLog.Info($"RelicStatsPersistence: history view exited ({reason})");
        }

        // Called when the history UI opens even if no history file is loaded (e.g., cached list).
        public static void EnterHistoryView(string reason = "ui-open") {
            try {
                ModLog.Info($"RelicStatsPersistence: EnterHistoryView invoked ({reason})");
                if (historyViewActive) {
                    ModLog.Info($"RelicStatsPersistence: history view already active ({reason})");
                    return;
                }
                if (RelicTracker.IsRunActive && suspendedRunSnapshot == null) {
                    suspendedRunSnapshot = new SnapshotEnvelope {
                        Counters = RelicTracker.ExportSnapshot(),
                        Note = string.Empty
                    };
                    ModLog.Info($"RelicStatsPersistence: suspended active run snapshot ({reason})");
                }
                historyViewActive = true;
                ModLog.Info($"RelicStatsPersistence: history view entered ({reason})");
            } catch { }
        }

        // Restore the suspended run snapshot (if any) after leaving history screens.
        public static void RestoreSuspendedRunSnapshotIfAny() {
            try {
                ModLog.Info("RelicStatsPersistence: RestoreSuspendedRunSnapshotIfAny invoked");
                // Do not override a staged run load
                if (pendingRunSnapshot != null) return;

                if (suspendedRunSnapshot != null) {
                    ApplySnapshot(suspendedRunSnapshot, historyMode: false);
                    suspendedRunSnapshot = null;
                    ModLog.Info("RelicStatsPersistence: restored suspended run snapshot after history view");
                    historyViewActive = false;
                } else {
                    ModLog.Info("RelicStatsPersistence: no suspended run snapshot to restore");
                }
            } catch { }
        }

        public static bool HistoryViewActive => historyViewActive;

        static void ApplySnapshot(SnapshotEnvelope? env, bool historyMode) {
            ModLog.Info($"RelicStatsPersistence: ApplySnapshot invoked (historyMode={historyMode})");
            var counters = env?.Counters ?? new Dictionary<string, Dictionary<string, int>>();
            var note = env?.Note ?? string.Empty;
            RelicTracker.LoadSnapshot(counters, note, historyMode);
            ModLog.Info($"RelicStatsPersistence: applied snapshot mode={(historyMode ? "history" : "live")}, relicTypes={counters.Count}, note='{note}'");
        }

        static SnapshotEnvelope? LoadEnvelope(string basePath, string label) {
            try {
                ModLog.Info($"RelicStatsPersistence: LoadEnvelope invoked for {label} base={basePath}");
                var path = SidecarPath(basePath);
                if (!File.Exists(path)) {
                    ModLog.Info($"RelicStatsPersistence: sidecar missing for {label} at {path}");
                    return null;
                }
                var json = File.ReadAllText(path);
                var env = JsonSerializer.Deserialize<SnapshotEnvelope>(json, jsonOptions);
                ModLog.Info($"RelicStatsPersistence: loaded sidecar {path}");
                return env;
            } catch (Exception ex) {
                ModLog.Info($"RelicStatsPersistence: failed to load sidecar for {label} - {ex.Message}");
                return null;
            }
        }

        static string SidecarPath(string basePath) {
            ModLog.Info($"RelicStatsPersistence: SidecarPath invoked for {basePath}");
            var fullBase = basePath;
            try {
                fullBase = Path.IsPathRooted(basePath)
                    ? basePath
                    : Path.GetFullPath(basePath);
            } catch { }
            return fullBase + ".relicstats.json";
        }
    }
}