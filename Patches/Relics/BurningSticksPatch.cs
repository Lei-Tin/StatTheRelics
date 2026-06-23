using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    // Log the skill-card exhaust checks that Burning Sticks uses before cloning into the draw pile.
    [HarmonyPatch(typeof(BurningSticks), nameof(BurningSticks.AfterCardExhausted))]
    public static class BurningSticksPatch {
        class BurningSticksState {
            public bool WasUsedBefore { get; set; }
            public bool ShouldTrigger { get; set; }
            public string CardName { get; set; } = string.Empty;
            public CardType CardType { get; set; }
        }

        static void Prefix(BurningSticks __instance, PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal, ref object __state) {
            try {
                var wasUsed = GetWasUsedThisCombat(__instance);
                var shouldTrigger = __instance != null
                    && card != null
                    && card.Owner == __instance.Owner
                    && !wasUsed
                    && card.Type == CardType.Skill;

                __state = new BurningSticksState {
                    WasUsedBefore = wasUsed,
                    ShouldTrigger = shouldTrigger,
                    CardName = card != null ? DeckUtil.GetCardDisplayName(card, preferBaseTitle: true) : "null",
                    CardType = card != null ? card.Type : default
                };

                ModLog.Info($"BurningSticksPatch: Prefix card={((BurningSticksState)__state).CardName}, type={((BurningSticksState)__state).CardType}, wasUsed={wasUsed}, shouldTrigger={shouldTrigger}");
            } catch { }
        }

        static void Postfix(BurningSticks __instance, Task __result, object __state) {
            try {
                var state = __state as BurningSticksState;
                if (state == null) return;

                if (__result == null) {
                    LogAfter(__instance, state, TaskStatus.RanToCompletion);
                    return;
                }

                __result.ContinueWith(task => LogAfter(__instance, state, task.Status));
            } catch { }
        }

        static void LogAfter(BurningSticks relic, BurningSticksState state, TaskStatus status) {
            try {
                var wasUsedAfter = GetWasUsedThisCombat(relic);
                ModLog.Info($"BurningSticksPatch: Postfix card={state.CardName}, type={state.CardType}, taskStatus={status}, wasUsedBefore={state.WasUsedBefore}, wasUsedAfter={wasUsedAfter}, shouldTrigger={state.ShouldTrigger}");
            } catch { }
        }

        internal static bool GetWasUsedThisCombat(object? relic) {
            try {
                var raw = ReflectionUtil.GetMemberValue(relic, "WasUsedThisCombat");
                return raw is bool used && used;
            } catch {
                return false;
            }
        }
    }

    // Count the actual Burning Sticks trigger: the relic marks itself used only after adding the cloned card.
    [HarmonyPatch(typeof(BurningSticks), "WasUsedThisCombat", MethodType.Setter)]
    public static class BurningSticksWasUsedSetterPatch {
        static void Prefix(BurningSticks __instance, bool value, ref object __state) {
            try {
                __state = BurningSticksPatch.GetWasUsedThisCombat(__instance);
            } catch { }
        }

        static void Postfix(BurningSticks __instance, bool value, object __state) {
            try {
                var wasUsedBefore = __state is bool b && b;
                if (wasUsedBefore || !value) return;

                RelicTracker.AddAmount(__instance, "Cards Generated", 1);
                ModLog.Info("BurningSticksWasUsedSetterPatch: WasUsedThisCombat false -> true, counted generated card");
            } catch { }
        }
    }
}
