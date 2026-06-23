using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(DaughterOfTheWind), nameof(DaughterOfTheWind.AfterCardPlayed))]
    public static class DaughterOfTheWindPatch {
        static void Prefix(DaughterOfTheWind __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref int __state) {
            try {
                if (__instance == null || cardPlay?.Card == null) return;
                if (cardPlay.Card.Owner != __instance.Owner) return;
                if (Convert.ToInt32(cardPlay.Card.Type) != 1) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Block", 1));
            } catch { }
        }

        static void Postfix(DaughterOfTheWind __instance, Task __result, int __state) {
            try {
                if (__state <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Block Gained", __state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            RelicTracker.AddAmount(__instance, "Block Gained", __state);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
