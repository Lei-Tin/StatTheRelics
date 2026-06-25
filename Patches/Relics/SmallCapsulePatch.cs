using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(SmallCapsule), nameof(SmallCapsule.AfterObtained))]
    public static class SmallCapsulePatch {
        static readonly object Sync = new();
        static readonly object Recorded = new();
        static readonly ConditionalWeakTable<RelicReward, object> RecordedRewards = new();
        static SmallCapsule? activeRelic;

        static void Prefix(SmallCapsule __instance) {
            try {
                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(SmallCapsule __instance, Task __result) {
            try {
                if (__result == null) {
                    Clear(__instance);
                    return;
                }

                __result.ContinueWith(_ => {
                    try { Clear(__instance); } catch { }
                });
            } catch {
                Clear(__instance);
            }
        }

        static void Clear(SmallCapsule relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static SmallCapsule? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        internal static void RecordOfferedRelic(RelicReward reward) {
            try {
                var relic = ActiveRelic;
                if (relic == null || reward == null) return;

                lock (Recorded) {
                    if (RecordedRewards.TryGetValue(reward, out _)) return;
                    RecordedRewards.Add(reward, Recorded);
                }

                var offeredRelic = reward.Relic
                    ?? ReflectionUtil.GetMemberValue(reward, "_relic")
                    ?? ReflectionUtil.GetMemberValue(reward, "_predeterminedRelic");
                var name = ReflectionUtil.GetModelTitle(offeredRelic) ?? offeredRelic?.GetType().Name;
                if (!string.IsNullOrWhiteSpace(name)) RelicTracker.SetText(relic, "Relic Offered", name);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(RelicReward), nameof(RelicReward.Populate))]
    public static class SmallCapsuleRelicRewardPopulatePatch {
        static void Postfix(RelicReward __instance) {
            SmallCapsulePatch.RecordOfferedRelic(__instance);
        }
    }
}
