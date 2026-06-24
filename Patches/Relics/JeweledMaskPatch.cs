using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(JeweledMask), nameof(JeweledMask.BeforeHandDraw))]
    public static class JeweledMaskPatch {
        static readonly object Sync = new();
        static JeweledMask? activeRelic;

        internal static JeweledMask? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        static void Prefix(JeweledMask __instance, Player player, PlayerChoiceContext choiceContext, ICombatState combatState) {
            try {
                if (__instance == null || player != __instance.Owner) return;
                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(JeweledMask __instance, Task __result) {
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

        static void Clear(JeweledMask relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static void CountAdded(Task<CardPileAddResult> task, CardModel card) {
            try {
                var relic = ActiveRelic;
                if (relic == null || card == null) return;

                if (task == null) {
                    Count(relic, card);
                    return;
                }

                task.ContinueWith(t => {
                    try {
                        if (t.Status != TaskStatus.RanToCompletion || !WasAdded(t.Result)) return;
                        Count(relic, card);
                    } catch { }
                });
            } catch { }
        }

        static void Count(JeweledMask relic, CardModel card) {
            RelicTracker.AddAmount(relic, "Free Cards Added", 1);
        }

        static bool WasAdded(CardPileAddResult result) {
            try {
                var success = ReflectionUtil.GetMemberValue(result, "success");
                return success is bool value && value;
            } catch {
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Add), new Type[] {
        typeof(CardModel),
        typeof(PileType),
        typeof(CardPilePosition),
        typeof(AbstractModel),
        typeof(bool)
    })]
    public static class JeweledMaskCardPileAddPatch {
        static void Postfix(CardModel card, Task<CardPileAddResult> __result) {
            JeweledMaskPatch.CountAdded(__result, card);
        }
    }
}
