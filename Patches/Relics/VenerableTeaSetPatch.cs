using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(VenerableTeaSet), nameof(VenerableTeaSet.AfterEnergyReset))]
    public static class VenerableTeaSetPatch {
        class State {
            public int Energy { get; set; }
        }

        static void Prefix(VenerableTeaSet __instance, Player player, ref object __state) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                if (!__instance.GainEnergyInNextCombat) return;

                __state = new State {
                    Energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Energy", 2))
                };
            } catch { }
        }

        static void Postfix(VenerableTeaSet __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.Energy <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Energy Gained", state.Energy);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Energy Gained", state.Energy);
                    } catch { }
                });
            } catch { }
        }
    }
}
