using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(SwordOfJade), nameof(SwordOfJade.AfterRoomEntered))]
    public static class SwordOfJadePatch {
        static void Prefix(SwordOfJade __instance, AbstractRoom room, ref object __state) {
            try {
                if (__instance == null || room is not CombatRoom) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Strength", 3));
            } catch { }
        }

        static void Postfix(SwordOfJade __instance, Task __result, object __state) {
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
