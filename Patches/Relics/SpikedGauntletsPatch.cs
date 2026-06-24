using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterPlayerTurnStart), new[] { typeof(PlayerChoiceContext), typeof(Player) })]
    public static class SpikedGauntletsPatch {
        static void Postfix(AbstractModel __instance, PlayerChoiceContext choiceContext, Player player) {
            try {
                _ = choiceContext;
                if (__instance is not SpikedGauntlets gauntlets || player == null || gauntlets.Owner != player) return;

                var energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(gauntlets, "Energy", 1));
                if (energy > 0) RelicTracker.AddAmount(gauntlets, "Energy Given", energy);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class SpikedGauntletsPowerPatch {
        static void Postfix(CardModel __instance) {
            try {
                if (__instance == null || Convert.ToInt32(__instance.Type) != 3) return;
                var relic = ReflectionUtil.FindRelic<SpikedGauntlets>(__instance.Owner);
                if (relic == null) return;

                RelicTracker.AddAmount(relic, "Powers Played", 1);
            } catch { }
        }
    }
}
