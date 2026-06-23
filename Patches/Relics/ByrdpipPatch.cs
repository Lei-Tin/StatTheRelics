using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    // Count how many times the Byrd Swoop card is actually played while Byrdpip is owned.
    [HarmonyPatch(typeof(ByrdSwoop), "OnPlay")]
    public static class ByrdpipPatch {
        static void Postfix(ByrdSwoop __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, Task __result) {
            try {
                if (__instance == null) return;
                var relic = ReflectionUtil.FindRelic<Byrdpip>(__instance.Owner);
                if (relic == null) return;

                if (__result == null) {
                    CountPlay(relic);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) CountPlay(relic);
                });
            } catch { }
        }

        static void CountPlay(Byrdpip relic) {
            try {
                RelicTracker.AddAmount(relic, "Byrd Swoops Played", 1);
                ModLog.Info("ByrdpipPatch: counted Byrd Swoop play");
            } catch { }
        }
    }
}
