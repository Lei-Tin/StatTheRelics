using System;
using System.Collections.Generic;
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
    [HarmonyPatch(typeof(LostWisp), nameof(LostWisp.AfterCardPlayed))]
    public static class LostWispPatch {
        [ThreadStatic] internal static LostWisp? Current;

        static void Prefix(LostWisp __instance, CardPlay cardPlay) {
            try {
                var card = cardPlay?.Card;
                if (card == null || card.Owner != __instance.Owner) return;
                if (Convert.ToInt32(card.Type) != 3) return;

                Current = __instance;
            } catch { }
        }

        static void Postfix(Task __result) {
            try {
                _ = __result;
                Current = null;
            } catch {
                Current = null;
            }
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
    public static class LostWispDamagePatch {
        class DamageState {
            public LostWisp? Relic { get; set; }
        }

        static void Prefix(ref object __state) {
            try {
                if (LostWispPatch.Current == null) return;
                __state = new DamageState { Relic = LostWispPatch.Current };
            } catch { }
        }

        static void Postfix(Task<IEnumerable<DamageResult>> __result, object __state) {
            try {
                var state = __state as DamageState;
                if (state?.Relic == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
                        var total = 0;
                        foreach (var result in task.Result) {
                            if (result == null) continue;
                            total += Math.Max(0, result.TotalDamage - result.OverkillDamage);
                        }

                        if (total > 0) RelicTracker.AddAmount(state.Relic, "Damage Dealt", total);
                    } catch { }
                });
            } catch { }
        }
    }
}
