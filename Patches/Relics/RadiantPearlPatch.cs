using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(RadiantPearl), nameof(RadiantPearl.BeforeHandDraw))]
    public static class RadiantPearlPatch {
        [ThreadStatic] internal static RadiantPearl? Current;
        static readonly object Sync = new();
        static readonly ConditionalWeakTable<CardModel, RadiantPearlRef> MarkedCards = new();

        sealed class RadiantPearlRef {
            public RadiantPearl? Relic { get; set; }
        }

        static void Prefix(RadiantPearl __instance, Player player, PlayerChoiceContext choiceContext, CombatState combatState) {
            try {
                if (__instance == null || player == null || combatState == null) return;
                if (__instance.Owner != player) return;
                if (combatState.RoundNumber != 1) return;
                Current = __instance;
            } catch { }
        }

        static void Postfix() {
            Current = null;
        }

        internal static void MarkWhenAdded(RadiantPearl relic, Task<IReadOnlyList<CardPileAddResult>> task) {
            try {
                if (relic == null || task == null) return;
                task.ContinueWith(t => {
                    try {
                        if (t.Status != TaskStatus.RanToCompletion || t.Result == null) return;
                        foreach (var result in t.Result) {
                            if (!WasAdded(result)) continue;
                            var card = ReflectionUtil.GetMemberValue(result, "cardAdded") as CardModel;
                            if (card == null) continue;

                            lock (Sync) {
                                MarkedCards.Remove(card);
                                MarkedCards.Add(card, new RadiantPearlRef { Relic = relic });
                            }
                        }
                    } catch { }
                });
            } catch { }
        }

        internal static void CountLuminscencePlayed(CardModel card) {
            try {
                if (card == null) return;

                RadiantPearlRef? source;
                lock (Sync) {
                    if (!MarkedCards.TryGetValue(card, out source)) return;
                    MarkedCards.Remove(card);
                }

                if (source?.Relic == null) return;
                RelicTracker.AddAmount(source.Relic, "Luminscence Played", 1);
            } catch { }
        }

        static bool WasAdded(CardPileAddResult result) {
            try {
                var success = ReflectionUtil.GetMemberValue(result, "success");
                return success is bool value && value;
            } catch {
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.AddGeneratedCardsToCombat), new Type[] {
        typeof(IEnumerable<CardModel>),
        typeof(PileType),
        typeof(Player),
        typeof(CardPilePosition)
    })]
    public static class RadiantPearlGeneratedCardsPatch {
        static void Postfix(Task<IReadOnlyList<CardPileAddResult>> __result) {
            try {
                var relic = RadiantPearlPatch.Current;
                if (relic == null || __result == null) return;
                RadiantPearlPatch.MarkWhenAdded(relic, __result);
            } catch { }
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    public static class RadiantPearlCardPlayPatch {
        static void Postfix(CardModel __instance) {
            RadiantPearlPatch.CountLuminscencePlayed(__instance);
        }
    }
}
