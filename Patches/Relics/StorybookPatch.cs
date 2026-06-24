using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class StorybookPatch {
        static void Postfix(CardModel __instance) {
            try {
                if (__instance == null) return;
                if (!string.Equals(DeckUtil.GetCardMatchName(__instance), "Brightest Flame", System.StringComparison.OrdinalIgnoreCase)) return;

                var relic = ReflectionUtil.FindRelic<Storybook>(__instance.Owner);
                if (relic == null) return;

                RelicTracker.AddAmount(relic, "Brightest Flame Played", 1);
            } catch { }
        }
    }
}
