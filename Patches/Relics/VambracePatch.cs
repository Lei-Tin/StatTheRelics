using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Vambrace), nameof(Vambrace.AfterModifyingBlockAmount))]
    public static class VambracePatch {
        static void Prefix(Vambrace __instance, decimal modifiedAmount, CardModel cardSource, CardPlay cardPlay, ref object __state) {
            try {
                _ = cardPlay;
                if (__instance == null || cardSource == null || cardSource.Owner != __instance.Owner) return;
                if (modifiedAmount <= 0) return;
                if (ReflectionUtil.GetMemberValue(__instance, "TriggeringCard") != null) return;
                if (ReflectionUtil.GetMemberValue(__instance, "BlockGainedThisCombat") is bool gained && gained) return;
                __state = Math.Max(0, (int)(modifiedAmount / 2m));
            } catch { }
        }

        static void Postfix(Vambrace __instance, object __state) {
            try {
                if (__state is not int bonusBlock || bonusBlock <= 0) return;
                RelicTracker.AddAmount(__instance, "Bonus Block Gained", bonusBlock);
            } catch { }
        }
    }
}
