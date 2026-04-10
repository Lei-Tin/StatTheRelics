using System;
using System.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    // Count how many cards Bone Tea upgrades when it is consumed.
    [HarmonyPatch(typeof(BoneTea), nameof(BoneTea.AfterSideTurnStart))]
    public static class BoneTeaPatch {
        class BoneTeaState {
            public int Round { get; set; }
            public bool WasUsedUp { get; set; }
            public int UpgradableInHand { get; set; }
        }

        static void Prefix(BoneTea __instance, ref object __state) {
            try {
                var owner = __instance?.Owner;
                var round = ReflectionUtil.GetIntMemberValue(owner?.Creature?.CombatState, "RoundNumber", int.MaxValue);
                var isUsedUpRaw = ReflectionUtil.GetMemberValue(__instance, "IsUsedUp");
                var wasUsedUp = isUsedUpRaw is bool isUsedUp && isUsedUp;

                var upgradable = 0;
                if (!wasUsedUp && round == 1 && owner != null) {
                    var handCards = PileType.Hand.GetPile(owner)?.Cards;
                    upgradable = CountUnupgradedCards(handCards);
                }

                __state = new BoneTeaState {
                    Round = round,
                    WasUsedUp = wasUsedUp,
                    UpgradableInHand = upgradable
                };

                ModLog.Info($"BoneTeaPatch: Prefix round={round}, wasUsedUp={wasUsedUp}, upgradableInHand={upgradable}");
            } catch { }
        }

        static void Postfix(BoneTea __instance, object __state) {
            try {
                var state = __state as BoneTeaState;
                if (state == null) return;
                if (state.Round != 1) return;
                if (state.WasUsedUp) return;
                var isUsedUpRaw = ReflectionUtil.GetMemberValue(__instance, "IsUsedUp");
                if (isUsedUpRaw is not bool isUsedUp || !isUsedUp) return;
                if (state.UpgradableInHand <= 0) return;

                var relicTypeName = __instance?.GetType().FullName;
                if (!string.IsNullOrWhiteSpace(relicTypeName)) {
                    RelicTracker.AddAmountByType(relicTypeName, "Cards Upgraded", state.UpgradableInHand);
                }
                ModLog.Info($"BoneTeaPatch: Postfix added Cards Upgraded={state.UpgradableInHand}");
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
