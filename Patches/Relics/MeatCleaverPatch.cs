using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(MeatCleaver), nameof(MeatCleaver.TryModifyRestSiteOptions))]
    public static class MeatCleaverPatch {
        static void Postfix(MeatCleaver __instance, Player player, ICollection<RestSiteOption> options, bool __result) {
            try {
                _ = options;
                if (!__result || __instance == null || player == null || __instance.Owner != player) return;
                RelicTracker.GetOrCreate(__instance);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CookRestSiteOption), nameof(CookRestSiteOption.OnSelect))]
    public static class MeatCleaverCookPatch {
        class CookState {
            public MeatCleaver? Relic { get; set; }
            public Dictionary<string, int> BeforeDeck { get; set; } = new(StringComparer.Ordinal);
            public int BeforeMaxHp { get; set; }
        }

        static void Prefix(CookRestSiteOption __instance, ref object __state) {
            try {
                var owner = ReflectionUtil.GetMemberValue(__instance, "Owner") as Player;
                var relic = ReflectionUtil.FindRelic<MeatCleaver>(owner);
                if (owner == null || relic == null) return;

                __state = new CookState {
                    Relic = relic,
                    BeforeDeck = DeckUtil.CaptureDeckHistogramFromOwner(owner, preferBaseTitle: true),
                    BeforeMaxHp = owner.Creature?.MaxHp ?? 0
                };
            } catch { }
        }

        static void Postfix(Task<bool> __result, object __state) {
            try {
                var state = __state as CookState;
                if (state?.Relic == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || !task.Result) return;
                        Count(state);
                    } catch { }
                });
            } catch { }
        }

        static void Count(CookState state) {
            try {
                var relic = state.Relic;
                if (relic == null) return;

                var afterDeck = DeckUtil.CaptureDeckHistogramFromRelicOwner(relic, preferBaseTitle: true);
                var removed = DeckUtil.FindRemovedCards(state.BeforeDeck, afterDeck).Count;
                var maxHpGained = Math.Max(0, (relic.Owner?.Creature?.MaxHp ?? 0) - state.BeforeMaxHp);

                if (removed > 0) RelicTracker.AddAmount(relic, "Cards Removed", removed);
                if (maxHpGained > 0) RelicTracker.AddAmount(relic, "Max HP Gained", maxHpGained);
            } catch { }
        }
    }
}
