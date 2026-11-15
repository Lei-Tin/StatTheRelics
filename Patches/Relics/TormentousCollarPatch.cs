using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(TormentousCollar), nameof(TormentousCollar.BeforeSideTurnStart))]
    public static class TormentousCollarPatch {
        static void Prefix(TormentousCollar __instance, IReadOnlyList<Creature> participants, ICombatState combatState, out int __state) {
            __state = 0;
            try {
                if (__instance.Owner.RunState.CurrentRoom?.RoomType != RoomType.Elite) return;
                if (!participants.Contains(__instance.Owner.Creature) || (__instance.Owner.PlayerCombatState?.TurnNumber ?? int.MaxValue) > 1) return;
                var weak = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "WeakPower"));
                __state = weak * combatState.HittableEnemies.Count();
            } catch { }
        }

        static void Postfix(TormentousCollar __instance, Task __result, int __state) {
            try {
                if (__state <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Weak Applied", __state);
                    } catch { }
                });
            } catch { }
        }
    }
}
