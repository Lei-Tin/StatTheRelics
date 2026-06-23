using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace StatTheRelics.Patches.Relics {
    public static class DingyRugPatch {
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
                var colorlessCount = cards.Count(IsColorless);
                if (colorlessCount <= 0) return;

                RelicTracker.AddAmount(relic, "Colorless Cards Offered", colorlessCount);
            } catch { }
        }

        static bool IsColorless(CardModel card) {
            try {
                return card?.Pool?.IsColorless == true;
            } catch {
                return false;
            }
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
