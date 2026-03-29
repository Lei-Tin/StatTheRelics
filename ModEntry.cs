using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Logging;
using HarmonyLib;
using StatTheRelics.Patches;
using StatTheRelics.RelicStats;

[ModInitializer("Initialize")] 
public class ModEntry { 
    public static void Initialize() { 
        var harmony = new Harmony("StatTheRelics.patch");
        harmony.PatchAll();
        RelicTracker.RelicPatches.ApplyDynamicPatches(harmony);
        ManualRelicPatches.Apply(harmony);
        RelicStatsSavePatches.Apply(harmony);

        // Load stat definitions (one class per relic)
        RelicStatsRegistry.RegisterAllFromAssembly(typeof(ModEntry).Assembly);
    } 
}