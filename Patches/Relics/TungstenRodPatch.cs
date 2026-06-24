using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(TungstenRod), nameof(TungstenRod.ModifyHpLostAfterOsty))]
    public static class TungstenRodPatch {
        static void Postfix(TungstenRod __instance, Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource, decimal __result) {
            try {
                _ = props;
                _ = dealer;
                _ = cardSource;
                if (__instance == null || target == null || __instance.Owner?.Creature != target) return;
                var prevented = Math.Max(0, (int)(amount - __result));
                if (prevented > 0) RelicTracker.AddAmount(__instance, "HP Loss Prevented", prevented);
            } catch { }
        }
    }
}
