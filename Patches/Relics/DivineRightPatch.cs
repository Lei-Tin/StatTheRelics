using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Divine Right grants stars when entering a combat room.
    [HarmonyPatch(typeof(DivineRight), nameof(DivineRight.AfterRoomEntered))]
    public static class DivineRightPatch {
        static void Postfix(DivineRight __instance, object room) {
            try {
                var combat = IsCombatRoom(room);
                ModLog.Info($"DivineRightPatch: room={room?.GetType().FullName ?? "null"}, combat={combat}");

                if (!combat) {
                    return;
                }

                var starsGained = GetStarsBaseValue(__instance);
                if (starsGained > 0) {
                    RelicTracker.AddAmount(__instance, "Stars Gained", starsGained);
                }

                ModLog.Info($"DivineRightPatch: stars={starsGained}");
            } catch { }
        }

        static bool IsCombatRoom(object? room) {
            if (room == null) return false;

            var roomType = room.GetType();
            while (roomType != null) {
                if (string.Equals(roomType.Name, "CombatRoom", StringComparison.Ordinal)) {
                    return true;
                }
                roomType = roomType.BaseType;
            }

            return false;
        }

        static int GetStarsBaseValue(DivineRight relic) {
            try {
                var dynamicVars = ReflectionUtil.GetMemberValue(relic, "DynamicVars");
                if (dynamicVars == null) return 0;

                var starsVar = ReflectionUtil.GetMemberValue(dynamicVars, "Stars");
                if (starsVar == null) return 0;

                var raw = ReflectionUtil.GetMemberValue(starsVar, "BaseValue");
                if (raw == null) return 0;

                return Math.Max(0, Convert.ToInt32(raw));
            } catch {
                return 0;
            }
        }
    }
}
