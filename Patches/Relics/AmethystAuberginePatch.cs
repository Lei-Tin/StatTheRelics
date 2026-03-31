using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Track gold added by Amethyst Aubergine when it injects a reward.
    [HarmonyPatch(typeof(AmethystAubergine), nameof(AmethystAubergine.TryModifyRewards))]
    public static class AmethystAuberginePatch {
        static void Postfix(AmethystAubergine __instance, bool __result) {
            try {
                if (!__result) return;

                var relic = __instance;
                if (relic == null) return;

                var gold = relic.DynamicVars?.Gold?.IntValue ?? 0;
                if (gold <= 0) return;

                RelicTracker.AddAmount(relic, "Gold Generated", gold);
                ModLog.Info($"AmethystAuberginePatch: added {gold} gold for {relic.GetType().FullName}");
            } catch { }
        }
    }
}
