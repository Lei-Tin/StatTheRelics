using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Orichalcum), nameof(Orichalcum.BeforeSideTurnEnd))]
    public static class OrichalcumPatch {
        static void Prefix(Orichalcum __instance, ref object __state) {
            try {
                if (ReflectionUtil.GetMemberValue(__instance, "ShouldTrigger") is not bool shouldTrigger || !shouldTrigger) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Block", 6));
            } catch { }
        }

        static void Postfix(Orichalcum __instance, Task __result, object __state) {
            try {
                var amount = __state is int value ? value : 0;
                if (amount <= 0 || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Block Gained", amount);
                    } catch { }
                });
            } catch { }
        }
    }
}
