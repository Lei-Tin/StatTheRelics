using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch]
    public static class JossPaperDrawPatch {
        static readonly object Sync = new();
        static JossPaper? activeRelic;

        internal static JossPaper? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        static MethodBase TargetMethod() {
            return AccessTools.DeclaredMethod(typeof(JossPaper), "DrawIfThresholdMet");
        }

        static void Prefix(JossPaper __instance, PlayerChoiceContext choiceContext) {
            try {
                var threshold = Math.Max(1, ReflectionUtil.GetDynamicVarIntValue(__instance, "ExhaustAmount", 5));
                if (ReflectionUtil.GetIntMemberValue(__instance, "CardsExhausted", 0) < threshold) return;
                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(JossPaper __instance, Task __result) {
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

        static void Clear(JossPaper relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static void CountDrawn(Task<System.Collections.Generic.IEnumerable<CardModel>> task, Player player) {
            try {
                var relic = ActiveRelic;
                if (relic == null || player != relic.Owner || task == null) return;

                task.ContinueWith(t => {
                    try {
                        if (t.Status != TaskStatus.RanToCompletion || t.Result == null) return;
                        var drawn = t.Result.Count();
                        if (drawn > 0) RelicTracker.AddAmount(relic, "Cards Drawn", drawn);
                    } catch { }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Draw), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(decimal),
        typeof(Player),
        typeof(bool)
    })]
    public static class JossPaperCardPileDrawPatch {
        static void Postfix(Player player, Task<System.Collections.Generic.IEnumerable<CardModel>> __result) {
            JossPaperDrawPatch.CountDrawn(__result, player);
        }
    }
}
