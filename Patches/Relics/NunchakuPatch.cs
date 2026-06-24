using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Nunchaku), nameof(Nunchaku.AfterCardPlayed))]
    public static class NunchakuPatch {
        static void Prefix(Nunchaku __instance, CardPlay cardPlay, ref object __state) {
            try {
                if (__instance == null || cardPlay?.Card == null || cardPlay.Card.Owner != __instance.Owner) return;
                if (cardPlay.Card.Type != CardType.Attack) return;
                if (!CombatManager.Instance.IsInProgress) return;

                var threshold = Math.Max(1, ReflectionUtil.GetDynamicVarIntValue(__instance, "Cards", 10));
                var attacksPlayed = ReflectionUtil.GetIntMemberValue(__instance, "AttacksPlayed");
                if ((attacksPlayed + 1) % threshold != 0) return;

                __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Energy", 1));
            } catch { }
        }

        static void Postfix(Nunchaku __instance, Task __result, object __state) {
            try {
                var amount = __state is int value ? value : 0;
                if (amount <= 0 || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Energy Gained", amount);
                    } catch { }
                });
            } catch { }
        }
    }
}
