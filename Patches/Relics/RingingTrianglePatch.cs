using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Models.Powers;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(RetainHandPower), nameof(RetainHandPower.ShouldFlush))]
    public static class RingingTrianglePatch {
        static readonly object Sync = new();
        static readonly Dictionary<int, CountKey> LastCounted = new();

        struct CountKey {
            public int CombatHash { get; set; }
            public int TurnNumber { get; set; }
        }

        static void Postfix(RetainHandPower __instance, Player player, bool __result) {
            try {
                if (__instance == null || player == null || __instance.Owner != player.Creature) return;
                var relic = ReflectionUtil.FindRelic<RingingTriangle>(player);
                if (relic == null || player.PlayerCombatState?.TurnNumber != 1) return;
                if (__result) return;

                var turnNumber = ReflectionUtil.GetIntMemberValue(player.PlayerCombatState, "TurnNumber", int.MaxValue);
                var combat = player.Creature?.CombatState;
                if (turnNumber == int.MaxValue || combat == null) return;

                var key = new CountKey {
                    CombatHash = RuntimeHelpers.GetHashCode(combat),
                    TurnNumber = turnNumber
                };
                var relicHash = RuntimeHelpers.GetHashCode(relic);
                lock (Sync) {
                    if (LastCounted.TryGetValue(relicHash, out var last)
                        && last.CombatHash == key.CombatHash
                        && last.TurnNumber == key.TurnNumber) {
                        return;
                    }

                    LastCounted[relicHash] = key;
                }

                var count = PileType.Hand.GetPile(player)?.Cards?.Count ?? 0;
                if (count > 0) RelicTracker.AddAmount(relic, "Cards Retained", count);
            } catch { }
        }
    }
}
