using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(GalacticDust), nameof(GalacticDust.AfterStarsSpent))]
    public static class GalacticDustPatch {
        class StarsState {
            public int Amount { get; set; }
            public int BeforeStars { get; set; }
            public int Threshold { get; set; }
            public int BlockPerTrigger { get; set; }
        }

        static void Prefix(GalacticDust __instance, int amount, Player spender, ref object __state) {
            try {
                if (__instance == null || spender == null || __instance.Owner != spender || amount <= 0) return;
                __state = new StarsState {
                    Amount = amount,
                    BeforeStars = __instance.StarsSpent,
                    Threshold = Math.Max(1, ReflectionUtil.GetDynamicVarIntValue(__instance, "Stars", 10)),
                    BlockPerTrigger = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Block", 10))
                };
            } catch { }
        }

        static void Postfix(GalacticDust __instance, Task __result, object __state) {
            try {
                var state = __state as StarsState;
                if (state == null) return;

                if (__result == null) {
                    Count(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, state);
                    } catch { }
                });
            } catch { }
        }

        static void Count(GalacticDust relic, StarsState state) {
            try {
                var triggers = (state.BeforeStars + state.Amount) / state.Threshold;
                if (triggers <= 0) return;
                RelicTracker.AddAmount(relic, "Block Gained", triggers * state.BlockPerTrigger);
            } catch { }
        }
    }
}
