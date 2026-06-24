using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterRoomEntered), new[] { typeof(AbstractRoom) })]
    public static class JuzuBraceletPatch {
        static void Postfix(AbstractModel __instance, AbstractRoom room) {
            try {
                if (__instance is not JuzuBracelet relic || room == null) return;
                if (room.RoomType != RoomType.Event) return;
                RelicTracker.AddAmount(relic, "Question Marks Visited", 1);
            } catch { }
        }
    }
}
