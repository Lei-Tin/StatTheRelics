using MegaCrit.Sts2.Core.Localization;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

internal static class Localization {
    internal const string DirectoryName = "localization";
    internal const string DefaultLanguageCode = "eng";
    internal const string ArchivedStatsFormat = "Archived stats (mod {0}, game {1}; current mod {2}, game {3})";
    internal const string VersionMismatchFormat = "StatTheRelics data was saved by mod version {0}, but the current mod version is {1}. No relic stats are available for this save.";

    const string ArchivedStatsPrefix = "Archived stats (mod ";
    const string ArchivedGameSeparator = ", game ";
    const string ArchivedCurrentModSeparator = "; current mod ";
    const string ArchivedStatsSuffix = ")";
    const string VersionMismatchPrefix = "StatTheRelics data was saved by mod version ";
    const string VersionMismatchSeparator = ", but the current mod version is ";
    const string VersionMismatchSuffix = ". No relic stats are available for this save.";

    static readonly object sync = new();
    static IReadOnlyDictionary<string, string> translations = new Dictionary<string, string>(StringComparer.Ordinal);
    static string localizationDirectory = string.Empty;
    static volatile string activeLanguageCode = string.Empty;

    public static void Load() {
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var baseDirectory = string.IsNullOrWhiteSpace(assemblyDirectory) ? AppContext.BaseDirectory : assemblyDirectory;
        localizationDirectory = Path.Combine(baseDirectory, DirectoryName);
        EnsureCurrentLanguage(force: true);
    }

    public static string Get(string englishText) {
        EnsureCurrentLanguage();
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
        EnsureCurrentLanguage();
        if (string.IsNullOrEmpty(text) || translations.Count == 0) return text ?? string.Empty;

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var lines = normalized.Split('\n');
        for (var index = 0; index < lines.Length; index++) {
            lines[index] = TranslateLine(lines[index]);
        }
        return string.Join("\n", lines);
    }

    static void EnsureCurrentLanguage(bool force = false) {
        var languageCode = GetCurrentLanguageCode();
        if (!force && string.Equals(languageCode, activeLanguageCode, StringComparison.Ordinal)) return;

        lock (sync) {
            if (!force && string.Equals(languageCode, activeLanguageCode, StringComparison.Ordinal)) return;
            LoadLanguage(languageCode);
        }
    }

    static string GetCurrentLanguageCode() {
        try {
            return NormalizeLanguageCode(LocManager.Instance?.Language);
        } catch {
            return DefaultLanguageCode;
        }
    }

    static string NormalizeLanguageCode(string? languageCode) {
        if (string.IsNullOrWhiteSpace(languageCode)) return DefaultLanguageCode;

        var normalized = new string(languageCode.Trim()
            .Where(character => char.IsLetterOrDigit(character) || character is '-' or '_')
            .ToArray())
            .ToLowerInvariant();
        return string.IsNullOrEmpty(normalized) ? DefaultLanguageCode : normalized;
    }

    static void LoadLanguage(string languageCode) {
        var requestedPath = Path.Combine(localizationDirectory, languageCode + ".json");
        if (TryLoadFile(requestedPath, out var requestedTranslations, out var requestedError)) {
            translations = requestedTranslations;
            activeLanguageCode = languageCode;
            ModLog.Info($"Localization: loaded {translations.Count} entries for '{languageCode}' from {requestedPath}");
            return;
        }

        if (requestedError != null) {
            ModLog.Exception($"Localization: failed to load language '{languageCode}' from {requestedPath}", requestedError);
        }

        var fallbackPath = Path.Combine(localizationDirectory, DefaultLanguageCode + ".json");
        if (!string.Equals(languageCode, DefaultLanguageCode, StringComparison.Ordinal)) {
            if (TryLoadFile(fallbackPath, out var fallbackTranslations, out var fallbackError)) {
                translations = fallbackTranslations;
                activeLanguageCode = languageCode;
                ModLog.Info($"Localization: '{languageCode}' was unavailable; loaded English fallback from {fallbackPath}");
                return;
            }
            if (fallbackError != null) {
                ModLog.Exception($"Localization: failed to load English fallback from {fallbackPath}", fallbackError);
            }
        }

        translations = new Dictionary<string, string>(StringComparer.Ordinal);
        activeLanguageCode = languageCode;
        ModLog.Info($"Localization: no usable file for '{languageCode}'; using built-in English text");
    }

