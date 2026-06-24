using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    // Count the damage dealt by Bronze Scales' initial 3 Thorns.
    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(Creature),
        typeof(decimal),
        typeof(ValueProp),
        typeof(Creature),
        typeof(CardModel)
    })]
    public static class BronzeScalesPatch {
        class BronzeScalesState {
            public BronzeScales? Relic { get; set; }
            public int Cap { get; set; }
        }

        static void Prefix(Creature target, decimal amount, Creature dealer, CardModel cardSource, ref object __state) {
            try {
                if (target == null || dealer == null) return;
                if (cardSource != null) return;
                if (amount <= 0) return;
                if (!IsThornsPowerCall()) return;

                var relic = ReflectionUtil.FindRelic<BronzeScales>(dealer);
                if (relic == null) return;

                var bronzeThorns = ReflectionUtil.GetDynamicVarIntValue(relic, "ThornsPower", 3);
                if (bronzeThorns <= 0) bronzeThorns = 3;

                __state = new BronzeScalesState {
                    Relic = relic,
                    Cap = Math.Max(0, Math.Min((int)amount, bronzeThorns))
                };
            } catch { }
        }

        static void Postfix(Task<IEnumerable<DamageResult>> __result, object __state) {
            try {
                var state = __state as BronzeScalesState;
                if (state?.Relic == null || state.Cap <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(state.Relic, "Thorns Damage", state.Cap);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;

                        var total = 0;
                        foreach (var result in task.Result) {
                            if (result == null) continue;
                            total += Math.Max(0, result.TotalDamage);
                        }

                        var amount = Math.Min(state.Cap, total);
                        if (amount > 0) RelicTracker.AddAmount(state.Relic, "Thorns Damage", amount);
                    } catch { }
                });
            } catch { }
        }

        static bool IsThornsPowerCall() {
            try {
                var frames = new StackTrace().GetFrames();
                if (frames == null) return false;
                foreach (var frame in frames) {
                    var typeName = frame.GetMethod()?.DeclaringType?.FullName;
                    if (typeName != null && typeName.IndexOf("ThornsPower", StringComparison.Ordinal) >= 0) return true;
                }
            } catch { }

            return false;
        }
    }
}
