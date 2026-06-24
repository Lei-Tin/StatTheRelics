using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PaelsTears), nameof(PaelsTears.AfterSideTurnStart))]
    public static class PaelsTearsPatch {
        static void Prefix(PaelsTears __instance, CombatSide side, System.Collections.Generic.IReadOnlyList<Creature> participants, ICombatState combatState, ref object __state) {
            try {
                _ = side;
                _ = combatState;
                if (__instance?.Owner?.Creature == null || participants == null) return;
                if (!participants.Contains(__instance.Owner.Creature)) return;
                if (ReflectionUtil.GetMemberValue(__instance, "HadLeftoverEnergy") is not bool hadLeftoverEnergy || !hadLeftoverEnergy) return;
                __state = ReflectionUtil.GetDynamicVarIntValue(__instance, "Energy", 2);
            } catch { }
        }

        static void Postfix(PaelsTears __instance, Task __result, object __state) {
            try {
                var amount = __state is int value ? value : 0;
                if (amount <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Energy Gained", amount);
                    } catch { }
                });
            } catch { }
        }
    }
}
