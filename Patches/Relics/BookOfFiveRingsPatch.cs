using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch]
    public static class BookOfFiveRingsPatch {
        class BookOfFiveRingsState {
            public BookOfFiveRings? Relic { get; set; }
            public Creature? Creature { get; set; }
            public int BeforeHp { get; set; }
        }

        static MethodBase TargetMethod() {
            return AccessTools.Method(typeof(BookOfFiveRings), "AfterCardChangedPiles")
                ?? throw new MissingMethodException("BookOfFiveRings.AfterCardChangedPiles not found");
        }

        static void Prefix(BookOfFiveRings __instance, ref object __state) {
            try {
                var creature = __instance?.Owner?.Creature;
                if (__instance == null || creature == null) return;

                __state = new BookOfFiveRingsState {
                    Relic = __instance,
                    Creature = creature,
                    BeforeHp = creature.CurrentHp
                };
            } catch { }
        }

        static void Postfix(Task __result, object __state) {
            try {
                var state = __state as BookOfFiveRingsState;
                if (state?.Relic == null || state.Creature == null) return;

                if (__result == null) {
                    CountHeal(state);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) CountHeal(state);
                });
            } catch { }
        }

        static void CountHeal(BookOfFiveRingsState state) {
            try {
                if (state.Relic == null || state.Creature == null) return;
                var amount = Math.Max(0, state.Creature.CurrentHp - state.BeforeHp);
                if (amount > 0) RelicTracker.AddAmount(state.Relic, "HP Healed", amount);
            } catch { }
        }
    }
}
