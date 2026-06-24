using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(TuningFork), nameof(TuningFork.AfterCardPlayed))]
    public static class TuningForkPatch {
        class State {
            public int Block { get; set; }
        }

        static void Prefix(TuningFork __instance, CardPlay cardPlay, ref object __state) {
            try {
                var card = cardPlay?.Card;
                if (__instance == null || card == null || card.Owner != __instance.Owner) return;
                if (card.Type != CardType.Skill) return;
                var threshold = Math.Max(1, ReflectionUtil.GetDynamicVarIntValue(__instance, "Cards", 10));
                if (__instance.SkillsPlayed + 1 < threshold) return;

                __state = new State {
                    Block = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Block", 7))
                };
            } catch { }
        }

        static void Postfix(TuningFork __instance, Task __result, object __state) {
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
