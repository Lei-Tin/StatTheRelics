using System.Globalization;
using System.Reflection;
using System.Text.Json;

internal static class Localization {
    internal const string FileName = "localization.json";

    static IReadOnlyDictionary<string, string> translations = new Dictionary<string, string>(StringComparer.Ordinal);

    public static void Load() {
        try {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var baseDirectory = string.IsNullOrWhiteSpace(assemblyDirectory) ? AppContext.BaseDirectory : assemblyDirectory;
            var path = Path.Combine(baseDirectory, FileName);

            if (!File.Exists(path)) {
                translations = new Dictionary<string, string>(StringComparer.Ordinal);
                ModLog.Info($"Localization: {FileName} was not found; using English defaults");
                return;
            }

            var options = new JsonSerializerOptions {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path), options)
                ?? new Dictionary<string, string>();

            translations = loaded
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

            ModLog.Info($"Localization: loaded {translations.Count} entries from {path}");
        } catch (Exception ex) {
            translations = new Dictionary<string, string>(StringComparer.Ordinal);
            ModLog.Exception("Localization: failed to load localization file; using English defaults", ex);
        }
    }

    public static string Get(string englishText) {
        if (string.IsNullOrEmpty(englishText)) return englishText ?? string.Empty;
        return translations.TryGetValue(englishText, out var translated) && !string.IsNullOrWhiteSpace(translated)
            ? translated
            : englishText;
    }

    public static string Format(string englishFormat, params object?[] args) {
        var format = Get(englishFormat);
        try {
            return string.Format(CultureInfo.CurrentCulture, format, args);
        } catch (FormatException) {
            return string.Format(CultureInfo.CurrentCulture, englishFormat, args);
        }
    }

    public static string TranslateTooltip(string text) {
        if (string.IsNullOrEmpty(text) || translations.Count == 0) return text ?? string.Empty;

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');
        for (var index = 0; index < lines.Length; index++) {
            lines[index] = TranslateLine(lines[index]);
        }
        return string.Join("\n", lines);
    }

    static string TranslateLine(string line) {
        if (translations.TryGetValue(line, out var exact) && !string.IsNullOrWhiteSpace(exact)) return exact;

        var separatorIndex = line.IndexOf(':');
        if (separatorIndex > 0) {
            var label = line[..separatorIndex];
            if (translations.TryGetValue(label, out var translated) && !string.IsNullOrWhiteSpace(translated)) {
                return translated + line[separatorIndex..];
            }
        }

        return line;
    }
}
