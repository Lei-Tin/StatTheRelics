using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(FuneraryMask), nameof(FuneraryMask.BeforeHandDraw))]
    public static class FuneraryMaskPatch {
        class MaskState {
            public int CardsGenerated { get; set; }
        }

        static void Prefix(FuneraryMask __instance, Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ref object __state) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                if (__instance.Owner?.PlayerCombatState?.TurnNumber != 1) return;
                __state = new MaskState {
                    CardsGenerated = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Cards", 3))
                };
            } catch { }
        }

        static void Postfix(FuneraryMask __instance, Task __result, object __state) {
            try {
                var state = __state as MaskState;
                if (state == null || state.CardsGenerated <= 0) return;

                if (__result == null) {
                    Count(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, state);
                    } catch { }
                });
            } catch { }
        }

        static void Count(FuneraryMask relic, MaskState state) {
            try {
                RelicTracker.AddAmount(relic, "Souls Added", state.CardsGenerated);
            } catch { }
        }
    }
}
