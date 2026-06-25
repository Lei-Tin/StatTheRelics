using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using MegaCrit.Sts2.Core.Saves;

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
                    Note = string.Empty
                };
                var json = JsonSerializer.Serialize(envelope, jsonOptions);
                var path = SidecarPath(basePath);

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
                // Do not override a staged run load.
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
                var saveStoreType = saveStore?.GetType().FullName ?? "null";

                ModLog.Info($"RelicStatsPersistence: loading {label} sidecar basePath={basePath}, sidecarPath={path}, saveStore={saveStoreType}");

                if (TryReadWithSaveStore(saveStore, path, label, out var storeJson)) {
                    json = storeJson;
                } else {
                    var physicalPath = PhysicalSidecarPath(basePath);
                    var physicalExists = File.Exists(physicalPath);
                    ModLog.Info($"RelicStatsPersistence: physical fallback {label} path={physicalPath}, exists={physicalExists}");
                    if (!physicalExists) return null;
                    json = File.ReadAllText(physicalPath);
                    ModLog.Info($"RelicStatsPersistence: physical read {label} path={physicalPath}, length={json.Length}");
                }

                if (string.IsNullOrWhiteSpace(json)) return null;

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
            var normalized = NormalizeStorePath(basePath);
            var dir = GetDirectoryName(normalized);
            var fileName = GetFileName(normalized);
            if (string.IsNullOrWhiteSpace(fileName)) return normalized + ".relicstats.json";

            return string.IsNullOrWhiteSpace(dir)
                ? "relicstats/" + fileName + ".relicstats.json"
                : dir + "/relicstats/" + fileName + ".relicstats.json";
        }

        static string PhysicalSidecarPath(string basePath) {
            var logicalSidecar = SidecarPath(basePath);
            try {
                return Path.IsPathRooted(logicalSidecar) ? logicalSidecar : Path.GetFullPath(logicalSidecar);
            } catch {
                return logicalSidecar;
            }
        }

        static bool TryWriteWithSaveStore(object? saveStore, string path, string json) {
            if (saveStore is not ISaveStore store) return false;
            try {
                var dir = GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(dir)) store.CreateDirectory(dir);
                store.WriteFile(path, json);
                return true;
            } catch {
                return false;
            }
        }

        static bool TryReadWithSaveStore(object? saveStore, string path, string label, out string? json) {
            json = null;
            if (saveStore is not ISaveStore store) {
                var saveStoreType = saveStore?.GetType().FullName ?? "null";
                ModLog.Info($"RelicStatsPersistence: no ISaveStore for {label} sidecar path={path}, saveStore={saveStoreType}");
                return false;
            }
            try {
                var exists = store.FileExists(path);
                ModLog.Info($"RelicStatsPersistence: ISaveStore FileExists {label} path={path}, exists={exists}, store={store.GetType().FullName}");
                if (!exists) return false;
                json = store.ReadFile(path);
                ModLog.Info($"RelicStatsPersistence: ISaveStore read {label} path={path}, length={json?.Length ?? 0}");
                return true;
            } catch (Exception ex) {
                ModLog.Info($"RelicStatsPersistence: ISaveStore read failed for {label} path={path} - {ex.GetType().Name}: {ex.Message}");
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

        static string NormalizeStorePath(string path) {
            return (path ?? string.Empty).Replace('\\', '/');
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
