using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(SongBow), nameof(SongBow.AfterRoomEntered))]
    public static class SongBowPatch {
        static void Prefix(SongBow __instance, AbstractRoom room, out int __state) {
            __state = 0;
            try {
                if (room is not CombatRoom) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "AimPower"));
            } catch { }
        }

        static void Postfix(SongBow __instance, Task __result, int __state) {
            try {
                if (__state <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Aim Gained", __state);
                    } catch { }
                });
            } catch { }
        }
    }
}
