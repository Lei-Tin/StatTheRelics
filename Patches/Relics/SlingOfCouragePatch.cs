using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(SlingOfCourage), nameof(SlingOfCourage.AfterRoomEntered))]
    public static class SlingOfCouragePatch {
        static void Prefix(SlingOfCourage __instance, AbstractRoom room, ref object __state) {
            try {
                if (__instance == null || room == null) return;
                if (Convert.ToInt32(room.RoomType) != 2) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Strength", 2));
            } catch { }
        }

        static void Postfix(SlingOfCourage __instance, Task __result, object __state) {
            try {
                if (__state is not int strength || strength <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Strength Gained", strength);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Strength Gained", strength);
                    } catch { }
                });
            } catch { }
        }
    }
}
