using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using System.Collections.Generic;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PrayerWheel), nameof(PrayerWheel.TryModifyRewards))]
    public static class PrayerWheelPatch {
        static void Postfix(PrayerWheel __instance, Player player, List<Reward> rewards, AbstractRoom room, bool __result) {
            try {
                if (!__result) return;
                if (__instance?.Owner != player) return;
                RelicTracker.AddAmount(__instance, "Card Rewards Added", 1);
            } catch { }
        }
    }
}
