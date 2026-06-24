using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PaelsEye), nameof(PaelsEye.BeforeSideTurnEndEarly))]
    public static class PaelsEyePatch {
        static readonly object Sync = new();
        static PaelsEye? activeRelic;

        static void Prefix(PaelsEye __instance, PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants) {
            try {
                _ = choiceContext;
                _ = side;
                if (__instance?.Owner?.Creature == null || participants == null) return;
                if (!participants.Contains(__instance.Owner.Creature)) return;
                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(PaelsEye __instance, Task __result) {
            try {
                if (__result == null) {
                    Clear(__instance);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        _ = task;
                        Clear(__instance);
                    } catch { }
                });
            } catch { }
        }

        static void Clear(PaelsEye relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static PaelsEye? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }
    }

    [HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Exhaust), new System.Type[] {
        typeof(PlayerChoiceContext),
        typeof(CardModel),
        typeof(bool),
        typeof(bool)
    })]
    public static class PaelsEyeExhaustPatch {
        static void Prefix(CardModel card, ref object __state) {
            try {
                var relic = PaelsEyePatch.ActiveRelic;
                if (relic == null || card == null || card.Owner != relic.Owner) return;
                __state = relic;
            } catch { }
        }

        static void Postfix(Task __result, object __state) {
            try {
                if (__state is not PaelsEye relic || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(relic, "Cards Exhausted", 1);
                    } catch { }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(PaelsEye), nameof(PaelsEye.AfterTakingExtraTurn))]
    public static class PaelsEyeExtraTurnPatch {
        static void Postfix(PaelsEye __instance, Player player, Task __result) {
            try {
                if (__instance == null || player == null || __instance.Owner != player || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Extra Turns Taken", 1);
                    } catch { }
                });
            } catch { }
        }
    }
}
