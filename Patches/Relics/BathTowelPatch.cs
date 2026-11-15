using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(BathTowel), nameof(BathTowel.AfterPlayerTurnStartLate))]
    public static class BathTowelPatch {
        static void Prefix(BathTowel __instance, Player player, out int __state) {
            __state = -1;
            try {
                if (player != __instance.Owner || (__instance.Owner.PlayerCombatState?.TurnNumber ?? int.MaxValue) > 1) return;
                __state = __instance.Owner.Creature.CurrentHp;
            } catch { }
        }

        static void Postfix(BathTowel __instance, Task __result, int __state) {
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
