using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Capture actual healing done by Black Blood.
    [HarmonyPatch(typeof(BlackBlood), nameof(BlackBlood.AfterCombatVictory))]
    public static class BlackBloodPatch {
        class HpState { public object? Creature { get; set; } public int Before { get; set; } }

        static void Prefix(BlackBlood __instance, ref object __state) {
            try {
                var creature = __instance?.Owner?.Creature;
                var beforeHp = GetHp(creature);
                __state = new HpState { Creature = creature, Before = beforeHp };
                ModLog.Info($"BlackBloodPatch: Prefix creature={creature?.GetType().FullName ?? "null"}, beforeHp={beforeHp}");
            } catch { }
        }

        static void Postfix(BlackBlood __instance, ref Task __result, object __state) {
            try {
                var creature = (__state as HpState)?.Creature ?? __instance?.Owner?.Creature;
                var beforeHp = (__state as HpState)?.Before ?? GetHp(creature);
                var afterHp = GetHp(creature);
                var healed = Math.Max(0, afterHp - beforeHp);
                ModLog.Info($"BlackBloodPatch: Postfix creature={creature?.GetType().FullName ?? "null"}, beforeHp={beforeHp}, afterHp={afterHp}, healed={healed}");
                if (__instance != null && healed > 0) {
                    RelicTracker.AddAmount(__instance, "HP Healed", healed);
                } else if (__instance != null) {
                    ModLog.Info("BlackBloodPatch: no positive healing this combat");
                }
            } catch { }
        }

        static int GetHp(object? creature) {
            try {
                if (creature == null) return 0;
                var currentHp = ReflectionUtil.GetMemberValue(creature, "CurrentHp");
                ModLog.Info($"BlackBloodPatch: HP via member CurrentHp -> value={currentHp}");
                if (currentHp != null) return Convert.ToInt32(currentHp);
            } catch {
                ModLog.Info("BlackBloodPatch: failed to get HP via property");
            }
            return 0;
        }
    }
}