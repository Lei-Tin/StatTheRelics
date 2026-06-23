using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ChemicalX), nameof(ChemicalX.BeforeCardPlayed))]
    public static class ChemicalXPatch {
        class ChemicalXState {
            public int Increase { get; set; }
        }

        static void Prefix(ChemicalX __instance, CardPlay cardPlay, ref object __state) {
            try {
                var card = cardPlay?.Card;
                if (__instance == null || card?.Owner != __instance.Owner) return;
                if (!card.EnergyCost.CostsX && !card.HasStarCostX) return;

                __state = new ChemicalXState {
                    Increase = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Increase"))
                };
            } catch { }
        }

        static void Postfix(ChemicalX __instance, Task __result, object __state) {
            try {
                var state = __state as ChemicalXState;
                if (state == null || state.Increase <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "X Cards Played", 1);
                    RelicTracker.AddAmount(__instance, "X Value Added", state.Increase);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) {
                        RelicTracker.AddAmount(__instance, "X Cards Played", 1);
                        RelicTracker.AddAmount(__instance, "X Value Added", state.Increase);
                    }
                });
            } catch { }
        }
    }
}
