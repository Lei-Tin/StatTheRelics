using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace StatTheRelics.Patches;

// Initialize one tracking session for the whole run. Player.CreateForNewRun is
// called once per player and is also reused by Run History.
[HarmonyPatch(typeof(RunManager), "InitializeNewRun")]
public static class RunStartPatch {
    static void Prefix() {
        try {
            RelicTracker.StartNewRunSession("InitializeNewRun");
        } catch { }
    }

    static void Postfix(RunManager __instance) {
        try {
            var state = ReflectionUtil.GetMemberValue(__instance, "State");
            if (ReflectionUtil.GetMemberValue(state, "Players") is not IEnumerable<Player> players) return;
            foreach (var player in players) {
                if (player?.Relics == null) continue;
                foreach (var relic in player.Relics) RelicTracker.GetOrCreate(relic);
            }
        } catch { }
    }
}
