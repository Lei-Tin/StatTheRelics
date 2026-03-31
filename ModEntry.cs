using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Logging;
using HarmonyLib;
using StatTheRelics.Patches;
using StatTheRelics.RelicStats;

[ModInitializer("Initialize")] 
public class ModEntry { 
    public static void Initialize() { 
        var harmony = new Harmony("StatTheRelics.patch");
        
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
    } 
}