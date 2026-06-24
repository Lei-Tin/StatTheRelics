using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(RunicPyramid), nameof(RunicPyramid.ShouldFlush))]
    public static class RunicPyramidPatch {
        static int lastCombatHash;
        static int lastTurn = -1;

        static void Postfix(RunicPyramid __instance, Player player, bool __result) {
            try {
                if (__result || __instance == null || player == null || __instance.Owner != player) return;

                var combatState = ReflectionUtil.GetMemberValue(player.Creature, "CombatState");
                var combatHash = combatState?.GetHashCode() ?? 0;
                var turn = player.PlayerCombatState?.TurnNumber ?? -1;
                if (combatHash == lastCombatHash && turn == lastTurn) return;
                lastCombatHash = combatHash;
                lastTurn = turn;

                var hand = PileType.Hand.GetPile(player);
                var count = hand?.Cards?.Count ?? 0;
                if (count > 0) RelicTracker.AddAmount(__instance, "Cards Retained", count);
            } catch { }
        }
    }
}
