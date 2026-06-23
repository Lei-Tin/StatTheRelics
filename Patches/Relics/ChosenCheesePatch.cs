using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ChosenCheese), nameof(ChosenCheese.AfterCombatEnd))]
    public static class ChosenCheesePatch {
        class ChosenCheeseState {
            public int MaxHp { get; set; }
        }

        static void Prefix(ChosenCheese __instance, CombatRoom _, ref object __state) {
            try {
                __state = new ChosenCheeseState {
                    MaxHp = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "MaxHp"))
                };
            } catch { }
        }

        static void Postfix(ChosenCheese __instance, Task __result, object __state) {
            try {
                var state = __state as ChosenCheeseState;
                if (state == null || state.MaxHp <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Max HP Gained", state.MaxHp);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) {
                        RelicTracker.AddAmount(__instance, "Max HP Gained", state.MaxHp);
                    }
                });
            } catch { }
        }
    }
}
