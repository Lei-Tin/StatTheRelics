using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Add), new Type[] {
        typeof(CardModel),
        typeof(PileType),
        typeof(CardPilePosition),
        typeof(AbstractModel),
        typeof(bool)
    })]
    public static class BingBongPatch {
        static void Postfix(AbstractModel clonedBy, Task<CardPileAddResult> __result) {
            try {
                if (clonedBy is not BingBong relic) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        var success = ReflectionUtil.GetMemberValue(task.Result, "success");
                        if (success is bool ok && ok) RelicTracker.AddAmount(relic, "Cards Added", 1);
                    } catch { }
                });
            } catch { }
        }
    }
}
