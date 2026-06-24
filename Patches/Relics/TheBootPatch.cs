using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(TheBoot), nameof(TheBoot.ModifyHpLostAfterOstyLate))]
    public static class TheBootPatch {
        static void Postfix(TheBoot __instance, Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource, decimal __result) {
            try {
                _ = cardSource;
                if (__instance?.Owner?.Creature == null || target == null || dealer == null) return;
                if (dealer != __instance.Owner.Creature && dealer != __instance.Owner.Osty) return;
                if (target == __instance.Owner.Creature) return;
                if (!props.IsPoweredAttack()) return;

                var bonus = Math.Max(0, Convert.ToInt32(Math.Floor(__result - amount)));
                if (bonus > 0) RelicTracker.AddAmount(__instance, "Bonus Damage Dealt", bonus);
            } catch { }
        }
    }
}
