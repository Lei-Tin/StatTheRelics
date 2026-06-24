using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(NeowsBones), nameof(NeowsBones.AfterObtained))]
    public static class NeowsBonesPatch {
        static readonly object Sync = new();
        static NeowsBones? activeRelic;

        static void Prefix(NeowsBones __instance) {
            try {
                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(NeowsBones __instance, Task __result) {
            try {
                if (__result == null) {
                    Clear(__instance);
                    return;
                }

                __result.ContinueWith(_ => Clear(__instance));
            } catch {
                Clear(__instance);
            }
        }

        static void Clear(NeowsBones relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }

        internal static NeowsBones? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        internal static void AppendText(NeowsBones relic, string key, string value) {
            try {
                if (relic == null || string.IsNullOrWhiteSpace(value)) return;
                var current = RelicTracker.GetText(relic, key);
                RelicTracker.SetText(relic, key, string.IsNullOrWhiteSpace(current) ? value : current + "\n" + value);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(RelicReward), nameof(RelicReward.Populate))]
    public static class NeowsBonesRelicRewardPopulatePatch {
        static void Postfix(RelicReward __instance) {
            try {
                var relic = NeowsBonesPatch.ActiveRelic;
                if (relic == null || __instance == null) return;

                var offeredRelic = __instance.Relic ?? ReflectionUtil.GetMemberValue(__instance, "_relic");
                var name = ReflectionUtil.GetModelTitle(offeredRelic) ?? offeredRelic?.GetType().Name;
                if (!string.IsNullOrWhiteSpace(name)) NeowsBonesPatch.AppendText(relic, "Relics Offered", name);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Add), new Type[] {
        typeof(CardModel),
        typeof(PileType),
        typeof(CardPilePosition),
        typeof(AbstractModel),
        typeof(bool)
    })]
    public static class NeowsBonesCardAddPatch {
        static void Postfix(CardModel card, PileType newPileType, Task<CardPileAddResult> __result) {
            try {
                var bones = NeowsBonesPatch.ActiveRelic;
                if (bones == null || card == null || newPileType != PileType.Deck || __result == null) return;
                if (!IsCurse(card)) return;
                if (!IsFromNeowsBonesAfterObtained()) return;

                __result.ContinueWith((Task<CardPileAddResult> task) => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        var success = ReflectionUtil.GetMemberValue(task.Result, "success");
                        if (success is bool value && !value) return;
                        NeowsBonesPatch.AppendText(bones, "Curse Added", DeckUtil.GetCardDisplayName(card, preferBaseTitle: true));
                    } catch { }
                });
            } catch { }
        }

        static bool IsCurse(CardModel card) {
            try {
                return card.Type == CardType.Curse || card.Rarity == CardRarity.Curse;
            } catch {
                return false;
            }
        }

        static bool IsFromNeowsBonesAfterObtained() {
            try {
                var frames = new StackTrace().GetFrames();
                if (frames == null) return false;

                foreach (var frame in frames) {
                    var method = frame.GetMethod();
                    var typeName = method?.DeclaringType?.FullName;
                    if (typeName == null) continue;
                    if (typeName.Contains("MegaCrit.Sts2.Core.Models.Relics.NeowsBones", StringComparison.Ordinal)
                        && string.Equals(method?.Name, "MoveNext", StringComparison.Ordinal)) {
                        return true;
                    }
                }
            } catch { }

            return false;
        }
    }
}
