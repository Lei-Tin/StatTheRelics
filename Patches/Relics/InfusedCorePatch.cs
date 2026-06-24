using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(InfusedCore), nameof(InfusedCore.AfterSideTurnStart))]
    public static class InfusedCorePatch {
        class ChannelState {
            public int Lightning { get; set; }
        }

        static void Prefix(InfusedCore __instance, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ref object __state) {
            try {
                var ownerCreature = __instance?.Owner?.Creature;
                if (__instance == null || ownerCreature == null || participants == null || !participants.Contains(ownerCreature)) return;
                if (__instance.Owner.PlayerCombatState?.TurnNumber > 1) return;
                __state = new ChannelState {
                    Lightning = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Lightning", 3))
                };
            } catch { }
        }

        static void Postfix(InfusedCore __instance, Task __result, object __state) {
            try {
                var state = __state as ChannelState;
                if (state == null || state.Lightning <= 0) return;

                if (__result == null) {
                    Count(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, state);
                    } catch { }
                });
            } catch { }
        }

        static void Count(InfusedCore relic, ChannelState state) {
            RelicTracker.AddAmount(relic, "Lightning Orbs Channeled", state.Lightning);
        }
    }
}
