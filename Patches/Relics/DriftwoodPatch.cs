using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Driftwood), nameof(Driftwood.TryModifyRewardsLate))]
    public static class DriftwoodPatch {
        static void Postfix(Driftwood __instance, Player player, List<Reward> rewards, AbstractRoom room, bool __result) {
            try {
                if (__instance == null || player == null || rewards == null || __instance.Owner != player) return;
                if (!__result) return;
                foreach (var reward in rewards.OfType<CardReward>()) {
                    DriftwoodRerollPatch.MarkReward(reward);
                }
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardReward), nameof(CardReward.Reroll))]
    public static class DriftwoodRerollPatch {
        static readonly HashSet<int> DriftwoodRewards = new();

        internal static void MarkReward(CardReward reward) {
            try {
                if (reward == null) return;
                DriftwoodRewards.Add(reward.GetHashCode());
            } catch { }
        }

        static void Postfix(CardReward __instance) {
            try {
                if (__instance == null) return;
                if (!DriftwoodRewards.Contains(__instance.GetHashCode())) return;

                var relic = FindRelic(__instance);
                if (relic == null) return;
                RelicTracker.AddAmount(relic, "Rerolls", 1);
            } catch { }
        }

        static Driftwood? FindRelic(CardReward reward) {
            try {
                var player = ReflectionUtil.GetMemberValue(reward, "Player");
                return ReflectionUtil.FindRelic<Driftwood>(player);
            } catch {
                return null;
            }
        }
    }
}
