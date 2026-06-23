using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(FakeVenerableTeaSet), nameof(FakeVenerableTeaSet.AfterEnergyReset))]
    public static class FakeVenerableTeaSetPatch {
        class EnergyState {
            public int Energy { get; set; }
        }

        static void Prefix(FakeVenerableTeaSet __instance, Player player, ref object __state) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                if (!__instance.GainEnergyInNextCombat) return;

                __state = new EnergyState {
                    Energy = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Energy", 1))
                };
            } catch { }
        }

        static void Postfix(FakeVenerableTeaSet __instance, Task __result, object __state) {
            try {
                var state = __state as EnergyState;
                if (state == null || state.Energy <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Energy Gained", state.Energy);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            RelicTracker.AddAmount(__instance, "Energy Gained", state.Energy);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
