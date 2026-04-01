using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Logging;
using HarmonyLib;
using StatTheRelics.Patches;
using StatTheRelics.RelicStats;

[ModInitializer("Initialize")] 
public class ModEntry { 
    const string HarmonyId = "StatTheRelics.patch";

    public static void Initialize() { 
        var harmony = new Harmony(HarmonyId);

        try {
            ModLog.Info("ModEntry: applying Harmony patches");
            harmony.PatchAll();

            ModLog.Info("ModEntry: applying dynamic relic patches");
            RelicTracker.RelicPatches.ApplyDynamicPatches(harmony);

            ModLog.Info("ModEntry: applying relic stats save patches");
            RelicStatsSavePatches.Apply(harmony);

            // Load stat definitions (one class per relic)
            ModLog.Info("ModEntry: registering relic stat definitions");
            RelicStatsRegistry.RegisterAllFromAssembly(typeof(ModEntry).Assembly);

            ModLog.Info("ModEntry: initialization complete");
        } catch (Exception ex) {
            ModLog.Info($"ModEntry: initialization failed, rolling back all patches - {ex}");
            TryRollbackAllPatches();
            ModLog.Info("ModEntry: mod disabled for this session");
        }
    } 

    static void TryRollbackAllPatches() {
        try {
            var harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);
            ModLog.Info("ModEntry: rollback complete");
        } catch (Exception rollbackEx) {
            ModLog.Info($"ModEntry: rollback failed - {rollbackEx}");
        }
    }
}