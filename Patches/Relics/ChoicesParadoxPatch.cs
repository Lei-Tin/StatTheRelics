using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ChoicesParadox), nameof(ChoicesParadox.AfterPlayerTurnStart))]
    public static class ChoicesParadoxPatch {
        class ChoicesParadoxState {
            public int OfferedCards { get; set; }
        }

        static void Prefix(ChoicesParadox __instance, PlayerChoiceContext choiceContext, Player player, ref object __state) {
            try {
                if (__instance == null || player != __instance.Owner) return;
                if (player.PlayerCombatState == null || player.PlayerCombatState.TurnNumber != 1) return;

                __state = new ChoicesParadoxState {
                    OfferedCards = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Cards"))
                };
            } catch { }
        }

        static void Postfix(ChoicesParadox __instance, Task __result, object __state) {
            try {
                var state = __state as ChoicesParadoxState;
                if (state == null || state.OfferedCards <= 0) return;
                if (__result == null) {
                    AddStats(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) {
                        AddStats(__instance, state);
                    }
                });
            } catch { }
        }

        static void AddStats(ChoicesParadox relic, ChoicesParadoxState state) {
            RelicTracker.AddAmount(relic, "Cards Offered", state.OfferedCards);
            RelicTracker.AddAmount(relic, "Cards Added", 1);
        }
    }
}
