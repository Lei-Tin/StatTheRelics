using System;
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
                var dynamicVars = ReflectionUtil.GetMemberValue(relic, "DynamicVars");
                if (dynamicVars == null) {
                    ModLog.Info("BoundPhylacteryPatch: DynamicVars not found on relic; summon value unavailable.");
                    return 0;
                }

                var summonVar = ReflectionUtil.GetMemberValue(dynamicVars, "Summon");
                if (summonVar == null) {
                    ModLog.Info("BoundPhylacteryPatch: DynamicVars.Summon not found; summon value unavailable.");
                    return 0;
                }

                var raw = ReflectionUtil.GetMemberValue(summonVar, "BaseValue");
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
