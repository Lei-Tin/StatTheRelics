using System;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    public static class GhostSeedPatch {
        internal static void CountEtherealExhaust(GhostSeed relic, CardModel card) {
            try {
                if (relic == null || card == null || !IsBasicStrikeOrDefend(card)) return;
                RelicTracker.AddAmount(relic, "Strike & Defends Exhausted (from ethereal)", 1);
            } catch { }
        }

        static bool IsBasicStrikeOrDefend(CardModel card) {
            try {
                if (Convert.ToInt32(card.Rarity) != 1) return false;
                return card.Tags != null && (card.Tags.Contains((CardTag)1) || card.Tags.Contains((CardTag)2));
            } catch {
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Exhaust), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(CardModel),
        typeof(bool),
        typeof(bool)
    })]
    public static class GhostSeedEtherealExhaustPatch {
        static void Prefix(CardModel card, bool causedByEthereal, ref object __state) {
            try {
                if (!causedByEthereal || card?.Owner == null) return;
                var relic = ReflectionUtil.FindRelic<GhostSeed>(card.Owner);
                if (relic == null) return;
                __state = Tuple.Create(relic, card);
            } catch { }
        }

        static void Postfix(Task __result, object __state) {
            try {
                if (__state is not Tuple<GhostSeed, CardModel> state) return;

                if (__result == null) {
                    GhostSeedPatch.CountEtherealExhaust(state.Item1, state.Item2);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            GhostSeedPatch.CountEtherealExhaust(state.Item1, state.Item2);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
