using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch]
    public static class BookRepairKnifePatch {
        class BookRepairKnifeState {
            public BookRepairKnife? Relic { get; set; }
            public Creature? Creature { get; set; }
            public int BeforeHp { get; set; }
            public int EligibleDoomDeaths { get; set; }
        }

        static MethodBase TargetMethod() {
            return AccessTools.Method(typeof(BookRepairKnife), "AfterDiedToDoom")
                ?? throw new MissingMethodException("BookRepairKnife.AfterDiedToDoom not found");
        }

        static void Prefix(BookRepairKnife __instance, PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures, ref object __state) {
            try {
                if (__instance == null || creatures == null) return;
                var ownerCreature = __instance.Owner?.Creature;
                if (ownerCreature == null) return;

                var eligibleDeaths = CountEligibleDeaths(__instance, creatures);
                if (eligibleDeaths <= 0) return;

                __state = new BookRepairKnifeState {
                    Relic = __instance,
                    Creature = ownerCreature,
                    BeforeHp = ownerCreature.CurrentHp,
                    EligibleDoomDeaths = eligibleDeaths
                };
            } catch { }
        }

        static void Postfix(Task __result, object __state) {
            try {
                var state = __state as BookRepairKnifeState;
                if (state?.Relic == null || state.Creature == null || state.EligibleDoomDeaths <= 0) return;

                if (__result == null) {
                    CountHeal(state);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) CountHeal(state);
                });
            } catch { }
        }

        static int CountEligibleDeaths(BookRepairKnife relic, IEnumerable<Creature> creatures) {
            try {
                var count = 0;
                var ownerCreature = relic.Owner?.Creature;
                foreach (var creature in creatures) {
                    if (creature == null || creature == ownerCreature) continue;
                    if (!AllPowersAllowFatalTrigger(creature)) continue;
                    count++;
                }
                return count;
            } catch {
                return 0;
            }
        }

        static bool AllPowersAllowFatalTrigger(Creature creature) {
            try {
                foreach (var power in creature.Powers) {
                    if (power == null) continue;
                    if (!power.ShouldOwnerDeathTriggerFatal()) return false;
                }
                return true;
            } catch {
                return false;
            }
        }

        static void CountHeal(BookRepairKnifeState state) {
            try {
                if (state.Relic == null || state.Creature == null) return;
                var amount = Math.Max(0, state.Creature.CurrentHp - state.BeforeHp);
                if (amount > 0) RelicTracker.AddAmount(state.Relic, "HP Healed", amount);
            } catch { }
        }
    }
}
