using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(OldCoin), nameof(OldCoin.AfterObtained))]
    public static class OldCoinPatch {
        static void Prefix(OldCoin __instance, ref object __state) {
            try {
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Gold", 300));
            } catch { }
        }

        static void Postfix(OldCoin __instance, Task __result, object __state) {
            try {
                var amount = __state is int value ? value : 0;
                if (amount <= 0 || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Gold Gained", amount);
                    } catch { }
                });
            } catch { }
        }
    }
}
