using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace StatTheRelics.Patches;

// Hook the actual run creation, not log lines, to start a new counter session.
[HarmonyPatch(typeof(Player), nameof(Player.CreateForNewRun), new System.Type[] { typeof(CharacterModel), typeof(MegaCrit.Sts2.Core.Unlocks.UnlockState), typeof(ulong) })]
public static class RunStartPatch {
    static void Postfix(Player __result) {
        try {
            if (RelicTracker.IsHistoryStack()) return;
            RelicTracker.StartNewRunSession("CreateForNewRun");
        } catch { }
    }
}
