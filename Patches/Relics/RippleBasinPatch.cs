using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(RippleBasin), nameof(RippleBasin.BeforeSideTurnEnd))]
    public static class RippleBasinPatch {
        class State {
            public int Block { get; set; }
        }

        static void Prefix(RippleBasin __instance, PlayerChoiceContext choiceContext, IEnumerable<Creature> participants, ref object __state) {
            try {
                if (__instance?.Owner?.Creature == null || participants == null) return;
                if (!participants.Contains(__instance.Owner.Creature)) return;
                if (!IsActive(__instance)) return;

                __state = new State {
                    Block = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Block", 4))
                };
            } catch { }
        }

        static void Postfix(RippleBasin __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.Block <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Block Gained", state.Block);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Block Gained", state.Block);
                    } catch { }
                });
            } catch { }
        }

        static bool IsActive(RippleBasin relic) {
            try {
                var status = ReflectionUtil.GetMemberValue(relic, "Status");
                if (status == null) return false;
                return Convert.ToInt32(status) == 1;
            } catch {
                return false;
            }
        }
    }
}
