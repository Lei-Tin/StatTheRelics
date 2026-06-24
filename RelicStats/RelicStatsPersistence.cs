using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace StatTheRelics.RelicStats {
    // Handles saving/loading relic counter snapshots alongside run saves and run history files.
    internal static class RelicStatsPersistence {
        class SnapshotEnvelope {
            public string ModVersion { get; set; } = string.Empty;
            public Dictionary<string, Dictionary<string, int>> Counters { get; set; } = new();
            public Dictionary<string, Dictionary<string, string>> TextStats { get; set; } = new();
            public string Note { get; set; } = string.Empty;
            public bool StatsUnavailable { get; set; }
        }

        static readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = false };
        static readonly string currentModVersion = GetCurrentModVersion();

        static SnapshotEnvelope? pendingRunSnapshot; // staged after load, applied when run initializes
        static SnapshotEnvelope? pendingHistorySnapshot; // staged after history load, shown in UI
        static SnapshotEnvelope? suspendedRunSnapshot; // active run snapshot saved while viewing history
        static volatile bool historyViewActive;

        public static void SaveSnapshot(string basePath, object? saveStore = null) {
            try {
                var snapshot = RelicTracker.ExportSnapshot();
                var textSnapshot = RelicTracker.ExportTextSnapshot();
                var envelope = new SnapshotEnvelope {
                    ModVersion = currentModVersion,
                    Counters = snapshot,
                    TextStats = textSnapshot,
                    Note = ""
                };
                var path = SidecarPath(basePath);
                var json = JsonSerializer.Serialize(envelope, jsonOptions);

                if (TryWriteWithSaveStore(saveStore, path, json)) return;

                var physicalPath = PhysicalSidecarPath(basePath);
                var dir = Path.GetDirectoryName(physicalPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(physicalPath, json);
            } catch (Exception ex) {
                ModLog.Info($"RelicStatsPersistence: failed to save sidecar for {basePath} - {ex.Message}");
            }
        }

        public static void StageRunSnapshot(string basePath, object? saveStore = null) {
            pendingRunSnapshot = LoadEnvelope(basePath, "run", saveStore);
        }

        public static void ApplyStagedRunSnapshot() {
            if (pendingRunSnapshot == null) return;
            ApplySnapshot(pendingRunSnapshot, historyMode: false);
            pendingRunSnapshot = null;
        }

        public static void StageHistorySnapshot(string basePath, object? saveStore = null) {
            EnterHistoryView("load-history");

            pendingHistorySnapshot = LoadEnvelope(basePath, "history", saveStore);
            if (pendingHistorySnapshot == null) {
                pendingHistorySnapshot = new SnapshotEnvelope {
                    ModVersion = currentModVersion,
                    Note = "Stats unavailable for this run",
                    StatsUnavailable = true
                };
            }
            ApplySnapshot(pendingHistorySnapshot, historyMode: true);
            pendingHistorySnapshot = null;
        }

        public static void ClearSuspendedRunSnapshot(string reason = "reset") {
            if (suspendedRunSnapshot != null) {
                suspendedRunSnapshot = null;
            }
            historyViewActive = false;
        }

        // Used when we know the history UI is being closed even if no restoration occurs (e.g., no suspended snapshot).
        public static void ForceExitHistoryView(string reason = "force-exit") {
            historyViewActive = false;
        }

        // Called when the history UI opens even if no history file is loaded (e.g., cached list).
        public static void EnterHistoryView(string reason = "ui-open") {
            try {
                if (historyViewActive) {
                    return;
                }
                if (RelicTracker.IsRunActive && suspendedRunSnapshot == null) {
                    suspendedRunSnapshot = new SnapshotEnvelope {
                        ModVersion = currentModVersion,
                        Counters = RelicTracker.ExportSnapshot(),
                        TextStats = RelicTracker.ExportTextSnapshot(),
                        Note = string.Empty
                    };
                }
                historyViewActive = true;
            } catch { }
        }

        // Restore the suspended run snapshot (if any) after leaving history screens.
        public static void RestoreSuspendedRunSnapshotIfAny() {
            try {
                // Do not override a staged run load
                if (pendingRunSnapshot != null) return;

                if (suspendedRunSnapshot != null) {
                    ApplySnapshot(suspendedRunSnapshot, historyMode: false);
                    suspendedRunSnapshot = null;
                    historyViewActive = false;
                }
            } catch { }
        }

        public static bool HistoryViewActive => historyViewActive;

        static void ApplySnapshot(SnapshotEnvelope? env, bool historyMode) {
            var counters = env?.Counters ?? new Dictionary<string, Dictionary<string, int>>();
            var textStats = env?.TextStats ?? new Dictionary<string, Dictionary<string, string>>();
            var note = env?.Note ?? string.Empty;
            RelicTracker.LoadSnapshot(counters, textStats, note, historyMode, env?.StatsUnavailable == true);
        }

        static SnapshotEnvelope? LoadEnvelope(string basePath, string label, object? saveStore = null) {
            try {
                var path = SidecarPath(basePath);
                string? json = null;
                if (TryReadWithSaveStore(saveStore, path, out var storeJson)) {
                    json = storeJson;
                } else {
                    var physicalPath = PhysicalSidecarPath(basePath);
                    if (!File.Exists(physicalPath)) {
                        return null;
                    }
                    json = File.ReadAllText(physicalPath);
                }

                if (string.IsNullOrWhiteSpace(json)) {
                    return null;
                }

                var env = JsonSerializer.Deserialize<SnapshotEnvelope>(json, jsonOptions);
                if (env == null) return null;
                if (!IsCompatibleVersion(env.ModVersion)) return VersionMismatchEnvelope(env.ModVersion);
                return env;
            } catch (Exception ex) {
                ModLog.Info($"RelicStatsPersistence: failed to load sidecar for {label} - {ex.Message}");
                return null;
            }
        }

        static string SidecarPath(string basePath) {
            var dir = GetDirectoryName(basePath);
            var fileName = GetFileName(basePath);
            if (string.IsNullOrWhiteSpace(fileName)) return basePath + ".relicstats.json";
            return string.IsNullOrWhiteSpace(dir)
                ? "relicstats/" + fileName + ".relicstats.json"
                : dir + "/relicstats/" + fileName + ".relicstats.json";
        }

        static string PhysicalSidecarPath(string basePath) {
            var logicalSidecar = SidecarPath(basePath);
            var fullBase = logicalSidecar;
            try {
                fullBase = Path.IsPathRooted(logicalSidecar)
                    ? logicalSidecar
                    : Path.GetFullPath(logicalSidecar);
            } catch { }
            return fullBase;
        }

        static bool TryWriteWithSaveStore(object? saveStore, string path, string json) {
            if (saveStore == null) return false;
            try {
                var dir = GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(dir)) {
                    var createDirectory = saveStore.GetType().GetMethod("CreateDirectory", new[] { typeof(string) });
                    createDirectory?.Invoke(saveStore, new object[] { dir });
                }

                var writeFile = saveStore.GetType().GetMethod("WriteFile", new[] { typeof(string), typeof(string) });
                if (writeFile == null) return false;
                writeFile.Invoke(saveStore, new object[] { path, json });
                return true;
            } catch {
                return false;
            }
        }

        static bool TryReadWithSaveStore(object? saveStore, string path, out string? json) {
            json = null;
            if (saveStore == null) return false;
            try {
                var fileExists = saveStore.GetType().GetMethod("FileExists", new[] { typeof(string) });
                if (fileExists == null) return false;
                if (fileExists.Invoke(saveStore, new object[] { path }) is not bool exists || !exists) return false;

                var readFile = saveStore.GetType().GetMethod("ReadFile", new[] { typeof(string) });
                if (readFile == null) return false;
                json = readFile.Invoke(saveStore, new object[] { path }) as string;
                return true;
            } catch {
                json = null;
                return false;
            }
        }

        static string? GetDirectoryName(string path) {
            if (string.IsNullOrWhiteSpace(path)) return null;
            var slash = path.LastIndexOf('/');
            var backslash = path.LastIndexOf('\\');
            var index = Math.Max(slash, backslash);
            return index > 0 ? path[..index] : null;
        }

        static string? GetFileName(string path) {
            if (string.IsNullOrWhiteSpace(path)) return null;
            var slash = path.LastIndexOf('/');
            var backslash = path.LastIndexOf('\\');
            var index = Math.Max(slash, backslash);
            return index >= 0 && index < path.Length - 1 ? path[(index + 1)..] : path;
        }

        static SnapshotEnvelope VersionMismatchEnvelope(string? savedVersion) {
            var saved = string.IsNullOrWhiteSpace(savedVersion) ? "unknown" : savedVersion.Trim();
            return new SnapshotEnvelope {
                ModVersion = currentModVersion,
                Counters = new Dictionary<string, Dictionary<string, int>>(),
                TextStats = new Dictionary<string, Dictionary<string, string>>(),
                Note = $"StatTheRelics data was saved by mod version {saved}, but the current mod version is {currentModVersion}. No relic stats are available for this save.",
                StatsUnavailable = true
            };
        }

        static bool IsCompatibleVersion(string? savedVersion) {
            return string.Equals(NormalizeVersion(savedVersion), currentModVersion, StringComparison.Ordinal);
        }

        static string GetCurrentModVersion() {
            try {
                var asm = typeof(RelicStatsPersistence).Assembly;
                var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                var normalizedInfo = NormalizeVersion(info);
                if (!string.IsNullOrWhiteSpace(normalizedInfo)) return normalizedInfo;

                var version = asm.GetName().Version;
                if (version != null) return $"{version.Major}.{version.Minor}.{version.Build}";
            } catch { }

            return "unknown";
        }

        static string NormalizeVersion(string? version) {
            if (string.IsNullOrWhiteSpace(version)) return string.Empty;
            var value = version.Trim();
            var metadataIndex = value.IndexOf('+');
            if (metadataIndex >= 0) value = value[..metadataIndex];
            return value;
        }
    }
}
