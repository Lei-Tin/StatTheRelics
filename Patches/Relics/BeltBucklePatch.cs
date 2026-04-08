using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Count how many times Belt Buckle actually applies Dexterity.
    [HarmonyPatch(typeof(BeltBuckle), "ApplyDexterity")]
    public static class BeltBucklePatch {
        static void Prefix(BeltBuckle __instance) {
            try {
                var alreadyAppliedObj = ReflectionUtil.GetMemberValue(__instance, "DexterityApplied");
                var alreadyApplied = alreadyAppliedObj != null && Convert.ToBoolean(alreadyAppliedObj);
                if (alreadyApplied) return;

                RelicTracker.AddAmount(__instance, "Times Dexterity Applied", 1);
                ModLog.Info("BeltBucklePatch: counted Times Dexterity Applied +1");
            } catch { }
        }
    }
}
