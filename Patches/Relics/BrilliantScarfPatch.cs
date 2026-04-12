using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    // Count one free-card opportunity when the per-turn play counter reaches threshold - 1.
    [HarmonyPatch(typeof(BrilliantScarf), nameof(BrilliantScarf.AfterCardPlayed))]
    public static class BrilliantScarfPatch {
        static void Postfix(BrilliantScarf __instance) {
            try {
                if (__instance == null) return;

                var cardsPlayedThisTurn = ReflectionUtil.GetIntMemberValue(__instance, "CardsPlayedThisTurn", -1);
                var dynamicVars = ReflectionUtil.GetMemberValue(__instance, "DynamicVars");
                var cardsVar = ReflectionUtil.GetMemberValue(dynamicVars, "Cards");
                var intValueRaw = ReflectionUtil.GetMemberValue(cardsVar, "IntValue");
                var threshold = intValueRaw == null ? 0 : System.Math.Max(0, System.Convert.ToInt32(intValueRaw));

                if (threshold <= 0) return;
                if (cardsPlayedThisTurn != threshold - 1) return;

                RelicTracker.AddAmount(__instance, "Freed Cards", 1);
                ModLog.Info($"BrilliantScarfPatch: incremented Freed Cards at CardsPlayedThisTurn={cardsPlayedThisTurn}, threshold={threshold}");
            } catch { }
        }
    }
}
