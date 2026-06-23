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
    [HarmonyPatch(typeof(Crossbow), nameof(Crossbow.AfterSideTurnStart))]
    public static class CrossbowPatch {
        [ThreadStatic] internal static Crossbow? Current;

        static void Prefix(Crossbow __instance, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState) {
            try {
                if (__instance == null || participants == null) return;
                var ownerCreature = __instance.Owner?.Creature;
                if (ownerCreature == null || !participants.Contains(ownerCreature)) return;
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
    public static class CrossbowGeneratedCardsPatch {
        static void Prefix(Player creator, ref object __state) {
            try {
                var relic = CrossbowPatch.Current;
                if (relic == null || creator != relic.Owner) return;
                __state = relic;
            } catch { }
        }

        static void Postfix(Task<IReadOnlyList<CardPileAddResult>> __result, object __state) {
            try {
                if (__state is not Crossbow relic || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
                        var count = CountSuccessful(task.Result);
                        if (count > 0) RelicTracker.AddAmount(relic, "Cards Generated", count);
                    } catch { }
                });
            } catch { }
        }

        static int CountSuccessful(IEnumerable<CardPileAddResult> results) {
            var count = 0;
            foreach (var result in results) {
                var success = ReflectionUtil.GetMemberValue(result, "success");
                if (success is bool ok && ok) count++;
            }
            return count;
        }
    }
}
