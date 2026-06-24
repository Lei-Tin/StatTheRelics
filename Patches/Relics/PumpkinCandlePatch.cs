using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PumpkinCandle), nameof(PumpkinCandle.Rekindle))]
    public static class PumpkinCandleRekindlePatch {
        static void Postfix(PumpkinCandle __instance) {
            try {
                RelicTracker.AddAmount(__instance, "Times Rekindled", 1);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterPlayerTurnStart), new[] { typeof(PlayerChoiceContext), typeof(Player) })]
    public static class PumpkinCandleEnergyPatch {
        static void Postfix(AbstractModel __instance, PlayerChoiceContext choiceContext, Player player) {
            try {
                if (__instance is not PumpkinCandle pumpkinCandle || player == null) return;
                if (pumpkinCandle.Owner != player) return;

                var kindleCount = ReflectionUtil.GetIntMemberValue(pumpkinCandle, "KindleCount");
                if (kindleCount <= 0) return;

                var energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(pumpkinCandle, "Energy", 1));
                if (energy <= 0) return;
                RelicTracker.AddAmount(pumpkinCandle, "Energy Given", energy);
            } catch { }
        }
    }
}
