using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MegaCrit.Sts2.Core.Models;

namespace StatTheRelics {
    public static class DeckUtil {
        const string CardListFormat = "card-list-v1";
        static readonly JsonSerializerOptions cardJsonOptions = new() { PropertyNameCaseInsensitive = true };

        sealed class StoredCardReference {
            public string TypeName { get; set; } = string.Empty;
            public int UpgradeLevel { get; set; }
            public string? Group { get; set; }
            public int? OccurrenceIndex { get; set; }
        }

        sealed class StoredCardList {
            public string Format { get; set; } = CardListFormat;
            public List<StoredCardReference> Cards { get; set; } = new();
            public List<string> LegacyEntries { get; set; } = new();
        }

        public static Dictionary<string, int> CaptureDeckHistogramFromRelicOwner(object relic, bool preferBaseTitle = false) {
            var owner = ReflectionUtil.GetMemberValue(relic, "Owner");
            return CaptureDeckHistogramFromOwner(owner, preferBaseTitle);
        }

        public static Dictionary<string, int> CaptureDeckHistogramFromOwner(object? owner, bool preferBaseTitle = false) {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var card in EnumerateDeckCards(owner)) {
                var key = GetCardStorageValue(card);
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
            if (!string.IsNullOrWhiteSpace(title)) return AddUpgradeSuffix(card, title);
            return card.GetType().Name;
        }

        public static string GetCardMatchName(object card) {
            var title = ReflectionUtil.GetCardBaseTitle(card)
                ?? ReflectionUtil.GetCardTitle(card)
                ?? card.GetType().Name;
            return NormalizeCardNameForMatching(title);
        }

        public static string GetCardCodeName(object card) {
            return card?.GetType().FullName ?? string.Empty;
        }

        public static string GetCardStorageValue(object card, string? group = null, int? occurrenceIndex = null) {
            var reference = CreateStoredCardReference(card, occurrenceIndex);
            reference.Group = group;
            return JsonSerializer.Serialize(reference, cardJsonOptions);
        }

        public static string NormalizeCardNameForMatching(string? cardName) {
            var normalized = (cardName ?? string.Empty).Trim();
            while (normalized.EndsWith("+", StringComparison.Ordinal)) {
                normalized = normalized.Substring(0, normalized.Length - 1).TrimEnd();
            }
            return normalized;
        }

        static string AddUpgradeSuffix(object card, string title) {
            try {
                var name = (title ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(name)) return name;
                if (name.EndsWith("+", StringComparison.Ordinal)) return name;

                var isUpgraded = ReflectionUtil.GetMemberValue(card, "IsUpgraded");
                if (isUpgraded is not bool upgraded || !upgraded) return name;

                var upgradeLevel = Math.Max(1, ReflectionUtil.GetIntMemberValue(card, "CurrentUpgradeLevel", 1));
                return name + new string('+', upgradeLevel);
            } catch {
                return title;
            }
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
            var payload = new StoredCardList();
            foreach (var value in cards) {
                if (TryDeserializeCardReference(value, out var card)) payload.Cards.Add(card);
                else if (!string.IsNullOrWhiteSpace(value)) payload.LegacyEntries.Add(value);
            }
            return SerializeCardList(payload);
        }

        public static string SingleCardList(object card) {
            return JoinCardList(new[] { GetCardStorageValue(card) });
        }

        public static string AppendCardList(string? current, object card, int? occurrenceIndex = null) {
            var payload = DeserializeCardList(current);
            payload.Cards.Add(CreateStoredCardReference(card, occurrenceIndex));
            return SerializeCardList(payload);
        }

        public static string AppendCardStorageValue(string? current, string storedCard) {
            var payload = DeserializeCardList(current);
            if (TryDeserializeCardReference(storedCard, out var card)) payload.Cards.Add(card);
            else if (!string.IsNullOrWhiteSpace(storedCard)) payload.LegacyEntries.Add(storedCard);
            return SerializeCardList(payload);
        }

        public static string FormatStoredCardList(string value) {
            if (!TryDeserializeCardList(value, out var payload)) return value;

            var entries = new List<string>(payload.LegacyEntries.Where(entry => !string.IsNullOrWhiteSpace(entry)));
            string? currentGroup = null;
            foreach (var card in payload.Cards) {
                if (!string.Equals(currentGroup, card.Group, StringComparison.Ordinal)) {
                    if (!string.IsNullOrWhiteSpace(card.Group)) entries.Add(card.Group + ":");
                    currentGroup = card.Group;
                }
                entries.Add(GetLocalizedCardName(card));
            }
            return string.Join("\n", entries);
        }

        public static Dictionary<string, string> FormatStoredCardTextStats(IReadOnlyDictionary<string, string> textStats) {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in textStats) result[pair.Key] = FormatStoredCardList(pair.Value);
            return result;
        }

