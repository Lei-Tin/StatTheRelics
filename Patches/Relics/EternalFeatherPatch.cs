using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(EternalFeather), nameof(EternalFeather.AfterRoomEntered))]
    public static class EternalFeatherPatch {
        class HpState {
            public object? Creature { get; set; }
            public int Before { get; set; }
        }

        static void Prefix(EternalFeather __instance, AbstractRoom room, ref object __state) {
            try {
                if (__instance == null || room is not RestSiteRoom) return;
                var creature = __instance.Owner?.Creature;
                __state = new HpState { Creature = creature, Before = GetHp(creature) };
            } catch { }
        }

        static void Postfix(EternalFeather __instance, Task __result, object __state) {
            try {
                var state = __state as HpState;
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

        static void Count(EternalFeather relic, HpState state) {
            try {
                var creature = state.Creature ?? relic.Owner?.Creature;
                var healed = Math.Max(0, GetHp(creature) - state.Before);
                if (healed > 0) RelicTracker.AddAmount(relic, "HP Healed", healed);
            } catch { }
        }

        static int GetHp(object? creature) {
            try {
                var currentHp = ReflectionUtil.GetMemberValue(creature, "CurrentHp");
                return currentHp == null ? 0 : Convert.ToInt32(currentHp);
            } catch {
                return 0;
            }
        }
    }
}
