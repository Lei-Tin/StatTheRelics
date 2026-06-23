using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(EmberTea), nameof(EmberTea.AfterRoomEntered))]
    public static class EmberTeaPatch {
        const string TypeName = "MegaCrit.Sts2.Core.Models.Relics.EmberTea";

        class EmberTeaState {
            public int CombatsLeft { get; set; }
            public int Strength { get; set; }
        }

        static void Prefix(EmberTea __instance, AbstractRoom room, ref object __state) {
            try {
                if (__instance == null || room is not CombatRoom) return;
                var isUsedUp = ReflectionUtil.GetMemberValue(__instance, "IsUsedUp");
                if (isUsedUp is bool used && used) return;

                __state = new EmberTeaState {
                    CombatsLeft = ReflectionUtil.GetIntMemberValue(__instance, "CombatsLeft", 0),
                    Strength = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Strength", 2))
                };
            } catch { }
        }

        static void Postfix(EmberTea __instance, Task __result, object __state) {
            try {
                var state = __state as EmberTeaState;
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

        static void Count(EmberTea relic, EmberTeaState state) {
            try {
                var after = ReflectionUtil.GetIntMemberValue(relic, "CombatsLeft", state.CombatsLeft);
                if (after >= state.CombatsLeft) return;

                RelicTracker.AddAmountByType(TypeName, "Times Triggered", 1);
                if (state.Strength > 0) RelicTracker.AddAmountByType(TypeName, "Strength Gained", state.Strength);
            } catch { }
        }
    }
}
