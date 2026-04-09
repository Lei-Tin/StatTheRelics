using System;
using System.Collections;
using System.Collections.Generic;

namespace StatTheRelics {
    public static class DeckUtil {
        public static Dictionary<string, int> CaptureDeckHistogramFromRelicOwner(object relic, bool preferBaseTitle = false) {
            var owner = ReflectionUtil.GetMemberValue(relic, "Owner");
            return CaptureDeckHistogramFromOwner(owner, preferBaseTitle);
        }

        public static Dictionary<string, int> CaptureDeckHistogramFromOwner(object? owner, bool preferBaseTitle = false) {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var card in EnumerateDeckCards(owner)) {
                var key = GetCardDisplayName(card, preferBaseTitle);
                if (string.IsNullOrWhiteSpace(key)) continue;
                result[key] = result.TryGetValue(key, out var v) ? v + 1 : 1;
            }
            return result;
        }

        public static IEnumerable<object> EnumerateDeckCards(object? owner) {
            var deck = ReflectionUtil.GetMemberValue(owner, "Deck");
            if (deck == null) yield break;

            var cardsContainer = ReflectionUtil.GetMemberValue(deck, "Cards") ?? deck;
            if (cardsContainer is not IEnumerable enumerable) yield break;

            foreach (var card in enumerable) {
                if (card != null) yield return card;
            }
        }

        public static string GetCardDisplayName(object card, bool preferBaseTitle = false) {
            var title = preferBaseTitle
                ? ReflectionUtil.GetCardBaseTitle(card) ?? ReflectionUtil.GetCardTitle(card)
                : ReflectionUtil.GetCardTitle(card) ?? ReflectionUtil.GetCardBaseTitle(card);
            if (!string.IsNullOrWhiteSpace(title)) return title;
            return card.GetType().Name;
        }

        public static List<string> FindAddedCards(IReadOnlyDictionary<string, int> before, IReadOnlyDictionary<string, int> after) {
            var obtained = new List<string>();
            foreach (var kv in after) {
                var beforeVal = before.TryGetValue(kv.Key, out var b) ? b : 0;
                var delta = kv.Value - beforeVal;
                for (var i = 0; i < delta; i++) obtained.Add(kv.Key);
            }
            obtained.Sort(StringComparer.OrdinalIgnoreCase);
            return obtained;
        }

        public static List<string> FindRemovedCards(IReadOnlyDictionary<string, int> before, IReadOnlyDictionary<string, int> after) {
            var removed = new List<string>();
            foreach (var kv in before) {
                var afterVal = after.TryGetValue(kv.Key, out var a) ? a : 0;
                var delta = kv.Value - afterVal;
                for (var i = 0; i < delta; i++) removed.Add(kv.Key);
            }
            removed.Sort(StringComparer.OrdinalIgnoreCase);
            return removed;
        }

        public static Dictionary<string, int> PositiveDelta(IReadOnlyDictionary<string, int> before, IReadOnlyDictionary<string, int> after) {
            var delta = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var kv in after) {
                var beforeVal = before.TryGetValue(kv.Key, out var b) ? b : 0;
                var add = kv.Value - beforeVal;
                if (add > 0) delta[kv.Key] = add;
            }
            return delta;
        }

        public static string JoinCardList(IReadOnlyList<string> cards) {
            if (cards == null || cards.Count == 0) return string.Empty;
            return string.Join("\n", cards);
        }
    }
}