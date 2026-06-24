using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(LavaRock), nameof(LavaRock.TryModifyRewards))]
    public static class LavaRockPatch {
        sealed class Mark {
            public LavaRock Relic { get; }

            public Mark(LavaRock relic) {
                Relic = relic;
            }
        }

        static readonly object MarkLock = new();
        static readonly ConditionalWeakTable<RelicReward, Mark> MarkedRewards = new();

        static void Prefix(List<Reward> rewards, ref object __state) {
            try {
                __state = rewards?.Count ?? 0;
            } catch { }
        }

        static void Postfix(LavaRock __instance, List<Reward> rewards, bool __result, object __state) {
            try {
                if (!__result) return;
                var start = __state is int count ? count : 0;
                if (rewards == null || start >= rewards.Count) return;

                lock (MarkLock) {
                    for (var i = start; i < rewards.Count; i++) {
                        if (rewards[i] is RelicReward reward && !MarkedRewards.TryGetValue(reward, out _)) {
                            MarkedRewards.Add(reward, new Mark(__instance));
                        }
                    }
                }
            } catch { }
        }

        internal static void RecordIfMarked(RelicReward reward) {
            try {
                if (reward == null) return;
                Mark? mark;
                lock (MarkLock) {
                    if (!MarkedRewards.TryGetValue(reward, out mark)) return;
                }

                var relic = reward.Relic ?? ReflectionUtil.GetMemberValue(reward, "_relic");
                var name = ReflectionUtil.GetModelTitle(relic) ?? relic?.GetType().Name;
                if (string.IsNullOrWhiteSpace(name)) return;

                var current = RelicTracker.GetText(mark.Relic, "Relics Added");
                var value = string.IsNullOrWhiteSpace(current) ? name : current + "\n" + name;
                RelicTracker.SetText(mark.Relic, "Relics Added", value);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(RelicReward), nameof(RelicReward.Populate))]
    public static class LavaRockRelicRewardPopulatePatch {
        static void Postfix(RelicReward __instance) {
            LavaRockPatch.RecordIfMarked(__instance);
        }
    }
}
