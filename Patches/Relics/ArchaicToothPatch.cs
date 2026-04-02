using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Track the transformed card (output), not the pre-transform starter card.
    [HarmonyPatch(typeof(ArchaicTooth), "GetTranscendenceTransformedCard")]
    public static class ArchaicToothTransformedCardPatch {
        static void Postfix(ArchaicTooth __instance, object? starterCard, object? __result) {
            try {
                if (__instance == null) return;

                var transformedName = GetCardDisplayName(__result);
                if (!string.IsNullOrWhiteSpace(transformedName)) {
                    RelicTracker.SetText(__instance, "Card Obtained", transformedName);
                    var starterName = GetCardDisplayName(starterCard) ?? "null";
                    ModLog.Info($"ArchaicToothPatch: transformed '{starterName}' -> '{transformedName}'");
                } else {
                    ModLog.Info("ArchaicToothPatch: transformed card resolved as null/empty");
                }
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