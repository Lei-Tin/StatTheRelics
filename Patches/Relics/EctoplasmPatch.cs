using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Ectoplasm), nameof(Ectoplasm.ModifyGoldGained))]
    public static class EctoplasmPatch {
        static void Postfix(Ectoplasm __instance, Player player, decimal amount, decimal __result) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                var prevented = amount - __result;
                if (prevented <= 0) return;
                RelicTracker.AddAmount(__instance, "Gold Prevented", Convert.ToInt32(prevented));
            } catch { }
        }
    }
}
