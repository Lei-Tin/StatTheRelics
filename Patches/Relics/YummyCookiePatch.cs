using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(YummyCookie), nameof(YummyCookie.AfterObtained))]
    public static class YummyCookiePatch {
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

        static void Prefix(YummyCookie __instance, ref object __state) {
            try {
                if (__instance == null) return;
                var state = CaptureDeck(__instance);
                if (state.Cards.Count > 0) __state = state;
            } catch { }
        }

        static void Postfix(YummyCookie __instance, Task __result, object __state) {
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

        static void Count(YummyCookie relic, State state) {
            try {
                var names = new List<string>();
                foreach (var card in DeckUtil.EnumerateDeckCards(relic.Owner)) {
                    var key = RuntimeHelpers.GetHashCode(card);
                    if (!state.Cards.TryGetValue(key, out var before)) continue;

                    var after = GetUpgradeSnapshot(card);
                    if (after.Level <= before.Level && (!after.IsUpgraded || before.IsUpgraded)) continue;
                    if (state.Names.TryGetValue(key, out var name) && !string.IsNullOrWhiteSpace(name)) names.Add(name);
                }

                if (names.Count <= 0) return;
                RelicTracker.AddAmount(relic, "Cards Upgraded", names.Count);
                RelicTracker.SetText(relic, "Cards Upgraded", DeckUtil.JoinCardList(names));
            } catch { }
        }

        static State CaptureDeck(YummyCookie relic) {
            var state = new State();
            foreach (var card in DeckUtil.EnumerateDeckCards(relic.Owner)) {
                var key = RuntimeHelpers.GetHashCode(card);
                state.Cards[key] = GetUpgradeSnapshot(card);
                state.Names[key] = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
            }

            return state;
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
