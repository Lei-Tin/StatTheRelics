using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ConfusedPower), "NextEnergyCost")]
    public static class FakeSneckoEyePatch {
        static void Postfix(ConfusedPower __instance, int __result) {
            try {
                var relic = ReflectionUtil.FindRelic<FakeSneckoEye>(__instance?.Owner);
                if (relic == null) return;
                if (__result < 0 || __result > 3) return;

                RelicTracker.AddAmount(relic, $"{__result} Cost", 1);
            } catch { }
        }
    }
}
