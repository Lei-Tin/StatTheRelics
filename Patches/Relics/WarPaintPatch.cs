using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(WarPaint), nameof(WarPaint.AfterObtained))]
    public static class WarPaintPatch {
        readonly struct UpgradeSnapshot {
            public UpgradeSnapshot(bool isUpgraded, int level) {
                IsUpgraded = isUpgraded;
                Level = level;
            }

            public bool IsUpgraded { get; }
            public int Level { get; }
        }

        sealed class State {
            public Dictionary<int, UpgradeSnapshot> Cards { get; } = new();
            public Dictionary<int, string> Names { get; } = new();
        }

        static void Prefix(WarPaint __instance, ref object __state) {
            try {
                if (__instance == null) return;
                var state = CaptureDeck(__instance);
                if (state.Cards.Count > 0) __state = state;
            } catch { }
        }

        static void Postfix(WarPaint __instance, object __state) {
            try {
                if (__state is not State state) return;
                var upgraded = CountUpgraded(__instance, state);
                if (upgraded > 0) RelicTracker.AddAmount(__instance, "Cards Upgraded", upgraded);
            } catch { }
        }

        static State CaptureDeck(WarPaint relic) {
            var state = new State();
            foreach (var card in DeckUtil.EnumerateDeckCards(relic.Owner)) {
                var key = RuntimeHelpers.GetHashCode(card);
                state.Cards[key] = GetUpgradeSnapshot(card);
                state.Names[key] = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
            }

            return state;
        }

        static int CountUpgraded(WarPaint relic, State state) {
            var count = 0;
            var names = new List<string>();
            foreach (var card in DeckUtil.EnumerateDeckCards(relic.Owner)) {
                var key = RuntimeHelpers.GetHashCode(card);
                if (!state.Cards.TryGetValue(key, out var before)) continue;

                var after = GetUpgradeSnapshot(card);
                if (after.Level <= before.Level && (!after.IsUpgraded || before.IsUpgraded)) continue;

                count++;
                if (state.Names.TryGetValue(key, out var name) && !string.IsNullOrWhiteSpace(name)) names.Add(name);
            }

            if (names.Count > 0) RelicTracker.SetText(relic, "Cards Upgraded", DeckUtil.JoinCardList(names));
            return count;
        }

        static UpgradeSnapshot GetUpgradeSnapshot(object card) {
            try {
                var level = Math.Max(0, ReflectionUtil.GetIntMemberValue(card, "CurrentUpgradeLevel", 0));
                var upgraded = ReflectionUtil.GetMemberValue(card, "IsUpgraded") is bool isUpgraded
                    ? isUpgraded
                    : level > 0;
                return new UpgradeSnapshot(upgraded, level);
            } catch {
                return new UpgradeSnapshot(false, 0);
            }
        }
    }
}
