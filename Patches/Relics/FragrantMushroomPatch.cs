using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(FragrantMushroom), nameof(FragrantMushroom.AfterObtained))]
    public static class FragrantMushroomPatch {
        class PickupState {
            public int BeforeHp { get; set; }
            public List<CardState> Cards { get; } = new();
        }

        class CardState {
            public object Card { get; set; } = null!;
            public string Name { get; set; } = string.Empty;
            public bool WasUpgraded { get; set; }
        }

        static void Prefix(FragrantMushroom __instance, ref object __state) {
            try {
                if (__instance?.Owner?.Creature == null) return;
                var state = new PickupState {
                    BeforeHp = GetCurrentHp(__instance.Owner.Creature)
                };
                state.Cards.AddRange(CaptureCardStates(__instance));
                __state = state;
            } catch { }
        }

        static void Postfix(FragrantMushroom __instance, Task __result, object __state) {
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

        static void Count(FragrantMushroom relic, PickupState state) {
            try {
                var hpLost = Math.Max(0, state.BeforeHp - GetCurrentHp(relic.Owner?.Creature));
                if (hpLost > 0) RelicTracker.AddAmount(relic, "HP Lost", hpLost);

                var upgraded = state.Cards
                    .Where(c => !c.WasUpgraded && IsUpgraded(c.Card))
                    .Select(c => c.Name)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList();

                if (upgraded.Count <= 0) return;
                RelicTracker.SetText(relic, "Cards Upgraded", DeckUtil.JoinCardList(upgraded));
            } catch { }
        }

        static IEnumerable<CardState> CaptureCardStates(FragrantMushroom relic) {
            foreach (var card in DeckUtil.EnumerateDeckCards(relic.Owner)) {
                yield return new CardState {
                    Card = card,
                    Name = DeckUtil.GetCardDisplayName(card, preferBaseTitle: true),
                    WasUpgraded = IsUpgraded(card)
                };
            }
        }

        static int GetCurrentHp(object? creature) {
            try {
                var hp = ReflectionUtil.GetMemberValue(creature, "CurrentHp")
                    ?? ReflectionUtil.GetMemberValue(creature, "Hp")
                    ?? ReflectionUtil.GetMemberValue(creature, "Health");
                return hp == null ? 0 : Convert.ToInt32(hp);
            } catch {
                return 0;
            }
        }

        static bool IsUpgraded(object card) {
            try {
                var raw = ReflectionUtil.GetMemberValue(card, "IsUpgraded");
                return raw is bool value && value;
            } catch {
                return false;
            }
        }
    }
}
