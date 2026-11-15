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
    [HarmonyPatch(typeof(Ocarina), nameof(Ocarina.BeforeHandDraw))]
    public static class OcarinaPatch {
        [ThreadStatic] internal static Ocarina? Current;

        static void Prefix(Ocarina __instance, Player player) {
            Current = null;
            if (player == __instance.Owner && player.PlayerCombatState?.TurnNumber is <= 1) Current = __instance;
        }

        static void Postfix() { Current = null; }
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.AddGeneratedCardsToCombat))]
    public static class OcarinaGeneratedCardsPatch {
        static void Postfix(Player creator, Task<IReadOnlyList<CardPileAddResult>> __result) {
            try {
                var relic = OcarinaPatch.Current;
                if (relic == null || creator != relic.Owner || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        foreach (var result in task.Result) {
                            if (!result.success || result.cardAdded == null) continue;
                            if (result.cardAdded.Tags.Contains(CardTag.Wah)) RelicTracker.AddAmount(relic, "Wah Cards Generated", 1);
                            if (result.cardAdded.Tags.Contains(CardTag.Cho)) RelicTracker.AddAmount(relic, "Cho Cards Generated", 1);
                            if (result.cardAdded.Tags.Contains(CardTag.Ki)) RelicTracker.AddAmount(relic, "Ki Cards Generated", 1);
                        }
                    } catch { }
                }, TaskContinuationOptions.ExecuteSynchronously);
            } catch { }
        }
    }
}
