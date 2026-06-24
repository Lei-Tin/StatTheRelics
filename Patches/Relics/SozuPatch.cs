using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterPlayerTurnStart), new[] { typeof(PlayerChoiceContext), typeof(Player) })]
    public static class SozuPatch {
        static void Postfix(AbstractModel __instance, PlayerChoiceContext choiceContext, Player player) {
            try {
                _ = choiceContext;
                if (__instance is not Sozu sozu || player == null || sozu.Owner != player) return;

                var energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(sozu, "Energy", 1));
                if (energy > 0) RelicTracker.AddAmount(sozu, "Energy Given", energy);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(Sozu), nameof(Sozu.ShouldProcurePotion))]
    public static class SozuPotionPatch {
        static void Postfix(Sozu __instance, PotionModel potion, Player player, bool __result) {
            try {
                _ = potion;
                if (__instance == null || player == null || __instance.Owner != player) return;
                if (__result) return;

                RelicTracker.AddAmount(__instance, "Potions Confiscated", 1);
            } catch { }
        }
    }
}
