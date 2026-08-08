using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MegaCrit.Sts2.Core.Models;

namespace StatTheRelics {
    public static class RelicNameUtil {
        const string RelicListFormat = "relic-list-v1";
        static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        sealed class StoredRelicList {
            public string Format { get; set; } = RelicListFormat;
            public List<string> TypeNames { get; set; } = new();
            public List<string> LegacyEntries { get; set; } = new();
        }

        public static string AppendTypeNames(string? current, IEnumerable<string> typeNames) {
            var payload = Deserialize(current);
            foreach (var typeName in typeNames) {
                if (!string.IsNullOrWhiteSpace(typeName)) payload.TypeNames.Add(typeName);
            }
            return Serialize(payload);
        }

        public static string FormatStoredRelicList(string value) {
            if (!TryDeserialize(value, out var payload)) return value;
            var names = new List<string>(payload.LegacyEntries.Where(entry => !string.IsNullOrWhiteSpace(entry)));
            names.AddRange(payload.TypeNames.Select(GetLocalizedRelicName));
            return string.Join("\n", names);
        }

        static StoredRelicList Deserialize(string? value) {
            if (TryDeserialize(value, out var payload)) return payload;

            var legacy = new StoredRelicList();
            if (!string.IsNullOrWhiteSpace(value)) {
                legacy.LegacyEntries.AddRange(value
                    .Replace("\r\n", "\n")
                    .Replace('\r', '\n')
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            }
            return legacy;
        }

        static bool TryDeserialize(string? value, out StoredRelicList payload) {
            payload = new StoredRelicList();
            if (string.IsNullOrWhiteSpace(value) || value[0] != '{') return false;
            try {
                var parsed = JsonSerializer.Deserialize<StoredRelicList>(value, JsonOptions);
                if (parsed == null || !string.Equals(parsed.Format, RelicListFormat, StringComparison.Ordinal)) return false;
                parsed.TypeNames ??= new List<string>();
                parsed.LegacyEntries ??= new List<string>();
                payload = parsed;
                return true;
            } catch {
                return false;
            }
        }

        static string Serialize(StoredRelicList payload) {
            payload.Format = RelicListFormat;
            return JsonSerializer.Serialize(payload, JsonOptions);
        }

        static string GetLocalizedRelicName(string typeName) {
            var fallback = ShortTypeName(typeName);
            try {
                var type = Type.GetType(typeName + ", sts2", throwOnError: false)
                    ?? AppDomain.CurrentDomain.GetAssemblies()
                        .Select(assembly => assembly.GetType(typeName, throwOnError: false))
                        .FirstOrDefault(found => found != null);
                if (type == null) return fallback;

                var model = ModelDb.AllRelics.FirstOrDefault(relic => relic.GetType() == type);
                return ReflectionUtil.GetModelTitle(model) ?? fallback;
            } catch {
                return fallback;
            }
        }

        static string ShortTypeName(string typeName) {
            if (string.IsNullOrWhiteSpace(typeName)) return "Unknown";
            var separator = Math.Max(typeName.LastIndexOf('.'), typeName.LastIndexOf('+'));
            return separator >= 0 && separator < typeName.Length - 1 ? typeName[(separator + 1)..] : typeName;
        }
    }
}
