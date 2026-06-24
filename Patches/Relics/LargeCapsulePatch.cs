using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(LargeCapsule), nameof(LargeCapsule.AfterObtained))]
    public static class LargeCapsulePatch {
        static readonly object Sync = new();
        static LargeCapsule? activeRelic;

        static void Prefix(LargeCapsule __instance) {
            try {
                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(LargeCapsule __instance, Task __result) {
            try {
                if (__result == null) {
                    Clear(__instance);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        Clear(__instance);
                    } catch { }
                });
            } catch {
                Clear(__instance);
            }
        }

        static void Clear(LargeCapsule relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static LargeCapsule? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        internal static void CountRelic(LargeCapsule relic, RelicModel obtainedRelic) {
            try {
                if (relic == null || obtainedRelic == null) return;
                if (obtainedRelic is LargeCapsule) return;
                var name = ReflectionUtil.GetModelTitle(obtainedRelic) ?? obtainedRelic.GetType().Name;
                var current = RelicTracker.GetText(relic, "Relic Added");
                var value = string.IsNullOrWhiteSpace(current) ? name : current + "\n" + name;
                RelicTracker.SetText(relic, "Relic Added", value);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(RelicCmd), nameof(RelicCmd.Obtain), new Type[] {
        typeof(RelicModel),
        typeof(Player),
        typeof(int)
    })]
    public static class LargeCapsuleRelicObtainPatch {
        static void Postfix(RelicModel relic, Player player, Task<RelicModel> __result) {
            try {
                var capsule = LargeCapsulePatch.ActiveRelic;
                if (capsule == null || player == null || capsule.Owner != player || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        LargeCapsulePatch.CountRelic(capsule, task.Result ?? relic);
                    } catch { }
                });
            } catch { }
        }
    }
}
