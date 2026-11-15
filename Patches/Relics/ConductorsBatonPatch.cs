using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ConductorsBaton), nameof(ConductorsBaton.AfterCardPlayed))]
    public static class ConductorsBatonPatch {
        static void Prefix(ConductorsBaton __instance, out bool __state) {
            __state = ReflectionUtil.GetMemberValue(__instance, "_triggeredThisTurn") is true;
        }

        static void Postfix(ConductorsBaton __instance, Task __result, bool __state) {
            try {
                if (__state || __result == null) return;
                if (ReflectionUtil.GetMemberValue(__instance, "_triggeredThisTurn") is not true) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Damage Reduction Activations", 1);
                    } catch { }
                });
            } catch { }
        }
    }
}
