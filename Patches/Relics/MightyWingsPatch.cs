using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(MightyWings), nameof(MightyWings.AfterDamageGiven))]
    public static class MightyWingsPatch {
        static void Prefix(MightyWings __instance, Creature dealer, DamageResult result, ValueProp props, out int __state) {
            __state = 0;
            try {
                if (dealer == null || !props.IsPoweredAttack() || (dealer != __instance.Owner.Creature && dealer != __instance.Owner.Osty) || result.TotalDamage < ReflectionUtil.GetDynamicVarIntValue(__instance, "DamageThreshold")) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "StrengthPower"));
            } catch { }
        }

        static void Postfix(MightyWings __instance, Task __result, int __state) {
            try {
                if (__state <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        RelicTracker.AddAmount(__instance, "Strength Gained", __state);
                    } catch { }
                }, TaskContinuationOptions.ExecuteSynchronously);
            } catch { }
        }
    }

}
