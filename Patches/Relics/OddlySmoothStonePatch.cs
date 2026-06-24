using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(OddlySmoothStone), nameof(OddlySmoothStone.AfterRoomEntered))]
    public static class OddlySmoothStonePatch {
        static void Prefix(OddlySmoothStone __instance, AbstractRoom room, ref object __state) {
            try {
                if (room is not CombatRoom) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Dexterity", 1));
            } catch { }
        }

        static void Postfix(OddlySmoothStone __instance, Task __result, object __state) {
            try {
                var amount = __state is int value ? value : 0;
                if (amount <= 0 || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Dexterity Gained", amount);
                    } catch { }
                });
            } catch { }
        }
    }
}
