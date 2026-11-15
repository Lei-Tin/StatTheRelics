using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(BathRobes), nameof(BathRobes.BeforeCombatStart))]
    public static class BathRobesPatch {
        static void Prefix(BathRobes __instance, out int __state) {
            __state = -1;
            try {
                if (__instance.WasUsed || __instance.Owner.Creature.IsDead || __instance.Owner.RunState.CurrentRoom?.RoomType != RoomType.Boss) return;
                __state = __instance.Owner.Creature.CurrentHp;
            } catch { }
        }

        static void Postfix(BathRobes __instance, Task __result, int __state) {
            try {
                if (__state < 0 || __result == null) return;
                var creature = __instance.Owner.Creature;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        var healed = Math.Max(0, creature.CurrentHp - __state);
                        if (healed > 0) RelicTracker.AddAmount(__instance, "HP Healed", healed);
                    } catch { }
                });
            } catch { }
        }
    }
}
