using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(FishingRod), nameof(FishingRod.AfterCombatEnd))]
    public static class FishingRodPatch {
        class CombatState {
            public bool NormalCombat { get; set; }
            public bool ShouldTrigger { get; set; }
            public List<CardState> Cards { get; } = new();
        }

        class CardState {
            public object Card { get; set; } = null!;
            public bool WasUpgraded { get; set; }
        }

        static void Prefix(FishingRod __instance, CombatRoom room, ref object __state) {
            try {
                if (__instance == null || room == null) return;
                var normalCombat = Convert.ToInt32(room.Encounter.RoomType) == 1;
                if (!normalCombat) return;

                var threshold = Math.Max(1, ReflectionUtil.GetDynamicVarIntValue(__instance, "Combats", 3));
                var shouldTrigger = (__instance.CombatsSeen + 1) % threshold == 0;
                var state = new CombatState {
                    NormalCombat = true,
                    ShouldTrigger = shouldTrigger
                };

                if (shouldTrigger) {
                    state.Cards.AddRange(CaptureCardStates(__instance));
                }

                __state = state;
            } catch { }
        }

        static void Postfix(FishingRod __instance, object __state) {
            try {
                var state = __state as CombatState;
                if (state == null || !state.NormalCombat) return;

                RelicTracker.AddAmount(__instance, "Normal Combats", 1);
                if (!state.ShouldTrigger) return;

                var upgraded = FindNewlyUpgradedCards(state.Cards);
                if (upgraded <= 0) return;

                RelicTracker.AddAmount(__instance, "Cards Upgraded", upgraded);
            } catch { }
        }

        static IEnumerable<CardState> CaptureCardStates(FishingRod relic) {
            foreach (var card in DeckUtil.EnumerateDeckCards(relic.Owner)) {
                yield return new CardState {
                    Card = card,
                    WasUpgraded = IsUpgraded(card)
                };
            }
        }

        static int FindNewlyUpgradedCards(IEnumerable<CardState> before) {
            return before
                .Where(c => !c.WasUpgraded && IsUpgraded(c.Card))
                .Count();
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
