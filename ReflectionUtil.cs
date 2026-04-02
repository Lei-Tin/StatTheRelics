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
    }
}