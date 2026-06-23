using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(CloakClasp), nameof(CloakClasp.BeforeSideTurnEnd))]
    public static class CloakClaspPatch {
        class CloakClaspState {
            public int Block { get; set; }
        }

        static void Prefix(CloakClasp __instance, PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ref object __state) {
            try {
                if (__instance == null || participants == null) return;
                var owner = __instance.Owner;
                if (owner == null) return;
                var ownerCreature = owner.Creature;
                if (ownerCreature == null || !participants.Contains(ownerCreature)) return;

                var handPile = PileTypeExtensions.GetPile((PileType)2, owner);
                var handCount = handPile?.Cards?.Count ?? 0;
                if (handCount <= 0) return;

                var blockPerCard = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Block", 1));
                var block = handCount * blockPerCard;
                if (block <= 0) return;

                __state = new CloakClaspState { Block = block };
            } catch { }
        }

        static void Postfix(CloakClasp __instance, Task __result, object __state) {
            try {
                var state = __state as CloakClaspState;
                if (state == null || state.Block <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Block Gained", state.Block);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            RelicTracker.AddAmount(__instance, "Block Gained", state.Block);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
