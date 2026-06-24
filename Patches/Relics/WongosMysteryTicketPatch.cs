using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    public static class WongosMysteryTicketPatch {
        sealed class Mark {
            public WongosMysteryTicket Relic { get; }

            public Mark(WongosMysteryTicket relic) {
                Relic = relic;
            }
        }

        static readonly object MarkLock = new();
        static readonly ConditionalWeakTable<RelicReward, Mark> MarkedRewards = new();

        internal static void MarkRewards(WongosMysteryTicket relic, IEnumerable<Reward> rewards) {
            try {
                if (relic == null || rewards == null) return;
                lock (MarkLock) {
                    foreach (var reward in rewards.OfType<RelicReward>()) {
                        if (!MarkedRewards.TryGetValue(reward, out _)) MarkedRewards.Add(reward, new Mark(relic));
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
                RelicTracker.SetText(mark.Relic, "Relics Added", string.IsNullOrWhiteSpace(current) ? name : current + "\n" + name);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(WongosMysteryTicket), nameof(WongosMysteryTicket.AfterCombatEnd))]
    public static class WongosMysteryTicketCombatPatch {
        sealed class State {
            public int CombatsFinished { get; set; }
        }

        static void Prefix(WongosMysteryTicket __instance, ref object __state) {
            try {
                if (__instance == null) return;
                __state = new State { CombatsFinished = __instance.CombatsFinished };
            } catch { }
        }

        static void Postfix(WongosMysteryTicket __instance, object __state) {
            try {
                if (__state is not State state || __instance == null) return;
                var finished = __instance.CombatsFinished - state.CombatsFinished;
                if (finished > 0) RelicTracker.AddAmount(__instance, "Combats Finished", finished);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(WongosMysteryTicket), nameof(WongosMysteryTicket.TryModifyRewards))]
    public static class WongosMysteryTicketRewardPatch {
        sealed class State {
            public int RelicRewards { get; set; }
            public int RewardCount { get; set; }
        }

        static void Prefix(WongosMysteryTicket __instance, Player player, List<Reward> rewards, AbstractRoom room, ref object __state) {
            try {
                _ = player;
                _ = room;
                if (__instance == null || rewards == null) return;
                __state = new State {
                    RelicRewards = rewards.OfType<RelicReward>().Count(),
                    RewardCount = rewards.Count
                };
            } catch { }
        }

        static void Postfix(WongosMysteryTicket __instance, List<Reward> rewards, bool __result, object __state) {
            try {
                if (!__result || __state is not State state || __instance == null || rewards == null) return;
                var added = rewards.OfType<RelicReward>().Count() - state.RelicRewards;
                if (added > 0) RelicTracker.AddAmount(__instance, "Relic Rewards Added", added);

                if (state.RewardCount < rewards.Count) {
                    WongosMysteryTicketPatch.MarkRewards(__instance, rewards.Skip(state.RewardCount));
                }
            } catch { }
        }
    }

    [HarmonyPatch(typeof(RelicReward), nameof(RelicReward.Populate))]
    public static class WongosMysteryTicketRelicRewardPopulatePatch {
        static void Postfix(RelicReward __instance) {
            WongosMysteryTicketPatch.RecordIfMarked(__instance);
        }
    }
}
