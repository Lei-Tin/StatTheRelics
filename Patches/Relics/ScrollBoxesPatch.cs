using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ScrollBoxes), nameof(ScrollBoxes.AfterObtained))]
    public static class ScrollBoxesPatch {
        static readonly object Sync = new();
        static ScrollBoxes? activeRelic;

        static void Prefix(ScrollBoxes __instance) {
            try {
                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(ScrollBoxes __instance, Task __result) {
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

        static void Clear(ScrollBoxes relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static ScrollBoxes? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        internal static void SetOfferedCards(ScrollBoxes relic, IReadOnlyList<IReadOnlyList<CardModel>> bundles) {
            try {
                var names = new List<string>();
                for (var i = 0; i < bundles.Count; i++) {
                    var bundle = bundles[i];
                    if (bundle == null) continue;

                    names.Add($"Box {i + 1}:");
                    foreach (var card in bundle) {
                        var name = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
                        if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
                    }

                    if (i + 1 < bundles.Count) names.Add(string.Empty);
                }

                if (names.Count > 0) RelicTracker.SetText(relic, "Cards Offered", DeckUtil.JoinCardList(names));
            } catch { }
        }

        internal static void SetAddedCards(ScrollBoxes relic, IEnumerable<CardModel> cards) {
            try {
                var added = cards
                    .Select(card => DeckUtil.GetCardDisplayName(card, preferBaseTitle: true))
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList();
                if (added.Count > 0) RelicTracker.SetText(relic, "Cards Added", DeckUtil.JoinCardList(added));
            } catch { }
        }
    }

    [HarmonyPatch(typeof(ScrollBoxes), nameof(ScrollBoxes.GenerateRandomBundles))]
    public static class ScrollBoxesGenerateRandomBundlesPatch {
        static void Postfix(Player player, List<IReadOnlyList<CardModel>> __result) {
            try {
                var relic = ScrollBoxesPatch.ActiveRelic;
                if (relic == null || player == null || relic.Owner != player || __result == null) return;
                ScrollBoxesPatch.SetOfferedCards(relic, __result);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromChooseABundleScreen), new Type[] {
        typeof(Player),
        typeof(IReadOnlyList<IReadOnlyList<CardModel>>)
    })]
    public static class ScrollBoxesChooseBundlePatch {
        static void Postfix(Player player, Task<IEnumerable<CardModel>> __result) {
            try {
                var relic = ScrollBoxesPatch.ActiveRelic;
                if (relic == null || player == null || relic.Owner != player || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
                        ScrollBoxesPatch.SetAddedCards(relic, task.Result);
                    } catch { }
                });
            } catch { }
        }
    }
}
