using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(FurCoat), nameof(FurCoat.BeforeCombatStart))]
    public static class FurCoatPatch {
        class CombatState {
            public List<CreatureState> Creatures { get; } = new();
        }

        class CreatureState {
            public Creature Creature { get; set; } = null!;
            public int BeforeHp { get; set; }
        }

        static void Prefix(FurCoat __instance, ref object __state) {
            try {
                if (!IsCurrentRoomMarked(__instance)) return;
                var state = new CombatState();
                foreach (var enemy in GetHittableEnemies(__instance)) {
                    if (enemy == null || enemy.CurrentHp <= 1) continue;
                    state.Creatures.Add(new CreatureState { Creature = enemy, BeforeHp = enemy.CurrentHp });
                }

                if (state.Creatures.Count > 0) __state = state;
            } catch { }
        }

        static void Postfix(FurCoat __instance, Task __result, object __state) {
            try {
                var state = __state as CombatState;
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

        internal static bool IsCurrentRoomMarked(FurCoat relic) {
            try {
                var coords = relic.GetMarkedCoords();
                if (coords == null) return false;
                var current = relic.Owner?.RunState?.CurrentMapPoint;
                if (current == null) return false;
                return coords.Contains(current.coord);
            } catch {
                return false;
            }
        }

        static void Count(FurCoat relic, CombatState state) {
            try {
                var enemies = 0;
                foreach (var creatureState in state.Creatures) {
                    var removed = Math.Max(0, creatureState.BeforeHp - creatureState.Creature.CurrentHp);
                    if (removed <= 0) continue;
                    enemies++;
                }

                if (enemies <= 0) return;
                RelicTracker.AddAmount(relic, "Free Combats Visited", 1);
            } catch { }
        }

        static IEnumerable<Creature> GetHittableEnemies(FurCoat relic) {
            try {
                var enemies = relic.Owner?.Creature?.CombatState?.HittableEnemies;
                if (enemies != null) return enemies;
            } catch { }

            return Array.Empty<Creature>();
        }
    }

}
