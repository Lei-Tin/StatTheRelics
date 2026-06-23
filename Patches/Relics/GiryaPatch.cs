using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Girya), nameof(Girya.AfterRoomEntered))]
    public static class GiryaCombatStartPatch {
        class CombatState {
            public int Strength { get; set; }
        }

        static void Prefix(Girya __instance, AbstractRoom room, ref object __state) {
            try {
                if (__instance == null || room is not CombatRoom) return;
                if (__instance.TimesLifted <= 0) return;
                __state = new CombatState { Strength = __instance.TimesLifted };
            } catch { }
        }

        static void Postfix(Girya __instance, Task __result, object __state) {
            try {
                var state = __state as CombatState;
                if (state == null) return;

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

        static void Count(Girya relic, CombatState state) {
            try {
                RelicTracker.AddAmount(relic, "Strength Gained", state.Strength);
            } catch { }
        }
    }
}
