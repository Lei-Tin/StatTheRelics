using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(MummifiedHand), nameof(MummifiedHand.AfterCardPlayed))]
    public static class MummifiedHandPatch {
        [ThreadStatic] internal static MummifiedHand? Current;

        static void Prefix(MummifiedHand __instance, CardPlay cardPlay) {
            try {
                if (__instance == null || cardPlay?.Card == null || cardPlay.Card.Owner != __instance.Owner || cardPlay.Card.Type != CardType.Power) return;
                Current = __instance;
            } catch { }
        }

        static void Postfix() {
            Current = null;
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.SetToFreeThisTurn))]
    public static class MummifiedHandSetFreePatch {
        static void Postfix(CardModel __instance) {
            try {
                var relic = MummifiedHandPatch.Current;
                if (relic == null || __instance == null || __instance.Owner != relic.Owner) return;
                RelicTracker.AddAmount(relic, "Free Cards Given", 1);
            } catch { }
        }
    }
}
