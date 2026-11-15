using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(BizarreLamp), nameof(BizarreLamp.BeforeHandDraw))]
    public static class BizarreLampPatch {
        [ThreadStatic] internal static BizarreLamp? Current;

        static void Prefix(BizarreLamp __instance, Player player) {
            if (player == __instance.Owner && player.PlayerCombatState?.TurnNumber is <= 1) Current = __instance;
        }

        static void Postfix() { Current = null; }
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.AddGeneratedCardsToCombat))]
    public static class BizarreLampGeneratedCardsPatch {
        static void Postfix(Player creator, Task<IReadOnlyList<CardPileAddResult>> __result) {
            try {
                var relic = BizarreLampPatch.Current;
                if (relic == null || creator != relic.Owner || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        var count = task.Result.Count(result => result.success);
                        if (count > 0) RelicTracker.AddAmount(relic, "Cards Generated", count);
                    } catch { }
                });
            } catch { }
        }
    }
}
