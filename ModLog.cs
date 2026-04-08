using MegaCrit.Sts2.Core.Logging;

internal static class ModLog {
    const string Prefix = "[StatTheRelics]: ";
    public static string RelicStatsHeader { get; set; } = "[purple][StatTheRelics][/purple]";

    public static void Info(string message) {
        Log.Info($"{Prefix}{message ?? string.Empty}");
    }
}
