using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(DistinguishedCape), nameof(DistinguishedCape.AfterObtained))]
    public static class DistinguishedCapePatch {
        internal const string TypeName = "MegaCrit.Sts2.Core.Models.Relics.DistinguishedCape";

        class DistinguishedCapeState {
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
            public int HpLoss { get; set; }
        }

        static void Prefix(DistinguishedCape __instance, ref object __state) {
            try {
                __state = new DistinguishedCapeState {
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(__instance, preferBaseTitle: true),
                    HpLoss = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "HpLoss", 9))
                };
            } catch { }
        }

        static void Postfix(DistinguishedCape __instance, Task __result, object __state) {
            try {
                var state = __state as DistinguishedCapeState;
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

        static void Count(DistinguishedCape relic, DistinguishedCapeState? state) {
            try {
                if (state == null) return;

                if (state.HpLoss > 0) RelicTracker.AddAmount(relic, "Max HP Lost", state.HpLoss);

                var after = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var added = DeckUtil.FindAddedCards(state.BeforeDeck, after);
                if (added.Count <= 0) return;

                RelicTracker.AddAmount(relic, "Cards Added", added.Count);
                RelicTracker.SetText(relic, "Cards Added", DeckUtil.JoinCardList(added));
            } catch { }
        }

        internal static bool IsApparition(CardModel card) {
            try {
                var name = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true);
                return string.Equals(name, "Apparition", StringComparison.OrdinalIgnoreCase);
            } catch {
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class DistinguishedCapeApparitionPlayPatch {
        static void Postfix(CardModel __instance) {
            try {
                if (__instance == null) return;
                if (!RelicTracker.HasTrackedRelicType(DistinguishedCapePatch.TypeName)) return;
                if (!DistinguishedCapePatch.IsApparition(__instance)) return;

                RelicTracker.AddAmountByType(DistinguishedCapePatch.TypeName, "Apparitions Played", 1);
            } catch { }
        }
    }
}
