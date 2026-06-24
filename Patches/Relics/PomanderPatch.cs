using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Pomander), nameof(Pomander.AfterObtained))]
    public static class PomanderPatch {
        class State {
            public List<CardState> Cards { get; } = new();
        }

        class CardState {
            public object Card { get; set; } = null!;
            public string Name { get; set; } = string.Empty;
            public bool WasUpgraded { get; set; }
        }

        static void Prefix(Pomander __instance, ref object __state) {
            try {
                var state = new State();
                state.Cards.AddRange(CaptureCardStates(__instance));
                __state = state;
            } catch { }
        }

        static void Postfix(Pomander __instance, Task __result, object __state) {
            try {
                if (__state is not State state) return;
                if (__result == null) {
                    Count(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, state);
                    } catch { }
                });
            } catch { }
        }

        static void Count(Pomander relic, State state) {
            try {
                var upgraded = state.Cards
                    .Where(c => !c.WasUpgraded && IsUpgraded(c.Card))
                    .Select(c => c.Name)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList();

                if (upgraded.Count > 0) RelicTracker.SetText(relic, "Cards Upgraded", DeckUtil.JoinCardList(upgraded));
            } catch { }
        }

        static IEnumerable<CardState> CaptureCardStates(Pomander relic) {
            foreach (var card in DeckUtil.EnumerateDeckCards(relic.Owner)) {
                yield return new CardState {
                    Card = card,
                    Name = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true),
                    WasUpgraded = IsUpgraded(card)
                };
            }
        }

        static bool IsUpgraded(object card) {
            try {
                var raw = ReflectionUtil.GetMemberValue(card, "IsUpgraded");
                return raw is bool value && value;
            } catch {
                return false;
            }
        }
    }
}
