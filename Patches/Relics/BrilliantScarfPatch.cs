using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    // Count the extra card only when it is actually played while the scarf is active.
    [HarmonyPatch(typeof(BrilliantScarf), nameof(BrilliantScarf.AfterCardPlayed))]
    public static class BrilliantScarfPatch {
        static void Prefix(BrilliantScarf __instance, CardPlay cardPlay, ref bool __state) {
            try {
                __state = false;
                if (__instance == null || cardPlay == null) return;
                if (cardPlay.IsAutoPlay) return;
                if (cardPlay.Card == null || cardPlay.Card.Owner != __instance.Owner) return;

                var cardsPlayedThisTurn = ReflectionUtil.GetIntMemberValue(__instance, "CardsPlayedThisTurn", -1);
                var dynamicVars = ReflectionUtil.GetMemberValue(__instance, "DynamicVars");
                var cardsVar = ReflectionUtil.GetMemberValue(dynamicVars, "Cards");
                var intValueRaw = ReflectionUtil.GetMemberValue(cardsVar, "IntValue");
                var threshold = intValueRaw == null ? 0 : System.Math.Max(0, System.Convert.ToInt32(intValueRaw));

                if (threshold <= 0) return;
                if (cardsPlayedThisTurn != threshold - 1) return;

                __state = true;
            } catch { }
        }

        static void Postfix(BrilliantScarf __instance, bool __state) {
            try {
                if (__instance == null || !__state) return;
                RelicTracker.AddAmount(__instance, "Freed Cards", 1);
                ModLog.Info("BrilliantScarfPatch: counted actual free card play");
            } catch { }
        }
    }
}
