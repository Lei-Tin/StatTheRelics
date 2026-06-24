using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Sai), nameof(Sai.AfterSideTurnStart))]
    public static class SaiPatch {
        static void Prefix(Sai __instance, IReadOnlyList<Creature> participants, ref object __state) {
            try {
                if (__instance == null || participants == null || !participants.Contains(__instance.Owner?.Creature)) return;
                __state = ReflectionUtil.GetDynamicVarIntValue(__instance, "Block", 7);
            } catch { }
        }

        static void Postfix(Sai __instance, Task __result, object __state) {
            try {
                if (__state is not int block || block <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Block Gained", block);
                    } catch { }
                });
            } catch { }
        }
    }
}
