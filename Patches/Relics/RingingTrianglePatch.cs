using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(RingingTriangle), nameof(RingingTriangle.ShouldFlush))]
    public static class RingingTrianglePatch {
        static readonly object Sync = new();
        static readonly Dictionary<int, CountKey> LastCounted = new();

        struct CountKey {
            public int CombatHash { get; set; }
            public int TurnNumber { get; set; }
        }

        static void Postfix(RingingTriangle __instance, Player player, bool __result) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                if (__result) return;

                var turnNumber = ReflectionUtil.GetIntMemberValue(player.PlayerCombatState, "TurnNumber", int.MaxValue);
                var combat = player.Creature?.CombatState;
                if (turnNumber == int.MaxValue || combat == null) return;

                var key = new CountKey {
                    CombatHash = RuntimeHelpers.GetHashCode(combat),
                    TurnNumber = turnNumber
                };
                var relicHash = RuntimeHelpers.GetHashCode(__instance);
                lock (Sync) {
                    if (LastCounted.TryGetValue(relicHash, out var last)
                        && last.CombatHash == key.CombatHash
                        && last.TurnNumber == key.TurnNumber) {
                        return;
                    }

                    LastCounted[relicHash] = key;
                }

                var count = PileType.Hand.GetPile(player)?.Cards?.Count ?? 0;
                if (count > 0) RelicTracker.AddAmount(__instance, "Cards Retained", count);
            } catch { }
        }
    }
}
