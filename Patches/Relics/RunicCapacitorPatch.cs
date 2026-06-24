using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(RunicCapacitor), nameof(RunicCapacitor.AfterSideTurnStart))]
    public static class RunicCapacitorPatch {
        static void Prefix(RunicCapacitor __instance, ref object __state) {
            try {
                __state = ReflectionUtil.GetDynamicVarIntValue(__instance, "Repeat", 3);
            } catch { }
        }

        static void Postfix(RunicCapacitor __instance, Task __result, object __state) {
            try {
                if (__state is not int slots || slots <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Orb Slots Added", slots);
                    } catch { }
                });
            } catch { }
        }
    }
}
