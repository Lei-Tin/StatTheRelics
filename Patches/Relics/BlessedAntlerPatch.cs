using HarmonyLib;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Track energy granted by Blessed Antler when turn-start logic runs for its owner.
    [HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterPlayerTurnStart), new[] { typeof(PlayerChoiceContext), typeof(Player) })]
    public static class BlessedAntlerPatch {
        static void Postfix(AbstractModel __instance, PlayerChoiceContext choiceContext, Player player) {
            try {
                if (__instance is not BlessedAntler blessedAntler || player == null) return;
                if (blessedAntler.Owner != player) return;

                RelicTracker.AddAmount(blessedAntler, "Energy Given", 1);

                ModLog.Info($"BlessedAntlerPatch: owner turn start counted for player={player.GetType().FullName}");
            } catch { }
        }
    }
}