using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(FencingManual), nameof(FencingManual.AfterSideTurnStart))]
    public static class FencingManualPatch {
        class ForgeState {
            public int Forge { get; set; }
        }

        static void Prefix(FencingManual __instance, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ref object __state) {
            try {
                var ownerCreature = __instance?.Owner?.Creature;
                if (__instance == null || ownerCreature == null || participants == null) return;
                if (!participants.Contains(ownerCreature)) return;
                if (__instance.Owner.PlayerCombatState?.TurnNumber > 1) return;

                __state = new ForgeState {
                    Forge = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Forge", 10))
                };
            } catch { }
        }

        static void Postfix(FencingManual __instance, Task __result, object __state) {
            try {
                var state = __state as ForgeState;
                if (state == null || state.Forge <= 0) return;

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

        static void Count(FencingManual relic, ForgeState state) {
            RelicTracker.AddAmount(relic, "Forge Gained", state.Forge);
        }
    }
}
