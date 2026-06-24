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
    public static class StrikeDummyPatch {
        internal static StrikeDummy? GetRelic(CardModel? cardSource, Creature? dealer) {
            try {
                if (cardSource == null || !IsStrike(cardSource)) return null;

                var relic = ReflectionUtil.FindRelic<StrikeDummy>(cardSource.Owner);
                if (relic == null) relic = ReflectionUtil.FindRelic<StrikeDummy>(dealer);
                return relic;
            } catch {
                return null;
            }
        }

        internal static void CountBonus(StrikeDummy? relic, Task<IEnumerable<DamageResult>> task) {
            try {
                if (relic == null || task == null) return;

                var extraDamage = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(relic, "ExtraDamage", 3));
                if (extraDamage <= 0) return;

                task.ContinueWith(t => {
                    try {
                        if (t.Status != TaskStatus.RanToCompletion || t.Result == null) return;
                        var bonus = t.Result.Sum(result => result == null ? 0 : Math.Min(extraDamage, Math.Max(0, result.TotalDamage)));
                        if (bonus > 0) RelicTracker.AddAmount(relic, "Bonus Damage Dealt", bonus);
                    } catch { }
                });
            } catch { }
        }

        static bool IsStrike(CardModel card) {
            try {
                return card.Tags != null && card.Tags.Contains((CardTag)1);
            } catch {
                return false;
            }
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
    public static class StrikeDummySingleDamagePatch {
        static void Postfix(Creature dealer, CardModel cardSource, Task<IEnumerable<DamageResult>> __result) {
            StrikeDummyPatch.CountBonus(StrikeDummyPatch.GetRelic(cardSource, dealer), __result);
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
    public static class StrikeDummyManyDamagePatch {
        static void Postfix(Creature dealer, CardModel cardSource, Task<IEnumerable<DamageResult>> __result) {
            StrikeDummyPatch.CountBonus(StrikeDummyPatch.GetRelic(cardSource, dealer), __result);
        }
    }
}
