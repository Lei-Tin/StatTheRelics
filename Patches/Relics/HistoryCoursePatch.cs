using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(HistoryCourse), nameof(HistoryCourse.AfterAutoPrePlayPhaseEntered))]
    public static class HistoryCoursePatch {
        static readonly object Sync = new();
        static HistoryCourse? activeRelic;

        internal static HistoryCourse? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        static void Prefix(HistoryCourse __instance, PlayerChoiceContext choiceContext, Player player) {
            try {
                if (__instance == null || player != __instance.Owner) return;
                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(HistoryCourse __instance, Task __result) {
            try {
                if (__result == null) {
                    Clear(__instance);
                    return;
                }

                __result.ContinueWith(_ => {
                    try { Clear(__instance); } catch { }
                });
            } catch { }
        }

        static void Clear(HistoryCourse relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static void CountAutoPlayed(Task task, CardModel card) {
            try {
                var relic = ActiveRelic;
                if (relic == null || card == null) return;

                if (task == null) {
                    Count(relic, card);
                    return;
                }

                task.ContinueWith(t => {
                    try {
                        if (t.Status == TaskStatus.RanToCompletion) Count(relic, card);
                    } catch { }
                });
            } catch { }
        }

        static void Count(HistoryCourse relic, CardModel card) {
            RelicTracker.AddAmount(relic, "Cards Auto Played", 1);
        }
    }

    [HarmonyPatch(typeof(CardCmd), nameof(CardCmd.AutoPlay), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(CardModel),
        typeof(Creature),
        typeof(AutoPlayType),
        typeof(bool),
        typeof(bool)
    })]
    public static class HistoryCourseAutoPlayPatch {
        static void Postfix(CardModel card, Task __result) {
            HistoryCoursePatch.CountAutoPlayed(__result, card);
        }
    }
}
