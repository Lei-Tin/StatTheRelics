using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(DiamondDiadem), nameof(DiamondDiadem.BeforeSideTurnEnd))]
    public static class DiamondDiademPatch {
        static void Prefix(DiamondDiadem __instance, PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ref bool __state) {
            try {
                if (__instance == null || participants == null) return;
                var ownerCreature = __instance.Owner?.Creature;
                if (ownerCreature == null || !participants.Contains(ownerCreature)) return;
                var threshold = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "CardThreshold", 2));
                if (__instance.CardsPlayedThisTurn > threshold) return;
                __state = true;
            } catch { }
        }

        static void Postfix(DiamondDiadem __instance, Task __result, bool __state) {
            try {
                if (!__state) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Times Triggered", 1);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            RelicTracker.AddAmount(__instance, "Times Triggered", 1);
                        }
                    } catch { }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(DiamondDiademPower), nameof(DiamondDiademPower.ModifyDamageMultiplicative))]
    public static class DiamondDiademPowerPatch {
        static void Postfix(DiamondDiademPower __instance, Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource, decimal __result) {
            try {
                if (__instance == null || target == null) return;
                if (__result >= 1m) return;
                if (target != __instance.Owner) return;
                if (!IsFromCreatureDamageCommand()) return;

                var relic = ReflectionUtil.FindRelic<DiamondDiadem>(target);
                if (relic == null) return;

                var block = GetBlock(target);
                var incomingDamage = Math.Max(0, DecimalToInt(amount));
                var reducedDamage = Math.Max(0, DecimalToInt(amount * __result));
                var hpDamageBeforeDiadem = Math.Max(0, incomingDamage - block);
                var hpDamageAfterDiadem = Math.Max(0, reducedDamage - block);
                var prevented = Math.Max(0, hpDamageBeforeDiadem - hpDamageAfterDiadem);
                if (prevented <= 0) return;

                RelicTracker.AddAmount(relic, "Damage Prevented", prevented);
                ModLog.Info($"DiamondDiademPowerPatch: amount={amount}, multiplier={__result}, block={block}, prevented={prevented}");
            } catch { }
        }

        static int GetBlock(object? creature) {
            try {
                var block = ReflectionUtil.GetMemberValue(creature, "Block")
                    ?? ReflectionUtil.GetMemberValue(creature, "CurrentBlock");
                return block == null ? 0 : Math.Max(0, Convert.ToInt32(block));
            } catch {
                return 0;
            }
        }

        static int DecimalToInt(decimal value) {
            try {
                return Convert.ToInt32(Math.Round(value, MidpointRounding.AwayFromZero));
            } catch {
                return 0;
            }
        }

        static bool IsFromCreatureDamageCommand() {
            try {
                var frames = new StackTrace().GetFrames();
                if (frames == null) return false;
                foreach (var frame in frames) {
                    var typeName = frame.GetMethod()?.DeclaringType?.FullName;
                    if (typeName != null && typeName.Contains("MegaCrit.Sts2.Core.Commands.CreatureCmd", StringComparison.Ordinal)) return true;
                }
            } catch { }

            return false;
        }
    }
}
