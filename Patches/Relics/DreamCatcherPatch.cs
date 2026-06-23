using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(DreamCatcher), nameof(DreamCatcher.TryModifyRestSiteHealRewards))]
    public static class DreamCatcherPatch {
        class DreamCatcherState {
            public int RewardCount { get; set; }
        }

        static void Prefix(DreamCatcher __instance, Player player, List<Reward> rewards, bool isMimicked, ref object __state) {
            try {
                if (__instance == null || player == null || rewards == null || __instance.Owner != player) return;
                __state = new DreamCatcherState {
                    RewardCount = rewards.OfType<CardReward>().Count()
                };
            } catch { }
        }

        static void Postfix(DreamCatcher __instance, List<Reward> rewards, bool __result, object __state) {
            try {
                var state = __state as DreamCatcherState;
                if (state == null || rewards == null || !__result) return;
                var added = Math.Max(0, rewards.OfType<CardReward>().Count() - state.RewardCount);
                if (added > 0) RelicTracker.AddAmount(__instance, "Card Rewards Added", added);
            } catch { }
        }
    }
}
