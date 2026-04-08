using System;
using System.Reflection;
using MegaCrit.Sts2.Core.Models;

namespace StatTheRelics {
    public static class ReflectionUtil {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static object? GetMemberValue(object? instance, string memberName) {
            if (instance == null || string.IsNullOrWhiteSpace(memberName)) return null;

            try {
                var type = instance.GetType();
                while (type != null) {
                    var prop = type.GetProperty(memberName, Flags);
                    if (prop != null) return prop.GetValue(instance);

                    var field = type.GetField(memberName, Flags);
                    if (field != null) return field.GetValue(instance);

                    type = type.BaseType;
                }
            } catch { }

            return null;
        }

        public static int GetIntMemberValue(object? instance, string memberName, int fallback = 0) {
            try {
                var raw = GetMemberValue(instance, memberName);
                if (raw == null) return fallback;
                return Convert.ToInt32(raw);
            } catch {
                return fallback;
            }
        }

        public static string? GetCardTitle(object? cardObject) {
            try {
                if (cardObject is not CardModel card) return null;
                var title = card.Title;
                return string.IsNullOrWhiteSpace(title) ? null : title;
            } catch {
                return null;
            }
        }

        public static string? GetCardBaseTitle(object? cardObject) {
            try {
                if (cardObject is not CardModel card) return null;

                // Prefer the localized base title because upgraded runtime title can append "+".
                var titleLocString = GetMemberValue(card, "TitleLocString");
                var fromLoc = GetLocStringText(titleLocString);
                if (!string.IsNullOrWhiteSpace(fromLoc)) return NormalizeCardTitle(fromLoc);

                var fallback = card.Title;
                return string.IsNullOrWhiteSpace(fallback) ? null : NormalizeCardTitle(fallback);
            } catch {
                return null;
            }
        }

        static string? GetLocStringText(object? locString) {
            try {
                if (locString == null) return null;

                var text = GetMemberValue(locString, "Text")
                    ?? GetMemberValue(locString, "Value")
                    ?? GetMemberValue(locString, "Localized");

                if (text is string s && !string.IsNullOrWhiteSpace(s)) return s;

                return null;
            } catch {
                return null;
            }
        }

        static string NormalizeCardTitle(string title) {
            var normalized = (title ?? string.Empty).Trim();
            while (normalized.EndsWith("+", StringComparison.Ordinal)) {
                normalized = normalized.Substring(0, normalized.Length - 1).TrimEnd();
            }
            return normalized;
        }
    }
}