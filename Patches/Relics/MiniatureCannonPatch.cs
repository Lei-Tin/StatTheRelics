using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    public static class MiniatureCannonPatch {
        internal static int GetBonus(CardModel? cardSource, ValueProp props) {
            try {
                if (cardSource == null || !props.IsPoweredAttack() || !cardSource.IsUpgraded) return 0;
                var relic = ReflectionUtil.FindRelic<MiniatureCannon>(cardSource.Owner);
                return relic == null ? 0 : Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(relic, "ExtraDamage", 3));
            } catch {
                return 0;
            }
        }

        internal static MiniatureCannon? GetRelic(CardModel? cardSource) {
            try {
                return cardSource == null ? null : ReflectionUtil.FindRelic<MiniatureCannon>(cardSource.Owner);
            } catch {
                return null;
            }
        }

        internal static void CountResults(MiniatureCannon? relic, int bonusPerHit, Task<IEnumerable<DamageResult>> task) {
            try {
                if (relic == null || bonusPerHit <= 0 || task == null) return;

                task.ContinueWith(t => {
                    try {
                        if (t.Status != TaskStatus.RanToCompletion || t.Result == null) return;
                        var total = t.Result.Sum(result => result == null ? 0 : Math.Min(bonusPerHit, Math.Max(0, result.TotalDamage)));
                        if (total > 0) RelicTracker.AddAmount(relic, "Bonus Damage", total);
                    } catch { }
                });
            } catch { }
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
    public static class MiniatureCannonManyDecimalDamagePatch {
        static void Postfix(ValueProp props, CardModel cardSource, Task<IEnumerable<DamageResult>> __result) {
            var bonus = MiniatureCannonPatch.GetBonus(cardSource, props);
            MiniatureCannonPatch.CountResults(MiniatureCannonPatch.GetRelic(cardSource), bonus, __result);
        }
    }
}
