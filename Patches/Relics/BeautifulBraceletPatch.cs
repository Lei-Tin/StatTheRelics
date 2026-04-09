using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    static class BeautifulBraceletSwiftTracker {
        internal const string BeautifulBraceletTypeName = "MegaCrit.Sts2.Core.Models.Relics.BeautifulBracelet";
        const string TrackedSwiftCardsDisplayKey = "Swift Cards Enchanted";

        static readonly Dictionary<string, int> beforeSwift3 = new(StringComparer.Ordinal);

        public static void CaptureBefore(BeautifulBracelet relic) {
            try {
                var owner = ReflectionUtil.GetMemberValue(relic, "Owner");
                if (owner == null) return;

                beforeSwift3.Clear();
                foreach (var kv in CaptureSwift3Histogram(owner)) beforeSwift3[kv.Key] = kv.Value;
            } catch { }
        }

        public static void CaptureAfter(BeautifulBracelet relic) {
            try {
                var owner = ReflectionUtil.GetMemberValue(relic, "Owner");
                if (owner == null) return;

                var after = CaptureSwift3Histogram(owner);
                var added = DeckUtil.PositiveDelta(beforeSwift3, after);
                var mergedTracked = LoadTrackedHistogram(relic);
                MergeInPlace(mergedTracked, added);

                RelicTracker.SetText(relic, TrackedSwiftCardsDisplayKey, SerializeCardList(mergedTracked));

                ModLog.Info($"BeautifulBraceletSwiftTracker: tracked total Swift(3) cards={CountTotal(mergedTracked)}, newlyAdded={CountTotal(added)}");
            } catch { }
        }

        public static bool TryCountTrackedSwift3CardPlay(CardModel card) {
            try {
                if (card == null) return false;
                if (!IsSwift3AndActive(card)) return false;

                var tracked = LoadTrackedHistogram();
                if (tracked.Count == 0) return false;

                var cardName = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
                if (string.IsNullOrWhiteSpace(cardName)) return false;

                var isTracked = tracked.TryGetValue(cardName, out var n) && n > 0;
                if (!isTracked) return false;

                RelicTracker.AddAmountByType(BeautifulBraceletTypeName, "Swift Cards Played", 1);
                return true;
            } catch {
                return false;
            }
        }

        static Dictionary<string, int> CaptureSwift3Histogram(object owner) {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var card in DeckUtil.EnumerateDeckCards(owner)) {
                if (!IsSwift3(card)) continue;
                var name = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
                if (string.IsNullOrWhiteSpace(name)) continue;
                result[name] = result.TryGetValue(name, out var v) ? v + 1 : 1;
            }
            return result;
        }

        static bool IsSwift3(object card) {
            var enchantment = ReflectionUtil.GetMemberValue(card, "Enchantment");
            var enchantmentType = enchantment?.GetType();
            if (enchantmentType == null || !string.Equals(enchantmentType.Name, "Swift", StringComparison.Ordinal)) return false;

            try {
                var amountRaw = ReflectionUtil.GetMemberValue(enchantment, "Amount");
                if (amountRaw == null) return false;
                var amount = Convert.ToDecimal(amountRaw);
                return amount == 3m;
            } catch {
                return false;
            }
        }

        static bool IsSwift3AndActive(object card) {
            var enchantment = ReflectionUtil.GetMemberValue(card, "Enchantment");
            if (enchantment == null) return false;
            if (!IsSwift3(card)) return false;

            try {
                var status = ReflectionUtil.GetMemberValue(enchantment, "Status");
                return status is EnchantmentStatus enchantmentStatus && enchantmentStatus == EnchantmentStatus.Normal;
            } catch {
                return false;
            }
        }

        static void MergeInPlace(Dictionary<string, int> merged, Dictionary<string, int> added) {
            foreach (var kv in added) {
                merged[kv.Key] = merged.TryGetValue(kv.Key, out var existing) ? existing + kv.Value : kv.Value;
            }
        }

        static Dictionary<string, int> LoadTrackedHistogram(BeautifulBracelet relic) {
            var raw = RelicTracker.GetText(relic, TrackedSwiftCardsDisplayKey);
            return ParseCardList(raw);
        }

        static Dictionary<string, int> LoadTrackedHistogram() {
            var raw = RelicTracker.GetTextByType(BeautifulBraceletTypeName, TrackedSwiftCardsDisplayKey);
            return ParseCardList(raw);
        }

        static string SerializeCardList(Dictionary<string, int> histogram) {
            if (histogram == null || histogram.Count == 0) return "None";
            var lines = new List<string>();
            foreach (var kv in histogram.Where(kv => kv.Value > 0).OrderBy(kv => kv.Key, StringComparer.Ordinal)) {
                for (var i = 0; i < kv.Value; i++) lines.Add(kv.Key);
            }
            return lines.Count == 0 ? "None" : string.Join("\n", lines);
        }

        static Dictionary<string, int> ParseCardList(string? raw) {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            if (string.IsNullOrWhiteSpace(raw)) return result;

            var lines = raw.Replace("\r", string.Empty).Split('\n');
            foreach (var line in lines) {
                var name = (line ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(name) || string.Equals(name, "None", StringComparison.OrdinalIgnoreCase)) continue;
                result[name] = result.TryGetValue(name, out var current) ? current + 1 : 1;
            }

            return result;
        }

        static int CountTotal(Dictionary<string, int> histogram) {
            var total = 0;
            foreach (var kv in histogram) total += kv.Value;
            return total;
        }
    }

    // Track which cards received Swift(3) from Beautiful Bracelet at pickup time.
    [HarmonyPatch(typeof(BeautifulBracelet), nameof(BeautifulBracelet.AfterObtained))]
    public static class BeautifulBraceletPatch {
        static void Prefix(BeautifulBracelet __instance) {
            try {
                if (__instance == null) return;
                BeautifulBraceletSwiftTracker.CaptureBefore(__instance);
            } catch { }
        }

        static void Postfix(BeautifulBracelet __instance, Task __result) {
            try {
                if (__instance == null) return;

                if (__result == null) {
                    BeautifulBraceletSwiftTracker.CaptureAfter(__instance);
                    return;
                }

                __result.ContinueWith(_ => {
                    BeautifulBraceletSwiftTracker.CaptureAfter(__instance);
                });
            } catch { }
        }
    }

    // Global card-play hook: count only tracked Swift(3) cards from Beautiful Bracelet.
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class BeautifulBraceletCardPlayPatch {
        static void Postfix(CardModel __instance) {
            try {
                if (__instance == null) return;
                if (!RelicTracker.HasTrackedRelicType(BeautifulBraceletSwiftTracker.BeautifulBraceletTypeName)) return;
                var counted = BeautifulBraceletSwiftTracker.TryCountTrackedSwift3CardPlay(__instance);
                if (!counted) return;
                ModLog.Info($"BeautifulBraceletCardPlayPatch: counted play for card={__instance.GetType().FullName}");
            } catch { }
        }
    }
}