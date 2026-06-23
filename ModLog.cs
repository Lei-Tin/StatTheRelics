using MegaCrit.Sts2.Core.Logging;
using System;

internal static class ModLog {
    const string Prefix = "[StatTheRelics]: ";
    public static string RelicStatsHeader { get; set; } = "[purple][StatTheRelics][/purple]";

    public static void Info(string message) {
        Log.Info($"{Prefix}{message ?? string.Empty}");
    }

    public static void Exception(string context, Exception ex) {
        Log.Info($"{Prefix}{context} failed - {ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}");
    }
}
