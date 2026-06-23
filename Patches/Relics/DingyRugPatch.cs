using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(DingyRug), nameof(DingyRug.ModifyCardRewardCreationOptions))]
    public static class DingyRugPatch {
        static readonly object TextLock = new();

        static void Prefix(DingyRug __instance, Player player, CardCreationOptions options, ref bool __state) {
            try {
                if (__instance == null || player == null || options == null) return;
                if (__instance.Owner != player) return;
                __state = true;
            } catch { }
        }

        static void Postfix(DingyRug __instance, CardCreationOptions options, CardCreationOptions __result, bool __state) {
            try {
                if (__state && options != null && __result != null && !ReferenceEquals(options, __result)) {
                    RelicTracker.AddAmount(__instance, "Rewards Modified", 1);
                }
            } catch { }
        }

        internal static DingyRug? FindRelic(CardReward reward) {
            try {
                var player = ReflectionUtil.GetMemberValue(reward, "Player");
                return ReflectionUtil.FindRelic<DingyRug>(player);
            } catch {
                return null;
            }
        }

        internal static void CountReward(DingyRug relic, IEnumerable<CardModel> cards) {
            try {
                var colorlessCards = cards
                    .Where(IsColorless)
                    .Select(GetCardName)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList();

                RelicTracker.AddAmount(relic, "Rewards Checked", 1);
                if (colorlessCards.Count <= 0) return;

                RelicTracker.AddAmount(relic, "Rewards With Colorless Cards", 1);
                RelicTracker.AddAmount(relic, "Colorless Cards Offered", colorlessCards.Count);
                AppendCardNames(relic, colorlessCards);
            } catch { }
        }

        static bool IsColorless(CardModel card) {
            try {
                return card?.Pool?.IsColorless == true;
            } catch {
                return false;
            }
        }

        static string GetCardName(CardModel card) {
            return ReflectionUtil.GetCardBaseTitle(card)
                ?? ReflectionUtil.GetCardTitle(card)
                ?? card.GetType().Name;
        }

        static void AppendCardNames(DingyRug relic, IReadOnlyList<string> names) {
            try {
                lock (TextLock) {
                    var existing = RelicTracker.GetText(relic, "Colorless Cards");
                    var values = string.IsNullOrWhiteSpace(existing)
                        ? new List<string>()
                        : existing.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToList();

                    values.AddRange(names);
                    RelicTracker.SetText(relic, "Colorless Cards", string.Join("\n", values));
                }
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardReward), nameof(CardReward.Populate))]
    public static class DingyRugCardRewardPatch {
        static void Postfix(CardReward __instance) {
            try {
                if (__instance == null) return;
                var relic = DingyRugPatch.FindRelic(__instance);
                if (relic == null) return;
                if (!WasDingyRugEligible(__instance)) return;
                DingyRugPatch.CountReward(relic, __instance.Cards ?? Array.Empty<CardModel>());
            } catch { }
        }

        static bool WasDingyRugEligible(CardReward reward) {
            try {
                var options = ReflectionUtil.GetMemberValue(reward, "Options") as CardCreationOptions;
                if (options == null) return false;
                if (options.Flags.HasFlag((CardCreationFlags)16)) return false;
                if (!options.Flags.HasFlag((CardCreationFlags)128)) return false;
                if (options.CustomCardPool != null) return false;
                return true;
            } catch {
                return true;
            }
        }
    }
}
