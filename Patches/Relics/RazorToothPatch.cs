using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(RazorTooth), nameof(RazorTooth.AfterCardPlayed))]
    public static class RazorToothPatch {
        class State {
            public bool ShouldCount { get; set; }
        }

        static void Prefix(RazorTooth __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref object __state) {
            try {
                var card = cardPlay?.Card;
                if (__instance == null || card == null || card.Owner != __instance.Owner) return;

                var type = Convert.ToInt32(card.Type);
                if (type != 1 && type != 2) return;
                if (!card.IsUpgradable) return;

                __state = new State { ShouldCount = true };
            } catch { }
        }

        static void Postfix(RazorTooth __instance, Task __result, object __state) {
            try {
                if (__state is not State state || !state.ShouldCount) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Cards Upgraded", 1);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Cards Upgraded", 1);
                    } catch { }
                });
            } catch { }
        }
    }
}
