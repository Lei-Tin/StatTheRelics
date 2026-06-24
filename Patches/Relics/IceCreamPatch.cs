using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(IceCream), nameof(IceCream.ShouldPlayerResetEnergy))]
    public static class IceCreamPatch {
        sealed class LastCountedTurn {
            public int Turn { get; set; } = -1;
        }

        static readonly ConditionalWeakTable<IceCream, LastCountedTurn> CountedTurns = new();

        static void Postfix(IceCream __instance, Player player, bool __result) {
            try {
                if (__instance == null || player == null || player != __instance.Owner || __result) return;
                var combatState = player.PlayerCombatState;
                if (combatState == null || combatState.TurnNumber <= 1) return;
                var energy = Math.Max(0, combatState.Energy);
                if (energy <= 0) return;

                var counted = CountedTurns.GetOrCreateValue(__instance);
                if (counted.Turn == combatState.TurnNumber) return;
                counted.Turn = combatState.TurnNumber;

                RelicTracker.AddAmount(__instance, "Energy Retained", energy);
            } catch { }
        }
    }
}
