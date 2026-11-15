using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new[] {
        typeof(PlayerChoiceContext), typeof(IEnumerable<DamageTargetData>), typeof(decimal), typeof(ValueProp),
        typeof(Creature), typeof(CardModel), typeof(CardPlay), typeof(ModifyDamageHookType)
    })]
    public static class WuppyPatch {
        static void Postfix(ValueProp props, Task<IEnumerable<DamageResult>> __result) {
            try {
                if (!props.IsPoweredAttack() || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        foreach (var result in task.Result) {
                            var relic = ReflectionUtil.FindRelic<Wuppy>(result.OriginalTarget);
                            if (relic == null || result.Receiver != relic.CombatWuppy || result.OriginalTarget != relic.Owner.Creature) continue;
                            if (result.UnblockedDamage > 0) RelicTracker.AddAmount(relic, "HP Blocked", result.UnblockedDamage);
                        }
                    } catch { }
                }, TaskContinuationOptions.ExecuteSynchronously);
            } catch { }
        }
    }
}
