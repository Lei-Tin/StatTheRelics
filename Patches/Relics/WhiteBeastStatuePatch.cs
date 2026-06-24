using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(WhiteBeastStatue), nameof(WhiteBeastStatue.ShouldForcePotionReward))]
    public static class WhiteBeastStatuePatch {
        static void Postfix(WhiteBeastStatue __instance, Player player, RoomType roomType, bool __result) {
            try {
                _ = roomType;
                if (!__result || __instance == null || player == null || __instance.Owner != player) return;
                RelicTracker.AddAmount(__instance, "Potion Rewards Added", 1);
            } catch { }
        }
    }
}
