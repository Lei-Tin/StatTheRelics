using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class RadiantPearlPatch {
        static void Postfix(CardModel __instance) {
            try {
                if (__instance == null) return;
                if (__instance is not Luminesce) return;

                var relic = ReflectionUtil.FindRelic<RadiantPearl>(__instance.Owner);
                if (relic == null) return;

                RelicTracker.AddAmount(relic, "Luminesce Played", 1);
            } catch { }
        }
    }
}
