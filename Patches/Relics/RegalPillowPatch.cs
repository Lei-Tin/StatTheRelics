using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(RegalPillow), nameof(RegalPillow.AfterRestSiteHeal))]
    public static class RegalPillowPatch {
        class State {
            public int BonusHealing { get; set; }
        }

        static void Prefix(RegalPillow __instance, Player player, bool isMimicked, ref object __state) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                __state = new State {
                    BonusHealing = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Heal", 15))
                };
            } catch { }
        }

        static void Postfix(RegalPillow __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.BonusHealing <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Bonus Healing Added", state.BonusHealing);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Bonus Healing Added", state.BonusHealing);
                    } catch { }
                });
            } catch { }
        }
    }
}
