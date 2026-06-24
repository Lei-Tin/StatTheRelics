using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(NeowsTalisman), nameof(NeowsTalisman.AfterObtained))]
    public static class NeowsTalismanPatch {
        class State {
            public Dictionary<int, (bool Upgraded, string Name)> Cards { get; } = new();
        }

        static void Prefix(NeowsTalisman __instance, ref object __state) {
            try {
                var state = new State();
                foreach (var card in DeckUtil.EnumerateDeckCards(__instance.Owner)) {
                    var key = RuntimeHelpers.GetHashCode(card);
                    var upgraded = ReflectionUtil.GetMemberValue(card, "IsUpgraded") is bool value && value;
                    state.Cards[key] = (upgraded, DeckUtil.GetCardDisplayName(card, preferBaseTitle: true));
                }

                __state = state;
            } catch { }
        }

        static void Postfix(NeowsTalisman __instance, object __state) {
            try {
                if (__state is not State state) return;

                foreach (var card in DeckUtil.EnumerateDeckCards(__instance.Owner)) {
                    var key = RuntimeHelpers.GetHashCode(card);
                    if (!state.Cards.TryGetValue(key, out var before) || before.Upgraded) continue;

                    var upgraded = ReflectionUtil.GetMemberValue(card, "IsUpgraded") is bool value && value;
                    if (!upgraded) continue;

                    AppendText(__instance, "Upgraded Card Name", before.Name);
                }
            } catch { }
        }

        static void AppendText(NeowsTalisman relic, string key, string value) {
            var current = RelicTracker.GetText(relic, key);
            RelicTracker.SetText(relic, key, string.IsNullOrWhiteSpace(current) ? value : current + "\n" + value);
        }
    }
}
