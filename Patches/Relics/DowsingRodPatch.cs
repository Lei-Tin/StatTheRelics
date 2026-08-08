using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using StatTheRelics.Patches;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch]
    [VersionOptionalPatch]
    public static class DowsingRodPatch {
        const string CardTypeName = "MegaCrit.Sts2.Core.Models.Cards.Abundance";
        const string RelicTypeName = "MegaCrit.Sts2.Core.Models.Relics.DowsingRod";

        static Type? CardType => AccessTools.TypeByName(CardTypeName);

        static bool Prepare() => CardType != null;

        static MethodBase TargetMethod() {
            return PatchTargetResolver.RequireAny(
                CardType!,
                new PatchTargetCandidate("OnPlay", typeof(PlayerChoiceContext), typeof(CardPlay))
            );
        }

        static void Postfix(object __instance, Task __result) {
            try {
                if (__result == null) return;
                var owner = ReflectionUtil.GetMemberValue(__instance, "Owner");
                var relic = ReflectionUtil.FindRelicByTypeName(owner, RelicTypeName);
                if (relic == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            RelicTracker.AddAmount(relic, "Abundance Played", 1);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
