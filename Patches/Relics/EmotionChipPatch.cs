using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(EmotionChip), nameof(EmotionChip.AfterPlayerTurnStart))]
    public static class EmotionChipPatch {
        class EmotionChipState {
            public int OrbCount { get; set; }
        }

        static void Prefix(EmotionChip __instance, PlayerChoiceContext choiceContext, Player player, ref object __state) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                var lostHp = ReflectionUtil.GetMemberValue(__instance, "LostHpInPreviousTurn");
                if (lostHp is not bool didLoseHp || !didLoseHp) return;

                __state = new EmotionChipState {
                    OrbCount = CountOrbs(player)
                };
            } catch { }
        }

        static void Postfix(EmotionChip __instance, Task __result, object __state) {
            try {
                var state = __state as EmotionChipState;
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

        static void Count(EmotionChip relic, EmotionChipState state) {
            try {
                if (state.OrbCount > 0) RelicTracker.AddAmount(relic, "Orb Passives Triggered", state.OrbCount);
            } catch { }
        }

        static int CountOrbs(Player player) {
            try {
                var orbs = player.PlayerCombatState?.OrbQueue?.Orbs;
                return orbs == null ? 0 : orbs.Count();
            } catch {
                return 0;
            }
        }
    }
}
