using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(SereTalon), nameof(SereTalon.AfterObtained))]
    public static class SereTalonPatch {
        internal sealed class ActiveState {
            public SereTalon Relic { get; }
            public int CurseCount { get; }
            public int CardsSeen { get; set; }

            public ActiveState(SereTalon relic) {
                Relic = relic;
                CurseCount = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(relic, "Curses", 2));
            }
        }

        static readonly object Sync = new();
        static ActiveState? active;

        static void Prefix(SereTalon __instance) {
            try {
                lock (Sync) active = new ActiveState(__instance);
            } catch { }
        }

        static void Postfix(SereTalon __instance, Task __result) {
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

        static void Clear(SereTalon relic) {
            lock (Sync) {
                if (ReferenceEquals(active?.Relic, relic)) active = null;
            }
        }

        internal static ActiveState? Active {
            get {
                lock (Sync) return active;
            }
        }

        internal static string ReserveKey(ActiveState state) {
            lock (Sync) {
                var key = state.CardsSeen < state.CurseCount ? "Curses Added" : "Wishes Added";
                state.CardsSeen++;
                return key;
            }
        }

        internal static void CountCard(SereTalon relic, string key, CardModel card) {
            try {
                if (relic == null || card == null || string.IsNullOrWhiteSpace(key)) return;

                var name = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
                if (string.IsNullOrWhiteSpace(name)) return;
                AppendText(relic, key, name);
            } catch { }
        }

        static void AppendText(SereTalon relic, string key, string value) {
            lock (Sync) {
                var current = RelicTracker.GetText(relic, key);
                RelicTracker.SetText(relic, key, string.IsNullOrWhiteSpace(current) ? value : current + "\n" + value);
            }
        }

        internal static void CountPlayed(CardModel card) {
            try {
                if (card == null) return;
                const string typeName = "MegaCrit.Sts2.Core.Models.Relics.SereTalon";
                if (!RelicTracker.HasTrackedRelicType(typeName)) return;

                var tracked = ParseCardList(RelicTracker.GetTextByType(typeName, "Wishes Added"));
                if (tracked.Count == 0) return;

                var cardName = DeckUtil.GetCardMatchName(card);
                if (string.IsNullOrWhiteSpace(cardName)) return;
                if (!tracked.Contains(cardName)) return;

                RelicTracker.AddAmountByType(typeName, "Wishes Played", 1);
            } catch { }
        }

        static HashSet<string> ParseCardList(string? raw) {
            var result = new HashSet<string>(StringComparer.Ordinal);
            if (string.IsNullOrWhiteSpace(raw)) return result;

            var lines = raw.Replace("\r", string.Empty).Split('\n');
            foreach (var line in lines) {
                var name = DeckUtil.NormalizeCardNameForMatching(line);
                if (!string.IsNullOrWhiteSpace(name) && !string.Equals(name, "None", StringComparison.OrdinalIgnoreCase)) result.Add(name);
            }

            return result;
        }
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Add), new Type[] {
        typeof(CardModel),
        typeof(PileType),
        typeof(CardPilePosition),
        typeof(AbstractModel),
        typeof(bool)
    })]
    public static class SereTalonCardPileAddPatch {
        sealed class AddState {
            public SereTalon Relic { get; set; } = null!;
            public string Key { get; set; } = string.Empty;
        }

        static void Prefix(ref object __state) {
            try {
                var state = SereTalonPatch.Active;
                if (state == null) return;
                __state = new AddState {
                    Relic = state.Relic,
                    Key = SereTalonPatch.ReserveKey(state)
                };
            } catch { }
        }

        static void Postfix(CardModel card, Task<CardPileAddResult> __result, object __state) {
            try {
                if (__state is not AddState state || card == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || !WasAdded(task.Result)) return;
                        var added = ReflectionUtil.GetMemberValue(task.Result, "cardAdded") as CardModel ?? card;
                        SereTalonPatch.CountCard(state.Relic, state.Key, added);
                    } catch { }
                });
            } catch { }
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

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class SereTalonCardPlayPatch {
        static void Postfix(CardModel __instance) {
            SereTalonPatch.CountPlayed(__instance);
        }
    }
}
