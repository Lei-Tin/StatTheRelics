using System;
using System.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    // Track Bread's turn-1 energy loss and subsequent turn energy gain.
    [HarmonyPatch(typeof(Bread), nameof(Bread.AfterSideTurnStart))]
    public static class BreadPatch {
        static void Postfix(Bread __instance, CombatSide side, CombatState combatState) {
            try {
                if (__instance == null) {
                    return;
                }

                var ownerSide = __instance.Owner?.Creature?.Side;
                var round = ReflectionUtil.GetIntMemberValue(combatState, "RoundNumber", int.MaxValue);

                if (ownerSide == null || side != ownerSide) {
                    return;
                }

                if (round == 1) {
                    var loseEnergy = ResolveDynamicVar(__instance, "LoseEnergy");
                    if (loseEnergy > 0) {
                        RelicTracker.AddAmount(__instance, "Energy Lost", loseEnergy);
                    } else {
                    }
                    return;
                }

                if (round > 1) {
                    var gainEnergy = ResolveDynamicVar(__instance, "GainEnergy");
                    if (gainEnergy > 0) {
                        RelicTracker.AddAmount(__instance, "Energy Gained", gainEnergy);
                    } else {
                    }
                    return;
                }

            } catch {
            }
        }

        static int ResolveDynamicVar(Bread? relic, string key) {
            try {
                if (relic == null || string.IsNullOrWhiteSpace(key)) return 0;

                var dynamicVars = ReflectionUtil.GetMemberValue(relic, "DynamicVars");
                if (dynamicVars == null) {
                    return 0;
                }

                var dynamicVar = ResolveDynamicVarByKey(dynamicVars, key);
                if (dynamicVar == null) {
                    return 0;
                }

                var baseValueRaw = ReflectionUtil.GetMemberValue(dynamicVar, "BaseValue");
                var intValueRaw = ReflectionUtil.GetMemberValue(dynamicVar, "IntValue");
                var raw = baseValueRaw ?? intValueRaw;

                if (raw == null) {
                    return 0;
                }

                var value = Math.Max(0, Convert.ToInt32(raw));
                return value;
            } catch {
                return 0;
            }
        }

        static object? ResolveDynamicVarByKey(object dynamicVars, string key) {
            try {
                if (dynamicVars is IDictionary dict && dict.Contains(key)) {
                    return dict[key];
                }

                var type = dynamicVars.GetType();

                var indexer = type.GetProperty("Item", new[] { typeof(string) });
                if (indexer != null) {
                    return indexer.GetValue(dynamicVars, new object[] { key });
                }

                var getItem = type.GetMethod("get_Item", new[] { typeof(string) });
                if (getItem != null) {
                    return getItem.Invoke(dynamicVars, new object[] { key });
                }
            } catch {
            }

            return null;
        }
    }
}
