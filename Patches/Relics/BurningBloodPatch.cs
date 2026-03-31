using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Capture actual healing done by Burning Blood (clamped to current HP) by wrapping the async task.
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
                if (healed > 0 && __instance != null) RelicTracker.AddAmount(__instance, "HP Healed", healed);
            } catch { }
        }

        static int GetHp(object? creature) {
            try {
                if (creature == null) return 0;
                var type = creature.GetType();
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                var prop = type.GetProperty("CurrentHp", flags);
                ModLog.Info($"BurningBloodPatch: HP via property CurrentHp -> prop={prop?.GetValue(creature)}");
                if (prop != null) return Convert.ToInt32(prop.GetValue(creature));
            } catch { 
                ModLog.Info("BurningBloodPatch: failed to get HP via property");
            }
            return 0;
        }
    }
}
