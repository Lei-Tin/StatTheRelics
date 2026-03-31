using System;
using System.IO;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using StatTheRelics.RelicStats;

namespace StatTheRelics.Patches {
    // Targeted save/load hooks to persist relic stats alongside run saves and run history files.
    internal static class RelicStatsSavePatches {
        public static void Apply(Harmony harmony) {
            ModLog.Info("RelicStatsSavePatches: Apply invoked");
            PatchSaveRun(harmony);
            PatchLoadRun(harmony);
            PatchInitializeSavedRun(harmony);
            PatchSaveHistory(harmony);
            PatchLoadHistory(harmony);
        }

        static void PatchSaveRun(Harmony harmony) {
            var target = AccessTools.DeclaredMethod(typeof(RunSaveManager), "SaveRun")
                         ?? throw new MissingMethodException("RunSaveManager.SaveRun not found");
            harmony.Patch(target, postfix: new HarmonyMethod(typeof(RelicStatsSavePatches), nameof(AfterSaveRun)));
            ModLog.Info($"RelicStatsSavePatches: patched {target.DeclaringType?.FullName}.{target.Name} -> AfterSaveRun");
        }

        static void PatchLoadRun(Harmony harmony) {
            var target = AccessTools.DeclaredMethod(typeof(RunSaveManager), "LoadRunSave")
                         ?? throw new MissingMethodException("RunSaveManager.LoadRunSave not found");
            harmony.Patch(target, postfix: new HarmonyMethod(typeof(RelicStatsSavePatches), nameof(AfterLoadRunSave)));
            ModLog.Info($"RelicStatsSavePatches: patched {target.DeclaringType?.FullName}.{target.Name} -> AfterLoadRunSave");
        }

        static void PatchInitializeSavedRun(Harmony harmony) {
            var initSavedRun = AccessTools.DeclaredMethod(typeof(RunManager), "InitializeSavedRun");
            var setupSavedSinglePlayer = AccessTools.DeclaredMethod(typeof(RunManager), "SetUpSavedSinglePlayer");
            var target = initSavedRun ?? setupSavedSinglePlayer;
            if (target == null) {
                throw new MissingMethodException("RunManager.InitializeSavedRun or RunManager.SetUpSavedSinglePlayer not found");
            }

            harmony.Patch(target, postfix: new HarmonyMethod(typeof(RelicStatsSavePatches), nameof(AfterInitializeSavedRun)));
            ModLog.Info($"RelicStatsSavePatches: patched {target.DeclaringType?.FullName}.{target.Name} -> AfterInitializeSavedRun");
        }

        static void PatchSaveHistory(Harmony harmony) {
            var target = AccessTools.DeclaredMethod(typeof(RunHistorySaveManager), "SaveHistoryInternal")
                         ?? throw new MissingMethodException("RunHistorySaveManager.SaveHistoryInternal not found");
            harmony.Patch(target, postfix: new HarmonyMethod(typeof(RelicStatsSavePatches), nameof(AfterSaveHistoryInternal)));
            ModLog.Info($"RelicStatsSavePatches: patched {target.DeclaringType?.FullName}.{target.Name} -> AfterSaveHistoryInternal");
        }

        static void PatchLoadHistory(Harmony harmony) {
            var target = AccessTools.DeclaredMethod(typeof(RunHistorySaveManager), "LoadHistory")
                         ?? throw new MissingMethodException("RunHistorySaveManager.LoadHistory not found");
            harmony.Patch(target, postfix: new HarmonyMethod(typeof(RelicStatsSavePatches), nameof(AfterLoadHistory)));
            ModLog.Info($"RelicStatsSavePatches: patched {target.DeclaringType?.FullName}.{target.Name} -> AfterLoadHistory");
        }

        static void AfterSaveRun(RunSaveManager __instance) {
            try {
                ModLog.Info("RelicStatsSavePatches: AfterSaveRun invoked");
                var path = GetProperty<string>(__instance, "CurrentRunSavePath");
                if (string.IsNullOrEmpty(path)) return;

                ModLog.Info($"RelicStatsSavePatches: AfterSaveRun saving sidecar for {path}");
                RelicStatsPersistence.SaveSnapshot(path);
            } catch { }
        }

        static void AfterLoadRunSave(RunSaveManager __instance) {
            try {
                ModLog.Info("RelicStatsSavePatches: AfterLoadRunSave invoked");
                var path = GetProperty<string>(__instance, "CurrentRunSavePath");
                if (string.IsNullOrEmpty(path)) return;

                ModLog.Info($"RelicStatsSavePatches: AfterLoadRunSave staging sidecar from {path}");
                RelicStatsPersistence.StageRunSnapshot(path);
            } catch { }
        }

        static void AfterInitializeSavedRun(RunManager __instance) {
            try {
                ModLog.Info("RelicStatsSavePatches: AfterInitializeSavedRun invoked");
                ModLog.Info("RelicStatsSavePatches: AfterInitializeSavedRun applying staged snapshot");
                RelicStatsPersistence.ApplyStagedRunSnapshot();
            } catch { }
        }

        static void AfterSaveHistoryInternal(RunHistorySaveManager __instance, string path, string content) {
            try {
                ModLog.Info("RelicStatsSavePatches: AfterSaveHistoryInternal invoked");
                var historyPath = GetProperty<string?>(__instance, "HistoryPath");
                if (string.IsNullOrEmpty(historyPath)) return;

                var fileName = Path.GetFileName(path ?? string.Empty);
                var basePath = string.IsNullOrEmpty(fileName)
                    ? historyPath
                    : Path.Combine(historyPath, fileName);

                ModLog.Info($"RelicStatsSavePatches: AfterSaveHistoryInternal saving sidecar for {basePath}");
                RelicStatsPersistence.SaveSnapshot(basePath);
            } catch { }
        }

        static void AfterLoadHistory(RunHistorySaveManager __instance, string fileName, ReadSaveResult<RunHistory> __result) {
            try {
                ModLog.Info("RelicStatsSavePatches: AfterLoadHistory invoked");
                var historyPath = GetProperty<string?>(__instance, "HistoryPath");
                if (string.IsNullOrEmpty(historyPath)) return;

                var basePath = Path.Combine(historyPath, Path.GetFileName(fileName ?? string.Empty));
                ModLog.Info($"RelicStatsSavePatches: AfterLoadHistory staging history snapshot from {basePath}");
                RelicStatsPersistence.StageHistorySnapshot(basePath);
            } catch { }
        }

        // Minimal helper for property access without needing explicit references to game assemblies.
        static T? GetProperty<T>(object obj, string name) {
            try {
                var getter = AccessTools.PropertyGetter(obj.GetType(), name) ?? AccessTools.Method(obj.GetType(), "get_" + name);
                if (getter == null) return default;
                return (T?)getter.Invoke(obj, null);
            } catch { return default; }
        }
    }
}