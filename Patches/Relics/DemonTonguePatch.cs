using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(DemonTongue), nameof(DemonTongue.AfterDamageReceived))]
    public static class DemonTonguePatch {
        class DemonTongueState {
            public int BeforeHp { get; set; }
            public int IntendedHeal { get; set; }
        }

        static void Prefix(
            DemonTongue __instance,
            PlayerChoiceContext choiceContext,
            Creature target,
            DamageResult result,
            ValueProp props,
            Creature dealer,
            CardModel cardSource,
            ref object __state
        ) {
            try {
                if (__instance == null || target == null || result == null) return;
                var ownerCreature = __instance.Owner?.Creature;
                if (ownerCreature == null || target != ownerCreature) return;
                if (ownerCreature.CombatState == null || ownerCreature.CombatState.CurrentSide != ownerCreature.Side) return;
                if (result.UnblockedDamage <= 0) return;
                if (ReflectionUtil.GetMemberValue(__instance, "_triggeredThisTurn") is bool triggered && triggered) return;
                __state = new DemonTongueState {
                    BeforeHp = GetHp(ownerCreature),
                    IntendedHeal = Math.Max(0, result.UnblockedDamage)
                };
            } catch { }
        }

        static void Postfix(DemonTongue __instance, Task __result, object __state) {
            try {
                var state = __state as DemonTongueState;
                if (state == null || state.IntendedHeal <= 0) return;

                if (__result == null) {
                    CountActualHealing(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            CountActualHealing(__instance, state);
                        }
                    } catch { }
                });
            } catch { }
        }

        static void CountActualHealing(DemonTongue relic, DemonTongueState state) {
            var afterHp = GetHp(relic.Owner?.Creature);
            var healed = Math.Max(0, afterHp - state.BeforeHp);
            if (healed > 0) RelicTracker.AddAmount(relic, "HP Healed", healed);
        }

        static int GetHp(object? creature) {
            try {
                var currentHp = ReflectionUtil.GetMemberValue(creature, "CurrentHp");
                if (currentHp != null) return Convert.ToInt32(currentHp);
            } catch { }

            return 0;
        }
    }
}
