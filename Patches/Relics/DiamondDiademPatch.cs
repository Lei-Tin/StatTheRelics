using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch]
    [PatchTargetAlternative(typeof(DiamondDiadem), "AfterSideTurnStart")]
    [PatchTargetAlternative(typeof(DiamondDiadem), "BeforeSideTurnEnd")]
    public static class DiamondDiademPatch {
        sealed class State {
            public bool UsesBlockEffect { get; set; }
            public int BlockBefore { get; set; }
        }

        static MethodBase TargetMethod() => PatchTargetResolver.RequireAny(
            typeof(DiamondDiadem),
            new PatchTargetCandidate("AfterSideTurnStart"),
            new PatchTargetCandidate("BeforeSideTurnEnd")
        );

        static void Prefix(DiamondDiadem __instance, MethodBase __originalMethod, object[] __args, ref object __state) {
            try {
                var owner = __instance?.Owner;
                var ownerCreature = owner?.Creature;
                if (ownerCreature == null || !IncludesOwner(__args, ownerCreature)) return;

                var usesBlockEffect = __originalMethod.Name == "AfterSideTurnStart";
                if (usesBlockEffect) {
                    if (owner?.PlayerCombatState?.TurnNumber > 1) return;
                } else {
                    var threshold = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "CardThreshold", 2));
                    var cardsPlayed = ReflectionUtil.GetIntMemberValue(__instance, "CardsPlayedThisTurn", int.MaxValue);
                    if (cardsPlayed > threshold) return;
                }

                __state = new State {
                    UsesBlockEffect = usesBlockEffect,
                    BlockBefore = GetBlock(ownerCreature)
                };
            } catch { }
        }

        static void Postfix(DiamondDiadem __instance, Task __result, object __state) {
            try {
                if (__state is not State state) return;
                if (__result == null) {
                    Count(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, state);
                    } catch { }
                });
            } catch { }
        }

        static void Count(DiamondDiadem relic, State state) {
            RelicTracker.AddAmount(relic, "Times Triggered", 1);
            if (!state.UsesBlockEffect) return;

            var gained = Math.Max(0, GetBlock(relic.Owner?.Creature) - state.BlockBefore);
            if (gained > 0) RelicTracker.AddAmount(relic, "Block Gained", gained);
        }

        static bool IncludesOwner(object[] arguments, Creature owner) {
            foreach (var argument in arguments) {
                if (argument is IEnumerable<Creature> participants) {
                    foreach (var participant in participants) {
                        if (ReferenceEquals(participant, owner)) return true;
                    }
                }
            }
            return false;
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
    }

    [HarmonyPatch]
    [VersionOptionalPatch]
    public static class DiamondDiademPowerPatch {
        static Type? PowerType => AccessTools.TypeByName("MegaCrit.Sts2.Core.Models.Powers.DiamondDiademPower");

        static bool Prepare() => PowerType != null;

        static MethodBase TargetMethod() {
            var type = PowerType ?? throw new TypeLoadException("DiamondDiademPower was expected for the legacy Diamond Diadem implementation.");
            return AccessTools.DeclaredMethod(type, "ModifyDamageMultiplicative")
                ?? throw new MissingMethodException(type.FullName, "ModifyDamageMultiplicative");
        }

        static void Postfix(object __instance, Creature target, decimal amount, decimal __result) {
            try {
                if (__instance == null || target == null || __result >= 1m) return;
                if (!ReferenceEquals(target, ReflectionUtil.GetMemberValue(__instance, "Owner"))) return;
                if (!IsFromCreatureDamageCommand()) return;

                var relic = ReflectionUtil.FindRelic<DiamondDiadem>(target);
                if (relic == null) return;

                var currentBlock = GetBlock(target);
                var incomingDamage = Math.Max(0, DecimalToInt(amount));
                var reducedDamage = Math.Max(0, DecimalToInt(amount * __result));
                var prevented = Math.Max(0,
                    Math.Max(0, incomingDamage - currentBlock) - Math.Max(0, reducedDamage - currentBlock));
                if (prevented > 0) RelicTracker.AddAmount(relic, "Damage Prevented", prevented);
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
                    if (typeName?.Contains("MegaCrit.Sts2.Core.Commands.CreatureCmd", StringComparison.Ordinal) == true) return true;
                }
            } catch { }
            return false;
        }
    }
}
