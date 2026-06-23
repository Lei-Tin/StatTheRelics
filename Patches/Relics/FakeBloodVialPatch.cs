using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(FakeBloodVial), nameof(FakeBloodVial.AfterPlayerTurnStartLate))]
    public static class FakeBloodVialPatch {
        class HpState {
            public object? Creature { get; set; }
            public int Before { get; set; }
        }

        static void Prefix(FakeBloodVial __instance, PlayerChoiceContext choiceContext, Player player, ref object __state) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                if (player.PlayerCombatState == null || player.PlayerCombatState.TurnNumber > 1) return;
                var creature = __instance.Owner?.Creature;
                __state = new HpState { Creature = creature, Before = GetHp(creature) };
            } catch { }
        }

        static void Postfix(FakeBloodVial __instance, Task __result, object __state) {
            try {
                var state = __state as HpState;
                if (state == null) return;

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

        static void Count(FakeBloodVial relic, HpState state) {
            try {
                var creature = state.Creature ?? relic.Owner?.Creature;
                var healed = Math.Max(0, GetHp(creature) - state.Before);
                if (healed > 0) RelicTracker.AddAmount(relic, "HP Healed", healed);
            } catch { }
        }

        static int GetHp(object? creature) {
            try {
                var currentHp = ReflectionUtil.GetMemberValue(creature, "CurrentHp");
                return currentHp == null ? 0 : Convert.ToInt32(currentHp);
            } catch {
                return 0;
            }
        }
    }
}
