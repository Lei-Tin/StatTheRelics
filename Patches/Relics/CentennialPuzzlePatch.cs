using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(CentennialPuzzle), nameof(CentennialPuzzle.AfterDamageReceived))]
    public static class CentennialPuzzlePatch {
        class CentennialPuzzleState {
            public int Cards { get; set; }
        }

        static void Prefix(
            CentennialPuzzle __instance,
            PlayerChoiceContext choiceContext,
            Creature target,
            DamageResult result,
            ValueProp props,
            Creature dealer,
            CardModel cardSource,
            ref object __state
        ) {
            try {
                if (__instance == null || target != __instance.Owner?.Creature) return;
                if (!CombatManager.Instance.IsInProgress) return;
                if (result == null || result.UnblockedDamage <= 0) return;
                if (__instance.UsedThisCombat) return;

                __state = new CentennialPuzzleState {
                    Cards = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Cards"))
                };
            } catch { }
        }

        static void Postfix(CentennialPuzzle __instance, Task __result, object __state) {
            try {
                var state = __state as CentennialPuzzleState;
                if (state == null || state.Cards <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Cards Drawn", state.Cards);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) {
                        RelicTracker.AddAmount(__instance, "Cards Drawn", state.Cards);
                    }
                });
            } catch { }
        }
    }
}
