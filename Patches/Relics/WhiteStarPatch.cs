using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(WhiteStar), nameof(WhiteStar.TryModifyRewards))]
    public static class WhiteStarPatch {
        static void Postfix(WhiteStar __instance, Player player, List<Reward> rewards, AbstractRoom room, bool __result) {
            try {
                _ = rewards;
                _ = room;
                if (!__result || __instance == null || player == null || __instance.Owner != player) return;
                RelicTracker.AddAmount(__instance, "Card Rewards Added", 1);
            } catch { }
        }
    }
}
