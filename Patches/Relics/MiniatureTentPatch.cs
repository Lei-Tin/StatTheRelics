using System;
using System.Collections;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(RestSiteRoom), nameof(RestSiteRoom.EnterInternal))]
    public static class MiniatureTentPatch {
        static void Postfix(IRunState runState, Task __result) {
            try {
                if (runState == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        CountRestSite(runState);
                    } catch { }
                });
            } catch { }
        }

        static void CountRestSite(IRunState runState) {
            try {
                var players = ReflectionUtil.GetMemberValue(runState, "Players") as IEnumerable;
                if (players == null) return;

                foreach (var player in players) {
                    var relic = ReflectionUtil.FindRelic<MiniatureTent>(player);
                    if (relic != null) RelicTracker.AddAmount(relic, "Rest Sites Visited", 1);
                }
            } catch { }
        }
    }
}
