using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(MawBank), nameof(MawBank.AfterRoomEntered))]
    public static class MawBankPatch {
        class RoomState {
            public int Gold { get; set; }
        }

        static void Prefix(MawBank __instance, AbstractRoom room, ref object __state) {
            try {
                if (__instance == null || room == null) return;
                if (__instance.Owner?.RunState?.BaseRoom != room) return;
                if (__instance.HasItemBeenBought) return;

                __state = new RoomState {
                    Gold = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Gold", 12))
                };
            } catch { }
        }

        static void Postfix(MawBank __instance, Task __result, object __state) {
            try {
                var state = __state as RoomState;
                if (state == null || state.Gold <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Gold Gained", state.Gold);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            RelicTracker.AddAmount(__instance, "Gold Gained", state.Gold);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
