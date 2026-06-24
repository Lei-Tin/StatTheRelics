using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(DigRestSiteOption), nameof(DigRestSiteOption.OnSelect))]
    public static class ShovelPatch {
        static readonly object Sync = new();
        static Shovel? activeRelic;

        static void Prefix(DigRestSiteOption __instance) {
            try {
                var owner = ReflectionUtil.GetMemberValue(__instance, "Owner");
                var relic = ReflectionUtil.FindRelic<Shovel>(owner);
                if (relic == null) return;
                lock (Sync) activeRelic = relic;
            } catch { }
        }

        static void Postfix(Task<bool> __result) {
            try {
                if (__result == null) {
                    Clear();
                    return;
                }

                __result.ContinueWith(_ => {
                    try { Clear(); } catch { }
                });
            } catch {
                Clear();
            }
        }

        static void Clear() {
            lock (Sync) activeRelic = null;
        }

        internal static Shovel? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        internal static void CountRelic(Shovel shovel, RelicModel obtainedRelic) {
            try {
                if (shovel == null || obtainedRelic == null || ReferenceEquals(shovel, obtainedRelic)) return;
                RelicTracker.AddAmount(shovel, "Relics Dug", 1);

                var name = ReflectionUtil.GetModelTitle(obtainedRelic) ?? obtainedRelic.GetType().Name;
                var current = RelicTracker.GetText(shovel, "Relics Dug Up");
                RelicTracker.SetText(shovel, "Relics Dug Up", string.IsNullOrWhiteSpace(current) ? name : current + "\n" + name);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(RelicCmd), nameof(RelicCmd.Obtain), new Type[] {
        typeof(RelicModel),
        typeof(Player),
        typeof(int)
    })]
    public static class ShovelRelicObtainPatch {
        static void Postfix(RelicModel relic, Player player, Task<RelicModel> __result) {
            try {
                var shovel = ShovelPatch.ActiveRelic;
                if (shovel == null || player == null || shovel.Owner != player || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        ShovelPatch.CountRelic(shovel, task.Result ?? relic);
                    } catch { }
                });
            } catch { }
        }
    }
}
