using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(GremlinHorn), nameof(GremlinHorn.AfterDeath))]
    public static class GremlinHornPatch {
        class TriggerState {
            public int Energy { get; set; }
            public int Cards { get; set; }
        }

        static void Prefix(GremlinHorn __instance, PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength, ref object __state) {
            try {
                var ownerCreature = __instance?.Owner?.Creature;
                if (__instance == null || ownerCreature == null || target == null) return;
                if (target.Side == ownerCreature.Side) return;

                __state = new TriggerState {
                    Energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Energy", 1)),
                    Cards = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Cards", 1))
                };
            } catch { }
        }

        static void Postfix(GremlinHorn __instance, Task __result, object __state) {
            try {
                var state = __state as TriggerState;
                if (state == null) return;

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

        static void Count(GremlinHorn relic, TriggerState state) {
            if (state.Energy > 0) RelicTracker.AddAmount(relic, "Energy Gained", state.Energy);
            if (state.Cards > 0) RelicTracker.AddAmount(relic, "Cards Drawn", state.Cards);
        }
    }
}
