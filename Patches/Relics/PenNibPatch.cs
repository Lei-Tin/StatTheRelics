using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    public static class PenNibPatch {
        internal static PenNib? GetActiveRelic(CardModel? cardSource) {
            try {
                if (cardSource == null) return null;
                var relic = ReflectionUtil.FindRelic<PenNib>(cardSource.Owner);
                var attackToDouble = ReflectionUtil.GetMemberValue(relic, "AttackToDouble");
                return ReferenceEquals(attackToDouble, cardSource) ? relic : null;
            } catch {
                return null;
            }
        }

        internal static void CountBonus(PenNib? relic, decimal amount, Task<IEnumerable<DamageResult>> task) {
            try {
                if (relic == null || amount <= 0 || task == null) return;
                var undoubledDamage = Math.Max(0, Convert.ToInt32(Math.Floor(amount)));

                task.ContinueWith(t => {
                    try {
                        if (t.Status != TaskStatus.RanToCompletion || t.Result == null) return;
                        var bonus = t.Result.Sum(result => result == null
                            ? 0
                            : Math.Max(0, Math.Max(0, result.TotalDamage) - Math.Min(undoubledDamage, Math.Max(0, result.TotalDamage))));
                        if (bonus > 0) RelicTracker.AddAmount(relic, "Bonus Damage Dealt", bonus);
                    } catch { }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(Creature),
        typeof(decimal),
        typeof(ValueProp),
        typeof(Creature),
        typeof(CardModel)
    })]
    public static class PenNibSingleDecimalDamagePatch {
        static void Postfix(decimal amount, CardModel cardSource, Task<IEnumerable<DamageResult>> __result) {
            PenNibPatch.CountBonus(PenNibPatch.GetActiveRelic(cardSource), amount, __result);
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(IEnumerable<Creature>),
        typeof(decimal),
        typeof(ValueProp),
        typeof(Creature),
        typeof(CardModel)
    })]
    public static class PenNibManyDecimalDamagePatch {
        static void Postfix(decimal amount, CardModel cardSource, Task<IEnumerable<DamageResult>> __result) {
            PenNibPatch.CountBonus(PenNibPatch.GetActiveRelic(cardSource), amount, __result);
        }
    }
}
