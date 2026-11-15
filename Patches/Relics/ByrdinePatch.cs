using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(GodByrd), "OnPlay")]
    public static class ByrdinePatch {
        static void Postfix(GodByrd __instance, Task __result) {
            try {
                var relic = ReflectionUtil.FindRelic<Byrdine>(__instance.Owner);
                if (relic == null || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(relic, "God Byrds Played", 1);
                    } catch { }
                });
            } catch { }
        }
    }
}
