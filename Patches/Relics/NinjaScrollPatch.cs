using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(NinjaScroll), nameof(NinjaScroll.BeforeHandDraw))]
    public static class NinjaScrollPatch {
        static void Prefix(NinjaScroll __instance, Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ref object __state) {
            try {
                _ = choiceContext;
                _ = combatState;
                if (__instance == null || player == null || __instance.Owner != player) return;
                if (player.PlayerCombatState?.TurnNumber > 1) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Shivs", 3));
            } catch { }
        }

        static void Postfix(NinjaScroll __instance, Task __result, object __state) {
            try {
                var amount = __state is int value ? value : 0;
                if (amount <= 0 || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Shivs Added", amount);
                    } catch { }
                });
            } catch { }
        }
    }
}
