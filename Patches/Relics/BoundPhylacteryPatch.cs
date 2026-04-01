using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Bound Phylactery summons a pet via a private async helper.
    [HarmonyPatch(typeof(BoundPhylactery), "SummonPet")]
    public static class BoundPhylacteryPatch {
        static void Postfix(BoundPhylactery __instance) {
            try {
                var summons = GetSummonBaseValue(__instance);
                if (summons > 0) {
                    RelicTracker.AddAmount(__instance, "Summons", summons);
                }

                ModLog.Info($"BoundPhylacteryPatch: SummonPet called, added={summons}");
            } catch { }
        }

        static int GetSummonBaseValue(BoundPhylactery relic) {
            try {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var dynamicVarsProp = relic.GetType().GetProperty("DynamicVars", flags);
                var dynamicVars = dynamicVarsProp?.GetValue(relic);
                if (dynamicVars == null) {
                    ModLog.Info("BoundPhylacteryPatch: DynamicVars not found on relic; summon value unavailable.");
                    return 0;
                }

                var summonProp = dynamicVars.GetType().GetProperty("Summon", flags);
                var summonVar = summonProp?.GetValue(dynamicVars);
                if (summonVar == null) {
                    ModLog.Info("BoundPhylacteryPatch: DynamicVars.Summon not found; summon value unavailable.");
                    return 0;
                }

                var baseValueProp = summonVar.GetType().GetProperty("BaseValue", flags);
                var raw = baseValueProp?.GetValue(summonVar);
                if (raw == null) {
                    ModLog.Info("BoundPhylacteryPatch: DynamicVars.Summon.BaseValue not found; summon value unavailable.");
                    return 0;
                }

                return Math.Max(0, Convert.ToInt32(raw));
            } catch {
                ModLog.Info("BoundPhylacteryPatch: Failed to resolve summon base value via reflection.");
                return 0;
            }
        }
    }
}
