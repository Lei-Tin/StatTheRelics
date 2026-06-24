using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Gorget), nameof(Gorget.AfterRoomEntered))]
    public static class GorgetPatch {
        class PlatingState {
            public int Plating { get; set; }
        }

        static void Prefix(Gorget __instance, AbstractRoom room, ref object __state) {
            try {
                if (room is not CombatRoom) return;
                __state = new PlatingState {
                    Plating = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "PlatingPower", 4))
                };
            } catch { }
        }

        static void Postfix(Gorget __instance, Task __result, object __state) {
            try {
                var state = __state as PlatingState;
                if (state == null || state.Plating <= 0) return;

                if (__result == null) {
                    Count(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, state);
                    } catch { }
                });
            } catch { }
        }

        static void Count(Gorget relic, PlatingState state) {
            RelicTracker.AddAmount(relic, "Plating Gained", state.Plating);
        }
    }
}
