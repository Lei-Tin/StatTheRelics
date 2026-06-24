using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(VexingPuzzlebox), nameof(VexingPuzzlebox.AfterPlayerTurnStart))]
    public static class VexingPuzzleboxPatch {
        sealed class State {
            public int CardsAdded { get; set; }
        }

        static void Prefix(VexingPuzzlebox __instance, Player player, ref object __state) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                if (__instance.Owner?.PlayerCombatState?.TurnNumber > 1) return;

                __state = new State { CardsAdded = 1 };
            } catch { }
        }

        static void Postfix(VexingPuzzlebox __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.CardsAdded <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Cards Added", state.CardsAdded);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Cards Added", state.CardsAdded);
                    } catch { }
                });
            } catch { }
        }
    }
}
