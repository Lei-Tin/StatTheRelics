using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(TheAbacus), nameof(TheAbacus.AfterShuffle))]
    public static class TheAbacusPatch {
        static void Prefix(TheAbacus __instance, PlayerChoiceContext choiceContext, Player shuffler, ref object __state) {
            try {
                _ = choiceContext;
                if (__instance == null || shuffler == null || __instance.Owner != shuffler) return;
                var block = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Block", 6));
                if (block > 0) __state = block;
            } catch { }
        }

        static void Postfix(TheAbacus __instance, Task __result, object __state) {
            try {
                if (__state is not int block || block <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Block Gained", block);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Block Gained", block);
                    } catch { }
                });
            } catch { }
        }
    }
}
