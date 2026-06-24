using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(MusicBox), nameof(MusicBox.AfterCardPlayed))]
    public static class MusicBoxPatch {
        static void Prefix(MusicBox __instance, CardPlay cardPlay, ref object __state) {
            try {
                var cardBeingPlayed = ReflectionUtil.GetMemberValue(__instance, "CardBeingPlayed");
                if (__instance == null || cardPlay?.Card == null || !ReferenceEquals(cardPlay.Card, cardBeingPlayed)) return;
                __state = true;
            } catch { }
        }

        static void Postfix(MusicBox __instance, Task __result, object __state) {
            try {
                if (__state is not true || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        RelicTracker.AddAmount(__instance, "Cards Copied", 1);
                    } catch { }
                });
            } catch { }
        }
    }
}
