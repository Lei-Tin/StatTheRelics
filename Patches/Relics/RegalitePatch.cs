using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Regalite), nameof(Regalite.AfterCardGeneratedForCombat))]
    public static class RegalitePatch {
        class State {
            public int Block { get; set; }
        }

        static void Prefix(Regalite __instance, CardModel card, Player creator, ref object __state) {
            try {
                if (__instance == null || creator == null || __instance.Owner != creator) return;
                __state = new State {
                    Block = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Block", 2))
                };
            } catch { }
        }

        static void Postfix(Regalite __instance, Task __result, object __state) {
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
