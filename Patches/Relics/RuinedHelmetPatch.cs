using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(RuinedHelmet), nameof(RuinedHelmet.TryModifyPowerAmountReceived))]
    public static class RuinedHelmetPatch {
        static void Postfix(RuinedHelmet __instance, Creature target, decimal amount, ref decimal modifiedAmount, bool __result) {
            try {
                if (!__result || __instance == null || target == null || __instance.Owner?.Creature != target) return;
                var bonus = decimal.ToInt32(Math.Max(0m, modifiedAmount - amount));
                if (bonus > 0) RelicTracker.AddAmount(__instance, "Bonus Strength Gained", bonus);
            } catch { }
        }
    }
}
