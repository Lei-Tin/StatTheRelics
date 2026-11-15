using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Wuppy), nameof(Wuppy.AfterCurrentHpChanged))]
    public static class WuppyPatch {
        static void Postfix(Wuppy __instance, Creature creature, decimal delta) {
            try {
                if (creature == __instance.CombatWuppy && delta < 0) RelicTracker.AddAmount(__instance, "HP Lost", (int)-delta);
            } catch { }
        }
    }
}
