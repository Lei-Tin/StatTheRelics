using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ToughBandages), nameof(ToughBandages.AfterCardDiscarded))]
    public static class ToughBandagesPatch {
        class State {
            public int Block { get; set; }
        }

        static void Prefix(ToughBandages __instance, CardModel card, ref object __state) {
            try {
                if (__instance == null || card == null || card.Owner != __instance.Owner) return;
                var ownerCreature = __instance.Owner?.Creature;
                if (ownerCreature == null || ownerCreature.Side != ownerCreature.CombatState?.CurrentSide) return;

                __state = new State {
                    Block = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Block", 3))
                };
            } catch { }
        }

        static void Postfix(ToughBandages __instance, Task __result, object __state) {
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
    }
}
