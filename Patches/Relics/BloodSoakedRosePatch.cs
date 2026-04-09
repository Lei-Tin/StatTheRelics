using HarmonyLib;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Track energy granted by Blood Soaked Rose when turn-start logic runs for its owner.
    [HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterPlayerTurnStart), new[] { typeof(PlayerChoiceContext), typeof(Player) })]
    public static class BloodSoakedRosePatch {
        static void Postfix(AbstractModel __instance, PlayerChoiceContext choiceContext, Player player) {
            try {
                if (__instance is not BloodSoakedRose bloodSoakedRose || player == null) return;
                if (bloodSoakedRose.Owner != player) return;

                RelicTracker.AddAmount(bloodSoakedRose, "Energy Given", 1);

                ModLog.Info($"BloodSoakedRosePatch: owner turn start counted for player={player.GetType().FullName}");

                // TODO: Also add tracking for the card generated (enthralled), how many times it is played
            } catch { }
        }
    }
}