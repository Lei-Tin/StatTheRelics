using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ThrowingAxe), nameof(ThrowingAxe.AfterModifyingCardPlayCount))]
    public static class ThrowingAxePatch {
        static void Postfix(ThrowingAxe __instance, CardModel card) {
            try {
                if (__instance == null || card == null || card.Owner != __instance.Owner) return;
                RelicTracker.AddAmount(__instance, "Extra Plays", 1);
            } catch { }
        }
    }
}
