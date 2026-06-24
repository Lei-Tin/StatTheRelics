using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(StoneCracker), nameof(StoneCracker.AfterRoomEntered))]
    public static class StoneCrackerPatch {
        [ThreadStatic] internal static StoneCracker? Current;

        internal readonly struct UpgradeSnapshot {
            public UpgradeSnapshot(bool isUpgraded, int level) {
                IsUpgraded = isUpgraded;
                Level = level;
            }

            public bool IsUpgraded { get; }
            public int Level { get; }
        }

        internal class UpgradeState {
            public Dictionary<int, UpgradeSnapshot> Cards { get; } = new();
        }

        static void Prefix(StoneCracker __instance, AbstractRoom room) {
            try {
                if (room is not CombatRoom || __instance?.Owner == null) return;
                Current = __instance;
            } catch { }
        }

        static void Postfix(StoneCracker __instance) {
            try {
                if (ReferenceEquals(Current, __instance)) Current = null;
            } catch {
                Current = null;
            }
        }

        internal static object? CaptureCards(IEnumerable<CardModel> cards) {
            try {
                if (Current == null || cards == null) return null;
                var state = new UpgradeState();
                foreach (var card in cards) {
                    if (card == null || card.Owner != Current.Owner) continue;
                    state.Cards[RuntimeHelpers.GetHashCode(card)] = GetUpgradeSnapshot(card);
                }

                return state.Cards.Count > 0 ? state : null;
            } catch {
                return null;
            }
        }

        internal static void Count(StoneCracker relic, IEnumerable<CardModel> cards, object? rawState) {
            try {
                if (rawState is not UpgradeState state || relic == null || cards == null) return;
                var upgraded = 0;
                foreach (var card in cards) {
                    if (card == null || card.Owner != relic.Owner) continue;
                    var key = RuntimeHelpers.GetHashCode(card);
                    if (!state.Cards.TryGetValue(key, out var before)) continue;

                    var after = GetUpgradeSnapshot(card);
                    if (after.Level <= before.Level && (!after.IsUpgraded || before.IsUpgraded)) continue;
                    upgraded++;
                }

                if (upgraded > 0) RelicTracker.AddAmount(relic, "Cards Upgraded", upgraded);
            } catch { }
        }

        static UpgradeSnapshot GetUpgradeSnapshot(object card) {
            try {
                var level = Math.Max(0, ReflectionUtil.GetIntMemberValue(card, "CurrentUpgradeLevel", 0));
                var upgraded = ReflectionUtil.GetMemberValue(card, "IsUpgraded") is bool isUpgraded
                    ? isUpgraded
                    : level > 0;
                return new UpgradeSnapshot(upgraded, level);
            } catch {
                return new UpgradeSnapshot(false, 0);
            }
        }
    }

    [HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Upgrade), new Type[] {
        typeof(IEnumerable<CardModel>),
        typeof(CardPreviewStyle)
    })]
    public static class StoneCrackerUpgradePatch {
        static void Prefix(IEnumerable<CardModel> cards, CardPreviewStyle style, ref object __state) {
            try {
                _ = style;
                var state = StoneCrackerPatch.CaptureCards(cards);
                if (state != null) __state = state;
            } catch { }
        }

        static void Postfix(IEnumerable<CardModel> cards, object __state) {
            try {
                var relic = StoneCrackerPatch.Current;
                if (relic == null || __state == null) return;
                StoneCrackerPatch.Count(relic, cards, __state);
            } catch { }
        }
    }
}
