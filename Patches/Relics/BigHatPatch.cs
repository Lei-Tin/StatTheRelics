using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(BigHat), nameof(BigHat.AfterSideTurnStart))]
    public static class BigHatPatch {
        [ThreadStatic] internal static BigHat? Current;

        static void Prefix(BigHat __instance, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState) {
            try {
                if (__instance == null || participants == null) return;
                var owner = __instance.Owner;
                var ownerCreature = owner?.Creature;
                if (ownerCreature == null) return;
                if (!participants.Contains(ownerCreature)) return;
                var playerCombatState = owner?.PlayerCombatState;
                if (playerCombatState == null || playerCombatState.TurnNumber > 1) return;
                Current = __instance;
            } catch { }
        }

        static void Postfix() {
            Current = null;
        }
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.AddGeneratedCardsToCombat), new Type[] {
        typeof(IEnumerable<CardModel>),
        typeof(PileType),
        typeof(Player),
        typeof(CardPilePosition)
    })]
    public static class BigHatGeneratedCardsPatch {
        static void Postfix(Task<IReadOnlyList<CardPileAddResult>> __result) {
            try {
                var relic = BigHatPatch.Current;
                if (relic == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        var count = CountSuccessful(task.Result);
                        if (count > 0) RelicTracker.AddAmount(relic, "Ethereal Cards Given", count);
                    } catch { }
                });
            } catch { }
        }

        static int CountSuccessful(IEnumerable<CardPileAddResult>? results) {
            try {
                if (results == null) return 0;
                var count = 0;
                foreach (var result in results) {
                    var success = ReflectionUtil.GetMemberValue(result, "success");
                    if (success is bool ok && ok) count++;
                }
                return count;
            } catch {
                return 0;
            }
        }
    }
}
