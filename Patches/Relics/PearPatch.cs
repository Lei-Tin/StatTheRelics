using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Pear), nameof(Pear.AfterObtained))]
    public static class PearPatch {
        static void Postfix(Pear __instance, Task __result) {
            try {
                var amount = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "MaxHp", 10));
                if (amount <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Max HP Gained", amount);
                    } catch { }
                });
            } catch { }
        }
    }
}
