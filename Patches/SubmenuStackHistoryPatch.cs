using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;
using StatTheRelics.RelicStats;

namespace StatTheRelics.Patches {
    // Run History can be embedded inside Compendium, so it may be hidden without
    // being the submenu popped from the global stack.
    [HarmonyPatch(typeof(NRunHistory))]
    internal static class RunHistoryVisibilityPatch {
        [HarmonyPatch("OnSubmenuOpened")]
        [HarmonyPostfix]
        static void AfterOpened() {
            try { RelicStatsPersistence.EnterHistoryView("run-history-opened"); } catch { }
        }

        [HarmonyPatch("OnSubmenuHidden")]
        [HarmonyPostfix]
        static void AfterHidden() {
            try {
                RelicStatsPersistence.RestoreSuspendedRunSnapshotIfAny();
                RelicStatsPersistence.ForceExitHistoryView("run-history-hidden");
            } catch { }
        }
    }

    // Detect run history open/close via the submenu stack to reliably suspend and restore live stats.
    [HarmonyPatch(typeof(NSubmenuStack))]
    internal static class SubmenuStackHistoryPatch {
        [HarmonyPatch("Push")]
        [HarmonyPostfix]
        static void AfterPush(NSubmenu screen) {
            try {
                if (screen is NRunHistory) {
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
                    RelicStatsPersistence.RestoreSuspendedRunSnapshotIfAny();
                    RelicStatsPersistence.ForceExitHistoryView("submenu-stack-pop");
                }
            } catch { }
        }
    }
}
