using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(DarkstonePeriapt), nameof(DarkstonePeriapt.AfterCardChangedPiles))]
    public static class DarkstonePeriaptPatch {
        class DarkstoneState {
            public int MaxHp { get; set; }
        }

        static void Prefix(DarkstonePeriapt __instance, CardModel card, PileType oldPileType, AbstractModel clonedBy, ref object __state) {
            try {
                if (__instance == null || card == null) return;
                if (card.Owner != __instance.Owner) return;
                if (card.Pile == null || Convert.ToInt32(card.Pile.Type) != 6) return;
                if (Convert.ToInt32(card.Type) != 5) return;

                var maxHp = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "MaxHp", 6));
                if (maxHp <= 0) return;
                __state = new DarkstoneState { MaxHp = maxHp };
            } catch { }
        }

        static void Postfix(DarkstonePeriapt __instance, Task __result, object __state) {
            try {
                var state = __state as DarkstoneState;
                if (state == null || state.MaxHp <= 0) return;

                if (__result == null) {
                    Count(__instance, state.MaxHp);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, state.MaxHp);
                    } catch { }
                });
            } catch { }
        }

        static void Count(DarkstonePeriapt relic, int maxHp) {
            RelicTracker.AddAmount(relic, "Max HP Gained", maxHp);
            RelicTracker.AddAmount(relic, "Curses Added", 1);
        }
    }
}
