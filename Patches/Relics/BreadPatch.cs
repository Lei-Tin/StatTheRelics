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
                    ModLog.Info("BreadPatch: Postfix skipped because instance is null");
                    return;
                }

                var ownerSide = __instance.Owner?.Creature?.Side;
                var round = ReflectionUtil.GetIntMemberValue(combatState, "RoundNumber", int.MaxValue);
                ModLog.Info($"BreadPatch: Postfix entered round={round}, side={side}, ownerSide={ownerSide}");

                if (ownerSide == null || side != ownerSide) {
                    ModLog.Info("BreadPatch: skipped because side does not match owner side");
                    return;
                }

                if (round == 1) {
                    var loseEnergy = ResolveDynamicVar(__instance, "LoseEnergy");
                    ModLog.Info($"BreadPatch: round 1 detected, LoseEnergy={loseEnergy}");
                    if (loseEnergy > 0) {
                        RelicTracker.AddAmount(__instance, "Energy Lost", loseEnergy);
                        ModLog.Info($"BreadPatch: incremented Energy Lost by {loseEnergy}");
                    } else {
                        ModLog.Info("BreadPatch: LoseEnergy <= 0, no increment applied");
                    }
                    return;
                }

                if (round > 1) {
                    var gainEnergy = ResolveDynamicVar(__instance, "GainEnergy");
                    ModLog.Info($"BreadPatch: round > 1 detected, GainEnergy={gainEnergy}");
                    if (gainEnergy > 0) {
                        RelicTracker.AddAmount(__instance, "Energy Gained", gainEnergy);
                        ModLog.Info($"BreadPatch: incremented Energy Gained by {gainEnergy}");
                    } else {
                        ModLog.Info("BreadPatch: GainEnergy <= 0, no increment applied");
                    }
                    return;
                }

                ModLog.Info("BreadPatch: round <= 0, no increment branch taken");
            } catch (Exception ex) {
                ModLog.Info($"BreadPatch: Postfix exception {ex.GetType().Name}: {ex.Message}");
            }
        }

        static int ResolveDynamicVar(Bread? relic, string key) {
            try {
                if (relic == null || string.IsNullOrWhiteSpace(key)) return 0;

                var dynamicVars = ReflectionUtil.GetMemberValue(relic, "DynamicVars");
                if (dynamicVars == null) {
                    ModLog.Info($"BreadPatch: DynamicVars not found while resolving '{key}'");
                    return 0;
                }

                var dynamicVar = ResolveDynamicVarByKey(dynamicVars, key);
                if (dynamicVar == null) {
                    ModLog.Info($"BreadPatch: DynamicVars['{key}'] could not be resolved");
                    return 0;
                }

                var baseValueRaw = ReflectionUtil.GetMemberValue(dynamicVar, "BaseValue");
                var intValueRaw = ReflectionUtil.GetMemberValue(dynamicVar, "IntValue");
                var raw = baseValueRaw ?? intValueRaw;

                if (raw == null) {
                    ModLog.Info($"BreadPatch: DynamicVars['{key}'] has neither BaseValue nor IntValue");
                    return 0;
                }

                var value = Math.Max(0, Convert.ToInt32(raw));
                ModLog.Info($"BreadPatch: resolved DynamicVars['{key}'] value={value} (raw type={raw.GetType().Name})");
                return value;
            } catch {
                ModLog.Info($"BreadPatch: failed to resolve dynamic var '{key}'");
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
                ModLog.Info($"BreadPatch: indexer lookup failed for key '{key}'");
            }

            return null;
        }
    }
}