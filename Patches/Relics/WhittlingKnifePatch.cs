using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Entities.Players;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(WhittlingKnife), nameof(WhittlingKnife.AfterShuffle))]
    public static class WhittlingKnifePatch {
        static void Prefix(WhittlingKnife __instance, Player shuffler, out int __state) {
            __state = 0;
            try {
                if (shuffler != __instance.Owner) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "AimPower"));
            } catch { }
        }

        static void Postfix(WhittlingKnife __instance, Task __result, int __state) {
            try {
                if (__state <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Aim Gained", __state);
                    } catch { }
                });
            } catch { }
        }
    }
}
