using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(LunarPastry), nameof(LunarPastry.AfterSideTurnEnd))]
    public static class LunarPastryPatch {
        class TriggerState {
            public int Stars { get; set; }
        }

        static void Prefix(LunarPastry __instance, PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ref object __state) {
            try {
                _ = choiceContext;
                _ = side;
                if (__instance?.Owner?.Creature == null || participants == null) return;
                if (!participants.Contains(__instance.Owner.Creature)) return;

                __state = new TriggerState {
                    Stars = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Stars", 1))
                };
            } catch { }
        }

        static void Postfix(LunarPastry __instance, Task __result, object __state) {
            try {
                var state = __state as TriggerState;
                if (state == null || state.Stars <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Stars Gained", state.Stars);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            RelicTracker.AddAmount(__instance, "Stars Gained", state.Stars);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
