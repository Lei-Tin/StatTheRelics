using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Capture opening elite hand draw bonus from Booming Conch.
    [HarmonyPatch(typeof(BoomingConch), nameof(BoomingConch.ModifyHandDraw))]
    public static class BoomingConchPatch {
        static void Postfix(BoomingConch __instance, Player player, decimal count, ref decimal __result) {
            try {
                var extraDraw = __result - count; // net cards added by the relic
                if (extraDraw > 0) {
                    RelicTracker.AddAmount(__instance, "Cards Drawn", Convert.ToInt32(extraDraw));
                }

                ModLog.Info($"BoomingConchPatch: player={player?.GetType().FullName ?? "null"}, baseCount={count}, result={__result}, extra={extraDraw}");
            } catch { }
        }
    }

    // Capture the first-turn elite energy bonus from Booming Conch.
    [HarmonyPatch(typeof(BoomingConch), nameof(BoomingConch.AfterSideTurnStart))]
    public static class BoomingConchEnergyPatch {
        class BoomingConchEnergyState {
            public int Energy { get; set; }
        }

        static void Prefix(BoomingConch __instance, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ref object __state) {
            try {
                var owner = __instance?.Owner;
                var ownerCreature = owner?.Creature;
                if (__instance == null || owner == null || ownerCreature == null || participants == null || combatState == null) return;
                if (!participants.Contains(ownerCreature)) return;
                if (owner.PlayerCombatState == null || owner.PlayerCombatState.TurnNumber > 1) return;
                if (combatState.RunState?.CurrentRoom?.RoomType != RoomType.Elite) return;

                __state = new BoomingConchEnergyState {
                    Energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Energy"))
                };
            } catch { }
        }

        static void Postfix(BoomingConch __instance, Task __result, object __state) {
            try {
                var state = __state as BoomingConchEnergyState;
                if (state == null || state.Energy <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Energy Gained", state.Energy);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) {
                        RelicTracker.AddAmount(__instance, "Energy Gained", state.Energy);
                    }
                });
            } catch { }
        }
    }
}
