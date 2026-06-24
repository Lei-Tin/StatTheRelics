using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(MiniRegent), nameof(MiniRegent.AfterStarsSpent))]
    public static class MiniRegentPatch {
        static void Prefix(MiniRegent __instance, Player spender, ref object __state) {
            try {
                var usedThisTurn = ReflectionUtil.GetMemberValue(__instance, "UsedThisTurn") is bool used && used;
                if (__instance == null || spender == null || __instance.Owner != spender || usedThisTurn) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Strength", 1));
            } catch { }
        }

        static void Postfix(MiniRegent __instance, Task __result, object __state) {
            try {
                var amount = __state is int value ? value : 0;
                if (amount <= 0 || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Strength Gained", amount);
                    } catch { }
                });
            } catch { }
        }
    }
}
