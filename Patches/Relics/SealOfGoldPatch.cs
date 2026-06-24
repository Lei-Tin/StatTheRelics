using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(SealOfGold), nameof(SealOfGold.AfterSideTurnStart))]
    public static class SealOfGoldPatch {
        class State {
            public int Energy { get; set; }
            public int Gold { get; set; }
        }

        static void Prefix(SealOfGold __instance, IReadOnlyList<Creature> participants, ref object __state) {
            try {
                var owner = __instance?.Owner;
                if (__instance == null || owner == null) return;
                if (participants == null || !Contains(participants, owner.Creature)) return;

                var gold = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Gold", 5));
                if (owner.Gold < gold) return;

                __state = new State {
                    Energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Energy", 1)),
                    Gold = gold
                };
            } catch { }
        }

        static bool Contains(IReadOnlyList<Creature> participants, Creature creature) {
            if (creature == null) return false;
            foreach (var participant in participants) {
                if (participant == creature) return true;
            }

            return false;
        }

        static void Postfix(SealOfGold __instance, Task __result, object __state) {
            try {
                if (__state is not State state) return;
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

        static void Count(SealOfGold relic, State state) {
            if (state.Energy > 0) RelicTracker.AddAmount(relic, "Energy Gained", state.Energy);
            if (state.Gold > 0) RelicTracker.AddAmount(relic, "Gold Spent", state.Gold);
        }
    }
}
