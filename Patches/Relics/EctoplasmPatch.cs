using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
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

    [HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterPlayerTurnStart), new[] { typeof(PlayerChoiceContext), typeof(Player) })]
    public static class EctoplasmEnergyPatch {
        static void Postfix(AbstractModel __instance, PlayerChoiceContext choiceContext, Player player) {
            try {
                _ = choiceContext;
                if (__instance is not Ectoplasm ectoplasm || player == null || ectoplasm.Owner != player) return;

                var energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(ectoplasm, "Energy", 1));
                if (energy > 0) RelicTracker.AddAmount(ectoplasm, "Energy Gained", energy);
            } catch { }
        }
    }
}
