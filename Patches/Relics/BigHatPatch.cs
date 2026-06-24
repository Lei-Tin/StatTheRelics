using System;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(BigHat), nameof(BigHat.AfterSideTurnStart))]
    public static class BigHatPatch {
        class State {
            public int CardsGiven { get; set; }
        }

        static void Prefix(BigHat __instance, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ref object __state) {
            try {
                _ = side;
                _ = combatState;
                if (__instance == null || participants == null) return;
                var owner = __instance.Owner;
                var ownerCreature = owner?.Creature;
                if (owner == null || ownerCreature == null) return;
                if (!participants.Contains(ownerCreature)) return;
                var playerCombatState = owner?.PlayerCombatState;
                if (playerCombatState == null || playerCombatState.TurnNumber > 1) return;

                var character = owner!.Character;
                var runState = owner.RunState;
                if (character == null || runState == null) return;

                var hasEtherealCards = character.CardPool
                    .GetUnlockedCards(owner.UnlockState, runState.CardMultiplayerConstraint)
                    .Any(card => card.Keywords.Contains(CardKeyword.Ethereal));
                if (!hasEtherealCards) return;

                __state = new State {
                    CardsGiven = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Cards", 2))
                };
            } catch { }
        }

        static void Postfix(BigHat __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.CardsGiven <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Ethereal Cards Given", state.CardsGiven);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Ethereal Cards Given", state.CardsGiven);
                    } catch { }
                });
            } catch { }
        }
    }
}
