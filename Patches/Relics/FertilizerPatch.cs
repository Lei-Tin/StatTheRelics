using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Fertilizer), nameof(Fertilizer.AfterCardGeneratedForCombat))]
    public static class FertilizerPatch {
        static void Prefix(Fertilizer __instance, CardModel card, Player creator, out int __state) {
            __state = 0;
            try {
                if (creator != __instance.Owner || card is not Sadness) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "StrengthPower"));
            } catch { }
        }

        static void Postfix(Fertilizer __instance, Task __result, int __state) {
            try {
                if (__state <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Strength Gained", __state);
                    } catch { }
                });
            } catch { }
        }
    }
}
