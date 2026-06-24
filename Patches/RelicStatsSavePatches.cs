using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using StatTheRelics.RelicStats;

namespace StatTheRelics.Patches {
    // Targeted save/load hooks to persist relic stats alongside run saves and run history files.
    internal static class RelicStatsSavePatches {
        public static void Apply(Harmony harmony) {
            PatchSaveRun(harmony);
            PatchLoadRun(harmony);
            PatchInitializeSavedRun(harmony);
            PatchSaveHistory(harmony);
            PatchLoadHistory(harmony);
        }

        static void PatchSaveRun(Harmony harmony) {
            var target = AccessTools.DeclaredMethod(typeof(RunSaveManager), "SaveRun", new[] { typeof(AbstractRoom) })
                         ?? throw new MissingMethodException("RunSaveManager.SaveRun(AbstractRoom) not found");
            harmony.Patch(target, postfix: new HarmonyMethod(typeof(RelicStatsSavePatches), nameof(AfterSaveRun)));
        }

        static void PatchLoadRun(Harmony harmony) {
            var target = AccessTools.DeclaredMethod(typeof(RunSaveManager), "LoadRunSave", Type.EmptyTypes)
                         ?? throw new MissingMethodException("RunSaveManager.LoadRunSave() not found");
            harmony.Patch(target, postfix: new HarmonyMethod(typeof(RelicStatsSavePatches), nameof(AfterLoadRunSave)));
        }

        static void PatchInitializeSavedRun(Harmony harmony) {
            var initSavedRun = AccessTools.DeclaredMethod(typeof(RunManager), "InitializeSavedRun", new[] { typeof(SerializableRun) });
            var setupSavedSinglePlayer = AccessTools.DeclaredMethod(typeof(RunManager), "SetUpSavedSinglePlayer", new[] { typeof(RunState), typeof(SerializableRun) });
            var target = initSavedRun ?? setupSavedSinglePlayer;
            if (target == null) {
                throw new MissingMethodException("RunManager.InitializeSavedRun(SerializableRun) or RunManager.SetUpSavedSinglePlayer(RunState, SerializableRun) not found");
            }

            harmony.Patch(target, postfix: new HarmonyMethod(typeof(RelicStatsSavePatches), nameof(AfterInitializeSavedRun)));
        }

        static void PatchSaveHistory(Harmony harmony) {
            var target = AccessTools.DeclaredMethod(typeof(RunHistorySaveManager), "SaveHistoryInternal", new[] { typeof(string), typeof(string) })
                         ?? throw new MissingMethodException("RunHistorySaveManager.SaveHistoryInternal(string, string) not found");
            harmony.Patch(target, postfix: new HarmonyMethod(typeof(RelicStatsSavePatches), nameof(AfterSaveHistoryInternal)));
        }

        static void PatchLoadHistory(Harmony harmony) {
            var target = AccessTools.DeclaredMethod(typeof(RunHistorySaveManager), "LoadHistory", new[] { typeof(string) })
                         ?? throw new MissingMethodException("RunHistorySaveManager.LoadHistory(string) not found");
            harmony.Patch(target, postfix: new HarmonyMethod(typeof(RelicStatsSavePatches), nameof(AfterLoadHistory)));
        }

        static void AfterSaveRun(RunSaveManager __instance) {
            try {
                var path = GetProperty<string>(__instance, "CurrentRunSavePath");
                if (string.IsNullOrEmpty(path)) return;

                RelicStatsPersistence.SaveSnapshot(path, GetField<object>(__instance, "_saveStore"));
            } catch { }
        }

        static void AfterLoadRunSave(RunSaveManager __instance) {
            try {
                var path = GetProperty<string>(__instance, "CurrentRunSavePath");
                if (string.IsNullOrEmpty(path)) return;

                RelicStatsPersistence.StageRunSnapshot(path, GetField<object>(__instance, "_saveStore"));
            } catch { }
        }

        static void AfterInitializeSavedRun(RunManager __instance) {
            try {
                RelicStatsPersistence.ApplyStagedRunSnapshot();
            } catch { }
        }

        static void AfterSaveHistoryInternal(RunHistorySaveManager __instance, string path, string content) {
            try {
                if (string.IsNullOrEmpty(path)) return;
                RelicStatsPersistence.SaveSnapshot(path, GetField<object>(__instance, "_saveStore"));
            } catch { }
        }

        static void AfterLoadHistory(RunHistorySaveManager __instance, string fileName, ReadSaveResult<RunHistory> __result) {
            try {
                var historyPath = GetProperty<string?>(__instance, "HistoryPath");
                if (string.IsNullOrEmpty(historyPath)) return;

                var basePath = Path.Combine(historyPath, fileName ?? string.Empty);
                RelicStatsPersistence.StageHistorySnapshot(basePath, GetField<object>(__instance, "_saveStore"));
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

        static T? GetField<T>(object obj, string name) {
            try {
                var field = AccessTools.Field(obj.GetType(), name);
                if (field == null) return default;
                return (T?)field.GetValue(obj);
            } catch { return default; }
        }
    }
}
