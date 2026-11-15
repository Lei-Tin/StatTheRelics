using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new[] {
        typeof(PlayerChoiceContext), typeof(IEnumerable<DamageTargetData>), typeof(decimal), typeof(ValueProp),
        typeof(Creature), typeof(CardModel), typeof(CardPlay), typeof(ModifyDamageHookType)
    })]
    public static class ConductorsBatonPatch {
        [ThreadStatic] internal static bool Calculating;
        [ThreadStatic] internal static bool IgnoreReduction;

        static void Prefix(IEnumerable<DamageTargetData> targetData, decimal amount, ValueProp props, Creature dealer,
            CardModel cardSource, CardPlay cardPlay, ModifyDamageHookType modifyDamageHookType,
            out Dictionary<ConductorsBaton, int> __state) {
            __state = new();
            try {
                if (targetData == null || dealer?.IsDead == true || !props.IsPoweredAttack() || (modifyDamageHookType & ModifyDamageHookType.Multiplicative) == 0) return;
                foreach (var data in targetData) {
                    var target = data.target;
                    var relic = ReflectionUtil.FindRelic<ConductorsBaton>(target);
                    if (relic == null || target.IsDead || target.GetPower<ConductorsBatonPower>() == null) continue;
                    try {
                        Calculating = true;
                        var reduced = Hook.ModifyDamage(relic.Owner.RunState, target.CombatState, target, dealer, amount, props,
                            cardSource, cardPlay, modifyDamageHookType, CardPreviewMode.None, out _);
                        IgnoreReduction = true;
                        var original = Hook.ModifyDamage(relic.Owner.RunState, target.CombatState, target, dealer, amount, props,
                            cardSource, cardPlay, modifyDamageHookType, CardPreviewMode.None, out _);
                        // Damage reduction happens before Wuppy redirects it or player block absorbs it.
                        var mitigated = Math.Max(0, (int)original - (int)reduced);
                        if (mitigated > 0) __state[relic] = mitigated;
                    } finally {
                        IgnoreReduction = false;
                        Calculating = false;
                    }
                }
            } catch { }
        }

        static void Postfix(Task<IEnumerable<DamageResult>> __result, Dictionary<ConductorsBaton, int> __state) {
            if (__result == null || __state.Count == 0) return;
            __result.ContinueWith(task => {
                try {
                    if (task.Status != TaskStatus.RanToCompletion) return;
                    foreach (var pair in __state) {
                        if (task.Result.Any(result => result.OriginalTarget == pair.Key.Owner.Creature))
                            RelicTracker.AddAmount(pair.Key, "Damage Mitigated", pair.Value);
                    }
                } catch { }
            }, TaskContinuationOptions.ExecuteSynchronously);
        }
    }

    [HarmonyPatch(typeof(ConductorsBatonPower), nameof(ConductorsBatonPower.ModifyDamageMultiplicative))]
    public static class ConductorsBatonReductionPatch {
        static void Postfix(ref decimal __result) {
            // Omit the reduction only during this relic's synchronous comparison calculation.
            if (ConductorsBatonPatch.IgnoreReduction) __result = 1m;
        }
    }
}
