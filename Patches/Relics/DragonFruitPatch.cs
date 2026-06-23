using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(DragonFruit), nameof(DragonFruit.AfterGoldGained))]
    public static class DragonFruitPatch {
        class DragonFruitState {
            public int MaxHp { get; set; }
        }

        static void Prefix(DragonFruit __instance, Player player, ref object __state) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                __state = new DragonFruitState {
                    MaxHp = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "MaxHp", 1))
                };
            } catch { }
        }

        static void Postfix(DragonFruit __instance, Task __result, object __state) {
            try {
                var state = __state as DragonFruitState;
                if (state == null || state.MaxHp <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Max HP Gained", state.MaxHp);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            RelicTracker.AddAmount(__instance, "Max HP Gained", state.MaxHp);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
