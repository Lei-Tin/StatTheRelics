using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class TanxsWhistlePatch {
        static void Postfix(CardModel __instance) {
            try {
                if (__instance == null) return;
                if (__instance is not Whistle) return;

                var relic = ReflectionUtil.FindRelic<TanxsWhistle>(__instance.Owner);
                if (relic == null) return;

                RelicTracker.AddAmount(relic, "Whistle Played", 1);
            } catch { }
        }
    }
}
