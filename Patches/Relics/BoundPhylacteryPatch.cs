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

            } catch { }
        }

        static int GetSummonBaseValue(BoundPhylactery relic) {
            try {
                var dynamicVars = ReflectionUtil.GetMemberValue(relic, "DynamicVars");
                if (dynamicVars == null) {
                    return 0;
                }

                var summonVar = ReflectionUtil.GetMemberValue(dynamicVars, "Summon");
                if (summonVar == null) {
                    return 0;
                }

                var raw = ReflectionUtil.GetMemberValue(summonVar, "BaseValue");
                if (raw == null) {
                    return 0;
                }

                return Math.Max(0, Convert.ToInt32(raw));
            } catch {
                return 0;
            }
        }
    }
}
