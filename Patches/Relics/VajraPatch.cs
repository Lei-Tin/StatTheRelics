using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Vajra), nameof(Vajra.AfterRoomEntered))]
    public static class VajraPatch {
        class State {
            public int Strength { get; set; }
        }

        static void Prefix(Vajra __instance, AbstractRoom room, ref object __state) {
            try {
                if (room is not CombatRoom) return;
                __state = new State {
                    Strength = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Strength", 1))
                };
            } catch { }
        }

        static void Postfix(Vajra __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.Strength <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Strength Gained", state.Strength);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Strength Gained", state.Strength);
                    } catch { }
                });
            } catch { }
        }
    }
}
