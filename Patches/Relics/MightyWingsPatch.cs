using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(MightyWings), nameof(MightyWings.AfterDamageGiven))]
    public static class MightyWingsPatch {
        internal const string CombatStrength = "Mighty Wings Strength This Combat";
        static void Prefix(MightyWings __instance, Creature dealer, DamageResult result, ValueProp props, out int __state) {
            __state = 0;
            try {
                if (dealer == null || !props.IsPoweredAttack() || (dealer != __instance.Owner.Creature && dealer != __instance.Owner.Osty) || result.TotalDamage < ReflectionUtil.GetDynamicVarIntValue(__instance, "DamageThreshold")) return;
                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "StrengthPower"));
            } catch { }
        }

        static void Postfix(MightyWings __instance, Task __result, int __state) {
            try {
                if (__state <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        RelicTracker.AddAmount(__instance, "Strength Gained", __state);
                        RelicTracker.AddAmount(__instance, CombatStrength, __state);
                    } catch { }
                }, TaskContinuationOptions.ExecuteSynchronously);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterCombatEnd))]
    public static class MightyWingsCombatEndPatch {
        static void Postfix(AbstractModel __instance) {
            if (__instance is MightyWings relic && RelicTracker.GetOrCreate(relic) is { } data)
                data.Counters[MightyWingsPatch.CombatStrength] = 0;
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new[] {
        typeof(PlayerChoiceContext), typeof(IEnumerable<DamageTargetData>), typeof(decimal), typeof(ValueProp),
        typeof(Creature), typeof(CardModel), typeof(CardPlay), typeof(ModifyDamageHookType)
    })]
    public static class MightyWingsDamagePatch {
        [ThreadStatic] internal static bool Calculating;
        [ThreadStatic] internal static int ExcludedStrength;

        static void Prefix(IEnumerable<DamageTargetData> targetData, decimal amount, ValueProp props, Creature dealer,
            CardModel cardSource, CardPlay cardPlay, ModifyDamageHookType modifyDamageHookType,
            out Dictionary<Creature, int> __state) {
            __state = new();
            try {
                if (targetData == null || dealer == null || !props.IsPoweredAttack() || (modifyDamageHookType & ModifyDamageHookType.Additive) == 0) return;
                var relic = ReflectionUtil.FindRelic<MightyWings>(dealer);
                if (relic == null || dealer != relic.Owner.Creature) return;
                var strength = (int)Math.Min(RelicTracker.GetCounter(relic, MightyWingsPatch.CombatStrength), Math.Max(0, dealer.GetPower<StrengthPower>()?.Amount ?? 0));
                if (strength <= 0) return;
                foreach (var data in targetData) {
                    var target = data.target;
                    if (target.IsDead) continue;
                    try {
                        Calculating = true;
                        var boosted = Hook.ModifyDamage(relic.Owner.RunState, target.CombatState, target, dealer, amount, props,
                            cardSource, cardPlay, modifyDamageHookType, CardPreviewMode.None, out _);
                        ExcludedStrength = strength;
                        var original = Hook.ModifyDamage(relic.Owner.RunState, target.CombatState, target, dealer, amount, props,
                            cardSource, cardPlay, modifyDamageHookType, CardPreviewMode.None, out _);
                        __state[target] = Math.Max(0, (int)boosted - (int)original);
                    } finally {
                        ExcludedStrength = 0;
                        Calculating = false;
                    }
                }
            } catch { }
        }

        static void Postfix(Creature dealer, Task<IEnumerable<DamageResult>> __result, Dictionary<Creature, int> __state) {
            try {
                var relic = ReflectionUtil.FindRelic<MightyWings>(dealer);
                if (relic == null || __result == null || __state.Count == 0) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        var bonus = task.Result.GroupBy(result => result.OriginalTarget).Sum(group =>
                            __state.TryGetValue(group.Key, out var extra) ? Math.Min(extra, group.Sum(result => Math.Max(0, result.TotalDamage))) : 0);
                        if (bonus > 0) RelicTracker.AddAmount(relic, "Extra Damage Dealt", bonus);
                    } catch { }
                }, TaskContinuationOptions.ExecuteSynchronously);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(StrengthPower), nameof(StrengthPower.ModifyDamageAdditive))]
    public static class MightyWingsStrengthDamagePatch {
        static void Postfix(ref decimal __result) {
            if (MightyWingsDamagePatch.ExcludedStrength > 0 && __result > 0)
                __result -= Math.Min(__result, MightyWingsDamagePatch.ExcludedStrength);
        }
    }
}
