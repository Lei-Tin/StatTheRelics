using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ToyBox), nameof(ToyBox.AfterObtained))]
    public static class ToyBoxPatch {
        static readonly object Sync = new();
        static ToyBox? activeRelic;

        static void Prefix(ToyBox __instance) {
            try {
                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(ToyBox __instance, Task __result) {
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

        static void Clear(ToyBox relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static ToyBox? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        internal static void SetWaxRelics(ToyBox relic, IEnumerable<Reward> rewards) {
            try {
                var names = rewards
                    .OfType<RelicReward>()
                    .Select(reward => reward.Relic ?? ReflectionUtil.GetMemberValue(reward, "_relic") ?? ReflectionUtil.GetMemberValue(reward, "_predeterminedRelic"))
                    .Where(relicModel => relicModel != null)
                    .Select(relicModel => ReflectionUtil.GetModelTitle(relicModel) ?? relicModel!.GetType().Name)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList();

                if (names.Count > 0) RelicTracker.SetText(relic, "Wax Relics", string.Join("\n", names));
            } catch { }
        }
    }

    [HarmonyPatch(typeof(RewardsCmd), nameof(RewardsCmd.OfferCustom), new Type[] {
        typeof(Player),
        typeof(List<Reward>)
    })]
    public static class ToyBoxOfferCustomPatch {
        static void Prefix(Player player, List<Reward> rewards) {
            try {
                var relic = ToyBoxPatch.ActiveRelic;
                if (relic == null || player == null || rewards == null || relic.Owner != player) return;
                ToyBoxPatch.SetWaxRelics(relic, rewards);
            } catch { }
        }
    }
}
