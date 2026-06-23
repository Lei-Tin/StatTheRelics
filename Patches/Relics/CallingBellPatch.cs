using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace StatTheRelics.Patches.Relics {
    // Record the exact curse and relic rewards produced by Calling Bell.
    [HarmonyPatch(typeof(CallingBell), nameof(CallingBell.AfterObtained))]
    public static class CallingBellPatch {
        class CallingBellState {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(CallingBell __instance, ref object __state) {
            try {
                __state = new CallingBellState {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(CallingBell __instance, Task __result, object __state) {
            try {
                var state = __state as CallingBellState;
                if (state == null) return;

                if (__result == null) {
                    CaptureCurse(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) CaptureCurse(__instance, state);
                });
            } catch { }
        }

        static void CaptureCurse(CallingBell relic, CallingBellState state) {
            try {
                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var added = DeckUtil.FindAddedCards(state.BeforeDeck, after);
                var curse = added.Count > 0 ? string.Join("\n", added) : "Unknown";
                RelicTracker.SetText(relic, "Curse", curse);
                ModLog.Info($"CallingBellPatch: curse={curse}");
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CallingBell), "GenerateRewards")]
    public static class CallingBellGenerateRewardsPatch {
        static void Postfix(CallingBell __instance, List<Reward> __result) {
            try {
                if (__instance == null || __result == null) return;
                CallingBellRewardTracker.TrackRewards(__result, __instance);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(RewardsCmd), nameof(RewardsCmd.OfferCustom), new Type[] {
        typeof(Player),
        typeof(List<Reward>)
    })]
    public static class CallingBellOfferCustomPatch {
        class OfferState {
            public CallingBell? Relic { get; set; }
            public List<Reward>? Rewards { get; set; }
        }

        static void Prefix(List<Reward> rewards, ref object __state) {
            try {
                var relic = CallingBellRewardTracker.FindTrackedRelic(rewards);
                if (relic == null) return;

                __state = new OfferState {
                    Relic = relic,
                    Rewards = rewards
                };
            } catch { }
        }

        static void Postfix(Task __result, object __state) {
            try {
                var state = __state as OfferState;
                if (state?.Relic == null || state.Rewards == null) return;

                if (__result == null) {
                    CallingBellRewardTracker.CaptureRelics(state.Relic, state.Rewards);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) {
                        CallingBellRewardTracker.CaptureRelics(state.Relic, state.Rewards);
                    }
                });
            } catch { }
        }
    }

    static class CallingBellRewardTracker {
        static readonly ConcurrentDictionary<int, CallingBell> rewardsToRelic = new();

        public static void TrackRewards(List<Reward> rewards, CallingBell relic) {
            try {
                rewardsToRelic[RuntimeHelpers.GetHashCode(rewards)] = relic;
            } catch { }
        }

        public static CallingBell? FindTrackedRelic(List<Reward>? rewards) {
            try {
                if (rewards == null) return null;
                return rewardsToRelic.TryGetValue(RuntimeHelpers.GetHashCode(rewards), out var relic) ? relic : null;
            } catch {
                return null;
            }
        }

        public static void CaptureRelics(CallingBell relic, List<Reward> rewards) {
            try {
                if (relic == null || rewards == null) return;
                rewardsToRelic.TryRemove(RuntimeHelpers.GetHashCode(rewards), out _);

                var names = rewards
                    .Select(GetRelicNameFromReward)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList();

                var text = names.Count > 0 ? string.Join("\n", names) : "Unknown";
                RelicTracker.SetText(relic, "Relics Offered", text);
                ModLog.Info($"CallingBellRewardTracker: relics={text.Replace("\n", ", ")}");
            } catch { }
        }

        static string? GetRelicNameFromReward(Reward reward) {
            try {
                if (reward == null) return null;

                var relic = ReflectionUtil.GetMemberValue(reward, "ClaimedRelic")
                    ?? ReflectionUtil.GetMemberValue(reward, "_relic")
                    ?? ReflectionUtil.GetMemberValue(reward, "_predeterminedRelic");

                return ReflectionUtil.GetModelTitle(relic) ?? relic?.GetType().Name;
            } catch {
                return null;
            }
        }
    }
}
