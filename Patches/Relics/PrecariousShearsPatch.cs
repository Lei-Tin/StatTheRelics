using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PrecariousShears), nameof(PrecariousShears.AfterObtained))]
    public static class PrecariousShearsPatch {
        class State {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
            public int HpBefore { get; set; }
        }

        static void Prefix(PrecariousShears __instance, ref object __state) {
            try {
                __state = new State {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true),
                    HpBefore = __instance?.Owner?.Creature?.CurrentHp ?? 0
                };
            } catch { }
        }

        static void Postfix(PrecariousShears __instance, Task __result, object __state) {
            try {
                if (__state is not State state) return;
                if (__result == null) {
                    Count(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, state);
                    } catch { }
                });
            } catch { }
        }

        static void Count(PrecariousShears relic, State state) {
            try {
                if (relic == null) return;
                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var removed = DeckUtil.FindRemovedCards(state.BeforeDeck, after);
                if (removed.Count > 0) {
                    RelicTracker.AddAmount(relic, "Cards Removed", removed.Count);
                    RelicTracker.SetText(relic, "Cards Removed", DeckUtil.JoinCardList(removed));
                }

                var hpAfter = relic.Owner?.Creature?.CurrentHp ?? state.HpBefore;
                var damage = Math.Max(0, state.HpBefore - hpAfter);
                if (damage > 0) RelicTracker.AddAmount(relic, "Damage Taken", damage);
            } catch { }
        }
    }
}
