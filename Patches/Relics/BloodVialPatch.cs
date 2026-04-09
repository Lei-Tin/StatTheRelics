using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Capture actual healing done by Blood Vial at start of combat.
    [HarmonyPatch(typeof(BloodVial), nameof(BloodVial.AfterPlayerTurnStartLate))]
    public static class BloodVialPatch {
        class HpState { public object? Creature { get; set; } public int Before { get; set; } }

        static void Prefix(BloodVial __instance, PlayerChoiceContext choiceContext, Player player, ref object __state) {
            try {
                var creature = __instance?.Owner?.Creature;
                var beforeHp = GetHp(creature);
                __state = new HpState { Creature = creature, Before = beforeHp };
                ModLog.Info($"BloodVialPatch: Prefix creature={creature?.GetType().FullName ?? "null"}, beforeHp={beforeHp}");
            } catch { }
        }

        static void Postfix(BloodVial __instance, Task __result, object __state) {
            try {
                if (__result == null) {
                    FinalizeHealing(__instance, __state as HpState);
                    return;
                }

                __result.ContinueWith(_ => {
                    FinalizeHealing(__instance, __state as HpState);
                });
            } catch { }
        }

        static void FinalizeHealing(BloodVial relic, HpState? state) {
            try {
                var creature = state?.Creature ?? relic?.Owner?.Creature;
                var beforeHp = state?.Before ?? GetHp(creature);
                var afterHp = GetHp(creature);
                var healed = Math.Max(0, afterHp - beforeHp);
                ModLog.Info($"BloodVialPatch: Postfix creature={creature?.GetType().FullName ?? "null"}, beforeHp={beforeHp}, afterHp={afterHp}, healed={healed}");

                if (relic != null && healed > 0) {
                    RelicTracker.AddAmount(relic, "HP Healed", healed);
                } else if (relic != null) {
                    ModLog.Info("BloodVialPatch: no positive healing this call");
                }
            } catch { }
        }

        static int GetHp(object? creature) {
            try {
                if (creature == null) return 0;
                var currentHp = ReflectionUtil.GetMemberValue(creature, "CurrentHp");
                ModLog.Info($"BloodVialPatch: HP via member CurrentHp -> value={currentHp}");
                if (currentHp != null) return Convert.ToInt32(currentHp);
            } catch {
                ModLog.Info("BloodVialPatch: failed to get HP via property");
            }
            return 0;
        }
    }
}