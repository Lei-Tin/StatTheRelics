using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Shuriken), nameof(Shuriken.AfterCardPlayed))]
    public static class ShurikenPatch {
        static void Prefix(Shuriken __instance, CardPlay cardPlay, ref object __state) {
            try {
                var card = cardPlay?.Card;
                if (__instance == null || card == null || card.Owner != __instance.Owner) return;
                if (CombatManager.Instance?.IsInProgress != true) return;
                if (card.Type != CardType.Attack) return;

                var cardsNeeded = Math.Max(1, ReflectionUtil.GetDynamicVarIntValue(__instance, "Cards", 3));
                var attacksBefore = ReflectionUtil.GetIntMemberValue(__instance, "AttacksPlayedThisTurn", 0);
                if ((attacksBefore + 1) % cardsNeeded != 0) return;

                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Strength", 1));
            } catch { }
        }

        static void Postfix(Shuriken __instance, Task __result, object __state) {
            try {
                if (__state is not int strength || strength <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Strength Gained", strength);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Strength Gained", strength);
                    } catch { }
                });
            } catch { }
        }
    }
}
