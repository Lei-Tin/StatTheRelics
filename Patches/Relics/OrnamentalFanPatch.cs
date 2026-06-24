using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(OrnamentalFan), nameof(OrnamentalFan.AfterCardPlayed))]
    public static class OrnamentalFanPatch {
        class FanState {
            public bool WillTrigger { get; set; }
            public int Block { get; set; }
        }

        static void Prefix(OrnamentalFan __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref object __state) {
            try {
                _ = choiceContext;
                if (__instance == null || cardPlay?.Card == null || cardPlay.Card.Owner != __instance.Owner) return;
                if (Convert.ToInt32(cardPlay.Card.Type) != 1) return;

                var threshold = Math.Max(1, ReflectionUtil.GetDynamicVarIntValue(__instance, "Cards", 3));
                var attacksPlayed = ReflectionUtil.GetIntMemberValue(__instance, "AttacksPlayedThisTurn", 0);
                __state = new FanState {
                    WillTrigger = (attacksPlayed + 1) % threshold == 0,
                    Block = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Block", 4))
                };
            } catch { }
        }

        static void Postfix(OrnamentalFan __instance, Task __result, object __state) {
            try {
                if (__state is not FanState state || !state.WillTrigger || state.Block <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Block Gained", state.Block);
                    } catch { }
                });
            } catch { }
        }
    }
}