        public static bool StoredCardListContains(string? value, object card, bool matchOccurrence = false) {
            if (card == null || string.IsNullOrWhiteSpace(value)) return false;
            var codeName = GetCardCodeName(card);
            var matchName = GetCardMatchName(card);

            if (TryDeserializeCardList(value, out var payload)) {
                var matchingCards = payload.Cards
                    .Where(entry => string.Equals(entry.TypeName, codeName, StringComparison.Ordinal))
                    .ToList();
                if (matchingCards.Count > 0) {
                    if (matchOccurrence && matchingCards.Any(entry => entry.OccurrenceIndex.HasValue)) {
                        var occurrence = GetCardTypeOccurrence(card);
                        return occurrence.HasValue && matchingCards.Any(entry => entry.OccurrenceIndex == occurrence);
                    }
                    return true;
                }
                return payload.LegacyEntries.Any(entry => LegacyCardNameMatches(entry, matchName));
            }

            return value
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(entry => LegacyCardNameMatches(entry, matchName));
        }

        static StoredCardReference CreateStoredCardReference(object card, int? occurrenceIndex = null) {
            var level = Math.Max(0, ReflectionUtil.GetIntMemberValue(card, "CurrentUpgradeLevel", 0));
            if (level == 0 && ReflectionUtil.GetMemberValue(card, "IsUpgraded") is bool upgraded && upgraded) level = 1;
            return new StoredCardReference {
                TypeName = GetCardCodeName(card),
                UpgradeLevel = level,
                OccurrenceIndex = occurrenceIndex
            };
        }

        static int? GetCardTypeOccurrence(object card) {
            try {
                var target = ReflectionUtil.GetMemberValue(card, "DeckVersion") ?? card;
                var owner = ReflectionUtil.GetMemberValue(card, "Owner");
                var typeName = GetCardCodeName(target);
                var occurrence = 0;
                foreach (var deckCard in EnumerateDeckCards(owner)) {
                    if (!string.Equals(GetCardCodeName(deckCard), typeName, StringComparison.Ordinal)) continue;
                    if (ReferenceEquals(deckCard, target)) return occurrence;
                    occurrence++;
                }
            } catch { }
            return null;
        }

        static string SerializeCardList(StoredCardList payload) {
            payload.Format = CardListFormat;
            return JsonSerializer.Serialize(payload, cardJsonOptions);
        }

        static StoredCardList DeserializeCardList(string? value) {
            if (TryDeserializeCardList(value, out var payload)) return payload;

            var legacy = new StoredCardList();
            if (!string.IsNullOrWhiteSpace(value)) {
                legacy.LegacyEntries.AddRange(value
                    .Replace("\r\n", "\n")
                    .Replace('\r', '\n')
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            }
            return legacy;
        }

        static bool TryDeserializeCardList(string? value, out StoredCardList payload) {
            payload = new StoredCardList();
            if (string.IsNullOrWhiteSpace(value) || value[0] != '{') return false;
            try {
                var parsed = JsonSerializer.Deserialize<StoredCardList>(value, cardJsonOptions);
                if (parsed == null || !string.Equals(parsed.Format, CardListFormat, StringComparison.Ordinal)) return false;
                parsed.Cards ??= new List<StoredCardReference>();
                parsed.LegacyEntries ??= new List<string>();
                payload = parsed;
                return true;
            } catch {
                return false;
            }
        }

        static bool TryDeserializeCardReference(string? value, out StoredCardReference card) {
            card = new StoredCardReference();
            if (string.IsNullOrWhiteSpace(value) || value[0] != '{') return false;
            try {
                var parsed = JsonSerializer.Deserialize<StoredCardReference>(value, cardJsonOptions);
                if (parsed == null || string.IsNullOrWhiteSpace(parsed.TypeName)) return false;
                card = parsed;
                return true;
            } catch {
                return false;
            }
        }

        static string GetLocalizedCardName(StoredCardReference card) {
            var fallback = ShortTypeName(card.TypeName);
            try {
                var type = Type.GetType(card.TypeName + ", sts2", throwOnError: false)
                    ?? AppDomain.CurrentDomain.GetAssemblies()
                        .Select(assembly => assembly.GetType(card.TypeName, throwOnError: false))
                        .FirstOrDefault(found => found != null);
                if (type == null || !typeof(CardModel).IsAssignableFrom(type)) return AddStoredUpgradeSuffix(fallback, card.UpgradeLevel);

                var model = ModelDb.AllCards.FirstOrDefault(card => card.GetType() == type);
                var title = model?.Title;
                return AddStoredUpgradeSuffix(string.IsNullOrWhiteSpace(title) ? fallback : title, card.UpgradeLevel);
            } catch {
                return AddStoredUpgradeSuffix(fallback, card.UpgradeLevel);
            }
        }

        static string ShortTypeName(string typeName) {
            if (string.IsNullOrWhiteSpace(typeName)) return "Unknown";
            var separator = Math.Max(typeName.LastIndexOf('.'), typeName.LastIndexOf('+'));
            return separator >= 0 && separator < typeName.Length - 1 ? typeName[(separator + 1)..] : typeName;
        }

        static string AddStoredUpgradeSuffix(string title, int upgradeLevel) {
            var normalized = NormalizeCardNameForMatching(title);
            return normalized + new string('+', Math.Max(0, upgradeLevel));
        }

        static bool LegacyCardNameMatches(string value, string matchName) {
            return string.Equals(NormalizeCardNameForMatching(value), matchName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
