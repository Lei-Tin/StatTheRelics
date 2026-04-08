using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    // Archaic Tooth transforms a starter card; track both the consumed and resulting card names.
    [HarmonyPatch(typeof(ArchaicTooth), "GetTranscendenceTransformedCard")]
    public static class ArchaicToothPatch {
        static void Postfix(ArchaicTooth __instance, object? starterCard, object? __result) {
            try {
                if (__instance == null) return;

                var starterName = GetCardDisplayName(starterCard);
                var transformedName = GetCardDisplayName(__result);

                RelicTracker.SetText(__instance, "Cards Lost", string.IsNullOrWhiteSpace(starterName) ? "Unknown" : starterName);
                RelicTracker.SetText(__instance, "Cards Obtained", string.IsNullOrWhiteSpace(transformedName) ? "Unknown" : transformedName);

                ModLog.Info($"ArchaicToothPatch: transformed '{starterName ?? "Unknown"}' -> '{transformedName ?? "Unknown"}'");
            } catch { }
        }

        static string? GetCardDisplayName(object? card) {
            if (card == null) return null;
            var title = ReflectionUtil.GetCardTitle(card);
            if (!string.IsNullOrWhiteSpace(title)) return title;
            return card.GetType().Name;
        }
    }
}