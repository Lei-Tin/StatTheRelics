using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ConfusedPower), nameof(ConfusedPower.AfterCardDrawn))]
    public static class FakeSneckoEyePatch {
        static void Postfix(ConfusedPower __instance, CardModel card) {
            try {
                var relic = ReflectionUtil.FindRelic<FakeSneckoEye>(__instance?.Owner);
                if (relic == null) return;

                var cost = card?.EnergyCost?.GetWithModifiers(CostModifiers.Local) ?? -1;
                if (cost < 0 || cost > 3) return;

                RelicTracker.AddAmount(relic, $"{cost} Cost", 1);
            } catch { }
        }
    }
}
