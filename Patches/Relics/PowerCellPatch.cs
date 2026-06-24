using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PowerCell), nameof(PowerCell.BeforeSideTurnStart))]
    public static class PowerCellPatch {
        static readonly object Sync = new();
        static PowerCell? activeRelic;

        static void Prefix(PowerCell __instance, IReadOnlyList<Creature> participants, ICombatState combatState) {
            try {
                if (__instance?.Owner?.Creature == null || participants == null) return;
                if (!participants.Contains(__instance.Owner.Creature)) return;
                if (combatState == null || combatState.RoundNumber > 1) return;

                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(PowerCell __instance, Task __result) {
            try {
                if (__result == null) {
                    Clear(__instance);
                    return;
                }

                __result.ContinueWith(_ => {
                    try { Clear(__instance); } catch { }
                });
            } catch {
                Clear(__instance);
            }
        }

        static void Clear(PowerCell relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static PowerCell? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Add), new Type[] {
        typeof(IEnumerable<CardModel>),
        typeof(PileType),
        typeof(CardPilePosition),
        typeof(AbstractModel),
        typeof(bool)
    })]
    public static class PowerCellCardPileAddPatch {
        static void Postfix(Task<IReadOnlyList<CardPileAddResult>> __result) {
            try {
                var relic = PowerCellPatch.ActiveRelic;
                if (relic == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        var count = CountSuccessful(task.Result);
                        if (count <= 0) return;

                        RelicTracker.AddAmount(relic, "Cards Added", count);
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
