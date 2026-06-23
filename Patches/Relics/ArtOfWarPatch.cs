using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ArtOfWar), nameof(ArtOfWar.AfterEnergyReset))]
    public static class ArtOfWarPatch {
        class ArtOfWarState {
            public int Energy { get; set; }
        }

        static void Prefix(ArtOfWar __instance, Player player, ref object __state) {
            try {
                if (__instance == null || player == null) return;
                if (__instance.Owner != player) return;
                var combatState = __instance.Owner?.Creature?.CombatState;
                if (combatState == null || combatState.RoundNumber <= 1) return;
                var anyAttacksLastTurn = ReflectionUtil.GetMemberValue(__instance, "AnyAttacksPlayedLastTurn");
                if (anyAttacksLastTurn is bool played && played) return;

                __state = new ArtOfWarState {
                    Energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Energy"))
                };
            } catch { }
        }

        static void Postfix(ArtOfWar __instance, Task __result, object __state) {
            try {
                var state = __state as ArtOfWarState;
                if (state == null || state.Energy <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Energy Given", state.Energy);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) {
                        RelicTracker.AddAmount(__instance, "Energy Given", state.Energy);
                    }
                });
            } catch { }
        }
    }
}
