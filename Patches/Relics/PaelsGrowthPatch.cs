using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PaelsGrowth), nameof(PaelsGrowth.AfterObtained))]
    public static class PaelsGrowthPatch {
        static readonly object Sync = new();
        static PaelsGrowth? activeRelic;

        static void Prefix(PaelsGrowth __instance) {
            lock (Sync) activeRelic = __instance;
        }

        static void Postfix(PaelsGrowth __instance, Task __result) {
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

        static void Clear(PaelsGrowth relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static PaelsGrowth? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        internal static void CountCards(PaelsGrowth relic, IEnumerable<CardModel> cards) {
            try {
                if (relic == null || cards == null) return;
                var names = cards
                    .Select(card => DeckUtil.GetCardDisplayName(card, preferBaseTitle: true))
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList();
                if (names.Count > 0) RelicTracker.SetText(relic, "Cards Enchanted", DeckUtil.JoinCardList(names));
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromDeckForEnchantment), new Type[] {
        typeof(Player),
        typeof(EnchantmentModel),
        typeof(int),
        typeof(CardSelectorPrefs)
    })]
    public static class PaelsGrowthSelectionPatch {
        static void Postfix(Player player, Task<IEnumerable<CardModel>> __result) {
            try {
                var relic = PaelsGrowthPatch.ActiveRelic;
                if (relic == null || player == null || relic.Owner != player || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) PaelsGrowthPatch.CountCards(relic, task.Result);
                    } catch { }
                });
            } catch { }
        }
    }
}
