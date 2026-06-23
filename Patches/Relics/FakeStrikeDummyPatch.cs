using System;
using System.Diagnostics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(FakeStrikeDummy), nameof(FakeStrikeDummy.ModifyDamageAdditive))]
    public static class FakeStrikeDummyPatch {
        static void Postfix(FakeStrikeDummy __instance, Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource, decimal __result) {
            try {
                if (__instance == null || __result <= 0) return;
                if (!IsFromCreatureDamageCommand()) return;

                var bonus = Math.Max(0, Convert.ToInt32(__result));
                if (bonus > 0) RelicTracker.AddAmount(__instance, "Bonus Damage", bonus);
            } catch { }
        }

        static bool IsFromCreatureDamageCommand() {
            try {
                var frames = new StackTrace().GetFrames();
                if (frames == null) return false;
                foreach (var frame in frames) {
                    var typeName = frame.GetMethod()?.DeclaringType?.FullName;
                    if (typeName != null && typeName.Contains("MegaCrit.Sts2.Core.Commands.CreatureCmd", StringComparison.Ordinal)) return true;
                }
            } catch { }

            return false;
        }
    }
}