    static bool TryLoadFile(
        string path,
        out IReadOnlyDictionary<string, string> loadedTranslations,
        out Exception? error
    ) {
        loadedTranslations = new Dictionary<string, string>(StringComparer.Ordinal);
        error = null;
        if (!File.Exists(path)) return false;

        try {
            var options = new JsonSerializerOptions {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path), options)
                ?? new Dictionary<string, string>();
            loadedTranslations = loaded
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            return true;
        } catch (Exception ex) {
            error = ex;
            return false;
        }
    }

    static string TranslateLine(string line) {
        if (translations.TryGetValue(line, out var exact) && !string.IsNullOrWhiteSpace(exact)) return exact;
        if (TryTranslateArchivedStats(line, out var archivedStats)) return archivedStats;
        if (TryTranslateVersionMismatch(line, out var versionMismatch)) return versionMismatch;

        var separatorIndex = line.IndexOf(':');
        if (separatorIndex > 0) {
            var label = line[..separatorIndex];
            if (translations.TryGetValue(label, out var translated) && !string.IsNullOrWhiteSpace(translated)) {
                return translated + line[separatorIndex..];
            }
        }

        return line;
    }

    static bool TryTranslateArchivedStats(string line, out string translated) {
        translated = string.Empty;
        if (!line.StartsWith(ArchivedStatsPrefix, StringComparison.Ordinal)
            || !line.EndsWith(ArchivedStatsSuffix, StringComparison.Ordinal)) {
            return false;
        }

        var values = line[ArchivedStatsPrefix.Length..^ArchivedStatsSuffix.Length];
        var savedGameIndex = values.IndexOf(ArchivedGameSeparator, StringComparison.Ordinal);
        if (savedGameIndex < 0) return false;

        var currentModIndex = values.IndexOf(
            ArchivedCurrentModSeparator,
            savedGameIndex + ArchivedGameSeparator.Length,
            StringComparison.Ordinal
        );
        if (currentModIndex < 0) return false;

        var currentGameIndex = values.IndexOf(
            ArchivedGameSeparator,
            currentModIndex + ArchivedCurrentModSeparator.Length,
            StringComparison.Ordinal
        );
        if (currentGameIndex < 0) return false;

        var savedMod = TranslateUnknown(values[..savedGameIndex]);
        var savedGame = TranslateUnknown(values[(savedGameIndex + ArchivedGameSeparator.Length)..currentModIndex]);
        var currentMod = TranslateUnknown(values[(currentModIndex + ArchivedCurrentModSeparator.Length)..currentGameIndex]);
        var currentGame = TranslateUnknown(values[(currentGameIndex + ArchivedGameSeparator.Length)..]);
        translated = Format(ArchivedStatsFormat, savedMod, savedGame, currentMod, currentGame);
        return true;
    }

    static bool TryTranslateVersionMismatch(string line, out string translated) {
        translated = string.Empty;
        if (!line.StartsWith(VersionMismatchPrefix, StringComparison.Ordinal)
            || !line.EndsWith(VersionMismatchSuffix, StringComparison.Ordinal)) {
            return false;
        }

        var values = line[VersionMismatchPrefix.Length..^VersionMismatchSuffix.Length];
        var separatorIndex = values.IndexOf(VersionMismatchSeparator, StringComparison.Ordinal);
        if (separatorIndex < 0) return false;

        var savedVersion = values[..separatorIndex];
        var currentVersion = values[(separatorIndex + VersionMismatchSeparator.Length)..];
        savedVersion = TranslateUnknown(savedVersion);
        translated = Format(VersionMismatchFormat, savedVersion, currentVersion);
        return true;
    }

    static string TranslateUnknown(string value) {
        return string.Equals(value, "Unknown", StringComparison.Ordinal) ? Get("Unknown") : value;
    }
}
