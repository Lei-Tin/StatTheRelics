using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Logging;
using HarmonyLib;
using StatTheRelics.Patches;
using StatTheRelics.RelicStats;
using System.Reflection;

[ModInitializer("Initialize")] 
public class ModEntry { 
    const string HarmonyId = "StatTheRelics.patch";

    public static void Initialize() { 
        var harmony = new Harmony(HarmonyId);

        try {
            ModLog.Info("ModEntry: initialization started");

            // Definitions are needed before patching so a failed relic patch can be
            // isolated to its owner and switched to the generic Flash counter.
            ModLog.Info("ModEntry: registering relic stat definitions");
            RelicStatsRegistry.RegisterAllFromAssembly(typeof(ModEntry).Assembly);

            ModLog.Info("ModEntry: applying declared Harmony patches");
            ApplyDeclaredPatches(harmony, typeof(ModEntry).Assembly);

            ModLog.Info("ModEntry: applying dynamic relic patches");
            RelicTracker.RelicPatches.ApplyDynamicPatches(harmony);

            ModLog.Info("ModEntry: applying save/history patches");
            RelicStatsSavePatches.Apply(harmony);

            ModLog.Info("ModEntry: initialization complete");
        } catch (Exception ex) {
            ModLog.Info($"ModEntry: initialization failed, rolling back all patches - {ex}");
            TryRollbackAllPatches();

            // Throw an exception so that mod loading fails
            throw new Exception("StatTheRelics failed to initialize. See log for details.", ex);
        }
    }

    static void ApplyDeclaredPatches(Harmony harmony, Assembly assembly) {
        var patchTypes = assembly.GetTypes()
            .Where(IsHarmonyPatchClass)
            .ToArray();

        foreach (var patchType in patchTypes.Where(type => type.Namespace != "StatTheRelics.Patches.Relics")) {
            harmony.CreateClassProcessor(patchType).Patch();
        }

        var relicPatchGroups = patchTypes
            .Where(type => type.Namespace == "StatTheRelics.Patches.Relics")
            .Select(type => new {
                PatchType = type,
                RelicTypeName = RelicStatsRegistry.ResolveRelicTypeNameForPatchClass(type)
            })
            .ToArray();

        var unmapped = relicPatchGroups.FirstOrDefault(item => item.RelicTypeName == null);
        if (unmapped != null) {
            throw new InvalidOperationException($"Cannot determine the relic owner of patch class {unmapped.PatchType.FullName}");
        }

        foreach (var group in relicPatchGroups.GroupBy(item => item.RelicTypeName!)) {
            ApplyRelicPatchGroup(harmony, group.Key, group.Select(item => item.PatchType));
        }
    }

    static bool IsHarmonyPatchClass(Type type) {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        return type.GetCustomAttributes(typeof(HarmonyPatch), false).Length > 0
            || type.GetMethods(flags).Any(method => method.GetCustomAttributes(typeof(HarmonyPatch), false).Length > 0);
    }

    static void ApplyRelicPatchGroup(Harmony harmony, string relicTypeName, IEnumerable<Type> patchTypes) {
        var processors = new List<PatchClassProcessor>();
        try {
            foreach (var patchType in patchTypes) {
                var processor = harmony.CreateClassProcessor(patchType);
                processors.Add(processor);
                processor.Patch();
            }
        } catch (Exception ex) {
            for (var i = processors.Count - 1; i >= 0; i--) {
                try { processors[i].Unpatch(); } catch { }
            }

            RelicStatsRegistry.MarkImplementationChanged(relicTypeName);
            ModLog.Info(
                $"ModEntry: relic implementation changed for {relicTypeName}; " +
                $"using Flash fallback - {ex.GetType().Name}: {ex.Message}"
            );
        }
    }

    static void TryRollbackAllPatches() {
        try {
            var harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);
        } catch (Exception rollbackEx) {
            ModLog.Info($"ModEntry: rollback failed - {rollbackEx}");
        }
    }
}
