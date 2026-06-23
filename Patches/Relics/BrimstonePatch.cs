using System;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    // Count Strength granted to the player and to living enemies by Brimstone.
    [HarmonyPatch(typeof(Brimstone), nameof(Brimstone.AfterSideTurnStart))]
    public static class BrimstonePatch {
        class BrimstoneState {
            public int SelfStrength { get; set; }
            public int EnemyStrengthTotal { get; set; }
        }

        static void Prefix(Brimstone __instance, CombatSide side, CombatState combatState, ref object __state) {
            try {
                var ownerCreature = __instance?.Owner?.Creature;
                if (__instance == null || ownerCreature == null || side != ownerCreature.Side) return;

                var selfStrength = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "SelfStrength"));
                var enemyStrength = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "EnemyStrength"));
                var livingEnemies = combatState?.GetOpponentsOf(ownerCreature)?.Count(c => c != null && c.IsAlive) ?? 0;

                __state = new BrimstoneState {
                    SelfStrength = selfStrength,
                    EnemyStrengthTotal = enemyStrength * livingEnemies
                };

                ModLog.Info($"BrimstonePatch: Prefix self={selfStrength}, enemyEach={enemyStrength}, livingEnemies={livingEnemies}");
            } catch { }
        }

        static void Postfix(Brimstone __instance, Task __result, object __state) {
            try {
                var state = __state as BrimstoneState;
                if (state == null) return;

                if (__result == null) {
                    AddStrength(__instance, state);
                    return;
                }

                __result.ContinueWith(_ => AddStrength(__instance, state));
            } catch { }
        }

        static void AddStrength(Brimstone relic, BrimstoneState state) {
            try {
                if (state.SelfStrength > 0) RelicTracker.AddAmount(relic, "Strength Gained", state.SelfStrength);
                if (state.EnemyStrengthTotal > 0) RelicTracker.AddAmount(relic, "Enemy Strength Given", state.EnemyStrengthTotal);
                ModLog.Info($"BrimstonePatch: self={state.SelfStrength}, enemyTotal={state.EnemyStrengthTotal}");
            } catch { }
        }
    }
}
