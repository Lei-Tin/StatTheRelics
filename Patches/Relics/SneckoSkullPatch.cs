using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(SneckoSkull), nameof(SneckoSkull.AfterModifyingPowerAmountGiven))]
    public static class SneckoSkullPatch {
        static void Postfix(SneckoSkull __instance, PowerModel power) {
            try {
                if (__instance == null || power is not PoisonPower) return;

                var amount = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Poison", 1));
                if (amount > 0) RelicTracker.AddAmount(__instance, "Poison Added", amount);
            } catch { }
        }
    }
}
