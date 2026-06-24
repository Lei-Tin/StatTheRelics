using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ReptileTrinket), nameof(ReptileTrinket.AfterPotionUsed))]
    public static class ReptileTrinketPatch {
        class State {
            public int Strength { get; set; }
        }

        static void Prefix(ReptileTrinket __instance, PotionModel potion, Creature target, ref object __state) {
            try {
                if (__instance == null || potion == null || potion.Owner != __instance.Owner) return;
                __state = new State {
                    Strength = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Strength", 3))
                };
            } catch { }
        }

        static void Postfix(ReptileTrinket __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.Strength <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Strength Gained", state.Strength);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Strength Gained", state.Strength);
                    } catch { }
                });
            } catch { }
        }
    }
}
