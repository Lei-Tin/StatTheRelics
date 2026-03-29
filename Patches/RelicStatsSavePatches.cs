using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Saves.Managers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using StatTheRelics.RelicStats;
using System.Reflection;

namespace StatTheRelics.Patches {
    // Targeted save/load hooks to persist relic stats alongside run saves and run history files.
    internal static class RelicStatsSavePatches {
        public static void Apply(Harmony harmony) {
            PatchIfFound(harmony, AccessTools.Method(typeof(RunSaveManager), "SaveRun"), nameof(AfterSaveRun));
            PatchIfFound(harmony, AccessTools.Method(typeof(RunSaveManager), "LoadRunSave"), nameof(AfterLoadRunSave));

            // Saved-run initialization (single-player). If InitializeSavedRun is absent, fall back to SetUpSavedSinglePlayer.
            var initSaved = AccessTools.Method(typeof(RunManager), "InitializeSavedRun")
                            ?? AccessTools.Method(typeof(RunManager), "SetUpSavedSinglePlayer");
            PatchIfFound(harmony, initSaved, nameof(AfterInitializeSavedRun));

            PatchIfFound(harmony, AccessTools.Method(typeof(RunHistorySaveManager), "SaveHistoryInternal"), nameof(AfterSaveHistoryInternal));
            PatchIfFound(harmony, AccessTools.Method(typeof(RunHistorySaveManager), "LoadHistory"), nameof(AfterLoadHistory));
        }

        static void PatchIfFound(Harmony harmony, MethodInfo original, string handlerName) {
            if (original == null) return;
            var hm = new HarmonyMethod(typeof(RelicStatsSavePatches).GetMethod(handlerName, BindingFlags.Static | BindingFlags.NonPublic));
            harmony.Patch(original, postfix: hm);
        }

        static void AfterSaveRun(RunSaveManager __instance) {
            try {
                var path = GetProperty<string>(__instance, "CurrentRunSavePath");
                if (!string.IsNullOrEmpty(path)) RelicStatsPersistence.SaveSnapshot(path);
            } catch { }
        }

        static void AfterLoadRunSave(RunSaveManager __instance) {
            try {
                var path = GetProperty<string>(__instance, "CurrentRunSavePath");
                if (!string.IsNullOrEmpty(path)) RelicStatsPersistence.StageRunSnapshot(path);
            } catch { }
        }

        static void AfterInitializeSavedRun(RunManager __instance) {
            try { RelicStatsPersistence.ApplyStagedRunSnapshot(); } catch { }
        }

        static void AfterSaveHistoryInternal(string path, string content) {
            try {
                if (!string.IsNullOrEmpty(path)) RelicStatsPersistence.SaveSnapshot(path);
            } catch { }
        }

        static void AfterLoadHistory(RunHistorySaveManager __instance, string fileName, ReadSaveResult<RunHistory> __result) {
            try {
                var historyPath = GetProperty<string>(__instance, "HistoryPath");
                var basePath = System.IO.Path.Combine(historyPath ?? string.Empty, fileName);
                RelicStatsPersistence.StageHistorySnapshot(basePath);
            } catch { }
        }

        static T GetProperty<T>(object obj, string name) {
            try {
                var getter = AccessTools.PropertyGetter(obj.GetType(), name) ?? AccessTools.Method(obj.GetType(), "get_" + name);
                if (getter == null) return default;
                return (T)getter.Invoke(obj, null);
            } catch { return default; }
        }
    }
}