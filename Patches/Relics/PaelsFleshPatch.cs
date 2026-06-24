using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PaelsFlesh), nameof(PaelsFlesh.AfterSideTurnStart))]
    public static class PaelsFleshPatch {
        static void Prefix(PaelsFlesh __instance, CombatSide side, System.Collections.Generic.IReadOnlyList<Creature> participants, ICombatState combatState, ref object __state) {
            try {
                _ = side;
                _ = combatState;
                if (__instance?.Owner?.Creature == null || participants == null) return;
                if (!participants.Contains(__instance.Owner.Creature)) return;
                if (__instance.Owner.PlayerCombatState?.TurnNumber < 3) return;
                __state = ReflectionUtil.GetDynamicVarIntValue(__instance, "Energy", 1);
            } catch { }
        }

        static void Postfix(PaelsFlesh __instance, Task __result, object __state) {
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
