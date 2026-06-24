using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(TinyMailbox), nameof(TinyMailbox.TryModifyRestSiteHealRewards))]
    public static class TinyMailboxPatch {
        static void Postfix(TinyMailbox __instance, Player player, bool __result) {
            try {
                if (!__result || __instance == null || player == null || __instance.Owner != player) return;
                RelicTracker.AddAmount(__instance, "Potion Rewards Added", 2);
            } catch { }
        }
    }
}
