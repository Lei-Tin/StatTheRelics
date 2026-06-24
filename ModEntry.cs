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
            harmony.PatchAll();

            RelicTracker.RelicPatches.ApplyDynamicPatches(harmony);

            RelicStatsSavePatches.Apply(harmony);

            // Load stat definitions (one class per relic)
            RelicStatsRegistry.RegisterAllFromAssembly(typeof(ModEntry).Assembly);
        } catch (Exception ex) {
            ModLog.Info($"ModEntry: initialization failed, rolling back all patches - {ex}");
            TryRollbackAllPatches();

            // Throw an exception so that mod loading fails
            throw new Exception("StatTheRelics failed to initialize. See log for details.", ex);
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
