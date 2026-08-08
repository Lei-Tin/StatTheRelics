using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace StatTheRelics.Patches;

// Seed numeric defaults as soon as a relic enters the run so sidecars persist
// zero values even when that relic never triggers.
[HarmonyPatch(typeof(RelicCmd), nameof(RelicCmd.Obtain), new Type[] {
    typeof(RelicModel),
    typeof(Player),
    typeof(int)
})]
internal static class RelicTrackerObtainPatch {
    static void Prefix(RelicModel relic) {
        try { RelicTracker.GetOrCreate(relic); } catch { }
    }
}
