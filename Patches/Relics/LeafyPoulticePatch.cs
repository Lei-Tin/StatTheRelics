using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(LeafyPoultice), nameof(LeafyPoultice.AfterObtained))]
    public static class LeafyPoulticePatch {
        class PickupState {
            public int BeforeMaxHp { get; set; }
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(LeafyPoultice __instance, ref object __state) {
            try {
                __state = new PickupState {
                    BeforeMaxHp = GetMaxHp(__instance.Owner?.Creature),
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true)
                };
            } catch { }
        }

        static void Postfix(LeafyPoultice __instance, Task __result, object __state) {
            try {
                var state = __state as PickupState;
                if (state == null) return;

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

        static void Count(LeafyPoultice relic, PickupState state) {
            try {
                var maxHpLost = Math.Max(0, state.BeforeMaxHp - GetMaxHp(relic.Owner?.Creature));
                if (maxHpLost > 0) RelicTracker.AddAmount(relic, "Max HP Lost", maxHpLost);

                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var removed = DeckUtil.FindRemovedCards(state.BeforeDeck, after);
                var added = DeckUtil.FindAddedCards(state.BeforeDeck, after);

                if (removed.Count > 0) RelicTracker.SetText(relic, "Cards Transformed", DeckUtil.JoinCardList(removed));
                if (added.Count > 0) RelicTracker.SetText(relic, "Cards Obtained", DeckUtil.JoinCardList(added));
            } catch { }
        }

        static int GetMaxHp(object? creature) {
            try {
                var raw = ReflectionUtil.GetMemberValue(creature, "MaxHp")
                    ?? ReflectionUtil.GetMemberValue(creature, "MaxHealth");
                return raw == null ? 0 : Convert.ToInt32(raw);
            } catch {
                return 0;
            }
        }
    }
}
