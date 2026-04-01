using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Cracked Core channels lightning at the start of round 1.
    [HarmonyPatch(typeof(CrackedCore), nameof(CrackedCore.BeforeSideTurnStart))]
    public static class CrackedCorePatch {
        static void Postfix(CrackedCore __instance, object combatState) {
            try {
                var round = GetRoundNumber(combatState);
                if (round <= 1) {
                    RelicTracker.AddAmount(__instance, "Lightning Orbs Channeled", 1);
                }
                ModLog.Info($"CrackedCorePatch: round={round}, added={(round <= 1 ? 1 : 0)} lightning orb for {__instance.GetType().FullName}");
            } catch { }
        }

        static int GetRoundNumber(object? combatState) {
            try {
                if (combatState == null) return int.MaxValue;
                return ReflectionUtil.GetIntMemberValue(combatState, "RoundNumber", int.MaxValue);
            } catch {
                return int.MaxValue;
            }
        }
    }
}
