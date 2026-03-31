using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;
using StatTheRelics.RelicStats;

namespace StatTheRelics.Patches {
    // Detect run history open/close via the submenu stack to reliably suspend and restore live stats.
    [HarmonyPatch(typeof(NSubmenuStack))]
    internal static class SubmenuStackHistoryPatch {
        [HarmonyPatch("Push")]
        [HarmonyPostfix]
        static void AfterPush(NSubmenu screen) {
            try {
                if (screen is NRunHistory) {
                    ModLog.Info("SubmenuStackHistoryPatch: RunHistory pushed");
                    RelicStatsPersistence.EnterHistoryView("submenu-stack-push");
                }
            } catch { }
        }

        [HarmonyPatch("Pop")]
        [HarmonyPrefix]
        static void BeforePop(NSubmenuStack __instance, ref NSubmenu? __state) {
            try { __state = __instance.Peek(); } catch { __state = null; }
        }

        [HarmonyPatch("Pop")]
        [HarmonyPostfix]
        static void AfterPop(NSubmenu? __state) {
            try {
                if (__state is NRunHistory) {
                    ModLog.Info("SubmenuStackHistoryPatch: RunHistory popped");
                    RelicStatsPersistence.RestoreSuspendedRunSnapshotIfAny();
                    RelicStatsPersistence.ForceExitHistoryView("submenu-stack-pop");
                }
            } catch { }
        }
    }
}
