using System;
using System.Collections.Concurrent;
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

        static SnapshotEnvelope pendingRunSnapshot; // staged after load, applied when run initializes
        static SnapshotEnvelope pendingHistorySnapshot; // staged after history load, shown in UI

        public static void SaveSnapshot(string basePath) {
            try {
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
            pendingRunSnapshot = LoadEnvelope(basePath, "run");
        }

        public static void ApplyStagedRunSnapshot() {
            if (pendingRunSnapshot == null) return;
            ApplySnapshot(pendingRunSnapshot, historyMode: false);
            pendingRunSnapshot = null;
        }

        public static void StageHistorySnapshot(string basePath) {
            pendingHistorySnapshot = LoadEnvelope(basePath, "history");
            if (pendingHistorySnapshot == null) {
                pendingHistorySnapshot = new SnapshotEnvelope { Note = "Stats unavailable: no relic stats saved with this run" };
            }
            ApplySnapshot(pendingHistorySnapshot, historyMode: true);
            pendingHistorySnapshot = null;
        }

        static void ApplySnapshot(SnapshotEnvelope env, bool historyMode) {
            var counters = env?.Counters ?? new Dictionary<string, Dictionary<string, int>>();
            var note = env?.Note ?? string.Empty;
            RelicTracker.LoadSnapshot(counters, note, historyMode);
        }

        static SnapshotEnvelope LoadEnvelope(string basePath, string label) {
            try {
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
            var fullBase = basePath;
            try {
                fullBase = Path.IsPathRooted(basePath)
                    ? basePath
                    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, basePath));
            } catch { }
            return fullBase + ".relicstats.json";
        }
    }
}