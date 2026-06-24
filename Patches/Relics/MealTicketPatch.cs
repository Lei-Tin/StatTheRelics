using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(MealTicket), nameof(MealTicket.AfterRoomEntered))]
    public static class MealTicketPatch {
        class RoomState {
            public int BeforeHp { get; set; }
        }

        static void Prefix(MealTicket __instance, AbstractRoom room, ref object __state) {
            try {
                if (__instance?.Owner?.Creature == null || room is not MerchantRoom) return;
                if (__instance.Owner.Creature.IsDead) return;
                __state = new RoomState { BeforeHp = __instance.Owner.Creature.CurrentHp };
            } catch { }
        }

        static void Postfix(MealTicket __instance, Task __result, object __state) {
            try {
                var state = __state as RoomState;
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

        static void Count(MealTicket relic, RoomState state) {
            try {
                var healed = Math.Max(0, relic.Owner.Creature.CurrentHp - state.BeforeHp);
                if (healed > 0) RelicTracker.AddAmount(relic, "HP Healed", healed);
            } catch { }
        }
    }
}
