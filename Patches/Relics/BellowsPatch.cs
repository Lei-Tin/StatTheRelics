using System;
using System.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Count how many cards Bellows upgrades in the opening hand.
    [HarmonyPatch(typeof(Bellows), nameof(Bellows.AfterPlayerTurnStart))]
    public static class BellowsPatch {
        class BellowsState {
            public int Round { get; set; }
            public bool IsOwnerTurn { get; set; }
            public int UpgradableInHand { get; set; }
        }

        static void Prefix(Bellows __instance, Player player, ref object __state) {
            try {
                var isOwnerTurn = __instance?.Owner != null && player == __instance.Owner;
                var round = ReflectionUtil.GetIntMemberValue(player?.Creature?.CombatState, "RoundNumber", int.MaxValue);

                var upgradable = 0;
                if (isOwnerTurn && round == 1) {
                    var owner = __instance?.Owner;
                    if (owner != null) {
                        var handCards = PileType.Hand.GetPile(owner)?.Cards;
                        upgradable = CountUnupgradedCards(handCards);
                    }
                }

                __state = new BellowsState {
                    Round = round,
                    IsOwnerTurn = isOwnerTurn,
                    UpgradableInHand = upgradable
                };

                ModLog.Info($"BellowsPatch: Prefix round={round}, isOwnerTurn={isOwnerTurn}, upgradableInHand={upgradable}");
            } catch { }
        }

        static void Postfix(Bellows __instance, object __state) {
            try {
                var state = __state as BellowsState;
                if (state == null) return;
                if (!state.IsOwnerTurn) return;
                if (state.Round != 1) return;
                if (state.UpgradableInHand <= 0) return;

                RelicTracker.AddAmount(__instance, "Cards Upgraded", state.UpgradableInHand);
                ModLog.Info($"BellowsPatch: Postfix added Cards Upgraded={state.UpgradableInHand}");
            } catch { }
        }

        static int CountUnupgradedCards(object? cardsObj) {
            try {
                if (cardsObj is not IEnumerable cards) return 0;
                var count = 0;
                foreach (var card in cards) {
                    if (card == null) continue;
                    if (!IsCardUpgraded(card)) count++;
                }
                return count;
            } catch {
                return 0;
            }
        }

        static bool IsCardUpgraded(object card) {
            try {
                var isUpgraded = ReflectionUtil.GetMemberValue(card, "IsUpgraded");
                if (isUpgraded == null) return false;
                return Convert.ToBoolean(isUpgraded);
            } catch {
                return false;
            }
        }
    }
}
