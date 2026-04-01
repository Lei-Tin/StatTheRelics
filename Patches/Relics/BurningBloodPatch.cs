using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Capture actual healing done by Burning Blood
    [HarmonyPatch(typeof(BurningBlood), nameof(BurningBlood.AfterCombatVictory))]
    public static class BurningBloodPatch {
        class HpState { public object? Creature { get; set; } public int Before { get; set; } }

        static void Prefix(BurningBlood __instance, ref object __state) {
            try {
                var creature = __instance?.Owner?.Creature;
                var beforeHp = GetHp(creature);
                __state = new HpState { Creature = creature, Before = beforeHp };
                ModLog.Info($"BurningBloodPatch: Prefix creature={creature?.GetType().FullName ?? "null"}, beforeHp={beforeHp}");
            } catch { }
        }

        static void Postfix(BurningBlood __instance, ref Task __result, object __state) {
            try {
                var creature = ( __state as HpState )?.Creature ?? __instance?.Owner?.Creature;
                var beforeHp = ( __state as HpState )?.Before ?? GetHp(creature);
                var afterHp = GetHp(creature);
                var healed = Math.Max(0, afterHp - beforeHp);
                ModLog.Info($"BurningBloodPatch: Postfix creature={creature?.GetType().FullName ?? "null"}, beforeHp={beforeHp}, afterHp={afterHp}, healed={healed}");
                if (__instance != null && healed > 0) {
                    RelicTracker.AddAmount(__instance, "HP Healed", healed);
                } else if (__instance != null) {
                    ModLog.Info("BurningBloodPatch: no positive healing this combat");
                }
            } catch { }
        }

        static int GetHp(object? creature) {
            try {
                if (creature == null) return 0;
                var currentHp = ReflectionUtil.GetMemberValue(creature, "CurrentHp");
                ModLog.Info($"BurningBloodPatch: HP via member CurrentHp -> value={currentHp}");
                if (currentHp != null) return Convert.ToInt32(currentHp);
            } catch { 
                ModLog.Info("BurningBloodPatch: failed to get HP via property");
            }
            return 0;
        }
    }
}
