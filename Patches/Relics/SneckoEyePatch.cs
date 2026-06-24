using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(SneckoEye), nameof(SneckoEye.ModifyHandDraw))]
    public static class SneckoEyePatch {
        static void Postfix(SneckoEye __instance, Player player, decimal count, ref decimal __result) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                var extraDraw = __result - count;
                if (extraDraw > 0) RelicTracker.AddAmount(__instance, "Cards Drawn", Convert.ToInt32(extraDraw));
            } catch { }
        }
    }

    [HarmonyPatch(typeof(ConfusedPower), nameof(ConfusedPower.AfterCardDrawn))]
    public static class SneckoEyeConfusedPowerPatch {
        static void Postfix(ConfusedPower __instance, CardModel card) {
            try {
                var relic = ReflectionUtil.FindRelic<SneckoEye>(__instance?.Owner);
                if (relic == null) return;

                var cost = card?.EnergyCost?.GetWithModifiers(CostModifiers.Local) ?? -1;
                if (cost < 0 || cost > 3) return;

                RelicTracker.AddAmount(relic, $"{cost} Cost", 1);
            } catch { }
        }
    }
}
