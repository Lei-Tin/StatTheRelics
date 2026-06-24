using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Kunai), "DoActivateVisuals")]
    public static class KunaiPatch {
        static void Postfix(Kunai __instance, Task __result) {
            try {
                var dexterity = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Dexterity", 1));
                if (dexterity <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Dexterity Gained", dexterity);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Dexterity Gained", dexterity);
                    } catch { }
                });
            } catch { }
        }
    }
}
