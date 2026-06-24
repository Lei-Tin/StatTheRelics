using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(WingedBoots), nameof(WingedBoots.AfterRoomEntered))]
    public static class WingedBootsPatch {
        const string TypeName = "MegaCrit.Sts2.Core.Models.Relics.WingedBoots";

        sealed class State {
            public int TimesUsed { get; set; }
        }

        static void Prefix(WingedBoots __instance, AbstractRoom room, ref object __state) {
            try {
                _ = room;
                if (__instance == null) return;
                __state = new State { TimesUsed = __instance.TimesUsed };
            } catch { }
        }

        static void Postfix(WingedBoots __instance, object __state) {
            try {
                if (__state is not State state || __instance == null) return;
                var used = __instance.TimesUsed - state.TimesUsed;
                if (used > 0) RelicTracker.AddAmountByType(TypeName, "Free Travel Used", used);
            } catch { }
        }
    }
}
