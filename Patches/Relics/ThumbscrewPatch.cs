using System;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.Execute))]
    public static class ThumbscrewPatch {
        static void Prefix(AttackCommand __instance, out int __state) {
            __state = __instance.Results.Count();
        }

        static void Postfix(AttackCommand __instance, Task<AttackCommand> __result, int __state) {
            try {
                if (__instance.ModelSource is not CardModel card || card.Type != CardType.Attack || __result == null) return;
                var relic = ReflectionUtil.FindRelic<Thumbscrew>(card.Owner);
                if (relic == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        var baseHits = ReflectionUtil.GetIntMemberValue(__instance, "_hitCount", 1);
                        if (baseHits <= 1) return;
                        // Only actual strikes populate Results; UI hit-count queries never reach here.
                        var extraHits = Math.Clamp(__instance.Results.Skip(__state).Count(hit => hit.Count > 0) - baseHits, 0, 1);
                        if (extraHits > 0) RelicTracker.AddAmount(relic, "Extra Hits Executed", extraHits);
                    } catch { }
                }, TaskContinuationOptions.ExecuteSynchronously);
            } catch { }
        }
    }
}
