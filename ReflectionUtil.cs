using System;
using System.Collections;
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

        public static object? GetDynamicVar(object? relic, string key) {
            try {
                if (relic == null || string.IsNullOrWhiteSpace(key)) return null;

                var dynamicVars = GetMemberValue(relic, "DynamicVars");
                if (dynamicVars == null) return null;

                var direct = GetMemberValue(dynamicVars, key);
                if (direct != null) return direct;

                if (dynamicVars is IDictionary dict && dict.Contains(key)) return dict[key];

                var type = dynamicVars.GetType();
                var indexer = type.GetProperty("Item", Flags, null, null, new[] { typeof(string) }, null);
                if (indexer != null) return indexer.GetValue(dynamicVars, new object[] { key });

                var getItem = type.GetMethod("get_Item", Flags, null, new[] { typeof(string) }, null);
                if (getItem != null) return getItem.Invoke(dynamicVars, new object[] { key });
            } catch { }

            return null;
        }

        public static int GetDynamicVarIntValue(object? relic, string key, int fallback = 0) {
            try {
                var dynamicVar = GetDynamicVar(relic, key);
                if (dynamicVar == null) return fallback;

                var raw = GetMemberValue(dynamicVar, "BaseValue")
                    ?? GetMemberValue(dynamicVar, "IntValue")
                    ?? GetMemberValue(dynamicVar, "Value");

                if (raw == null) return fallback;
                return Convert.ToInt32(raw);
            } catch {
                return fallback;
            }
        }

        public static T? FindRelic<T>(object? ownerOrCreature) where T : class {
            try {
                var relics = GetMemberValue(ownerOrCreature, "Relics");
                if (relics == null) {
                    var owner = GetMemberValue(ownerOrCreature, "Owner")
                        ?? GetMemberValue(ownerOrCreature, "Player");
                    relics = GetMemberValue(owner, "Relics");
                }

                if (relics is not IEnumerable enumerable) return null;
                foreach (var relic in enumerable) {
                    if (relic is T typed) return typed;
                }
            } catch { }

            return null;
        }

        public static string? GetModelTitle(object? model) {
            try {
                if (model == null) return null;

                var title = GetMemberValue(model, "Title");
                if (title is string s && !string.IsNullOrWhiteSpace(s)) return s;

                var loc = GetLocStringText(title);
                if (!string.IsNullOrWhiteSpace(loc)) return loc;

                var titleLocString = GetMemberValue(model, "TitleLocString");
                loc = GetLocStringText(titleLocString);
                if (!string.IsNullOrWhiteSpace(loc)) return loc;

                return model.GetType().Name;
            } catch {
                return null;
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
