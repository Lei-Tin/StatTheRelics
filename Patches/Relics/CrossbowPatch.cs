using System;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Crossbow), nameof(Crossbow.AfterSideTurnStart))]
    public static class CrossbowPatch {
        class State {
            public int CardsGenerated { get; set; }
        }

        static void Prefix(Crossbow __instance, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ref object __state) {
            try {
                _ = side;
                _ = combatState;
                if (__instance == null || participants == null) return;
                var owner = __instance.Owner;
                var ownerCreature = owner?.Creature;
                if (owner == null || ownerCreature == null || !participants.Contains(ownerCreature)) return;

                var hasAttackCards = owner.Character.CardPool
                    .GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint)
                    .Any(card => card.Type == CardType.Attack);
                if (!hasAttackCards) return;

                __state = new State { CardsGenerated = 1 };
            } catch { }
        }

        static void Postfix(Crossbow __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.CardsGenerated <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Cards Generated", state.CardsGenerated);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Cards Generated", state.CardsGenerated);
                    } catch { }
                });
            } catch { }
        }
    }
}
