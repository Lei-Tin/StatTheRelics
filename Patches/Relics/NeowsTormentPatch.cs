using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch]
    public static class NeowsTormentPatch {
        static MethodBase TargetMethod() {
            return AccessTools.DeclaredMethod(typeof(NeowsFury), "OnPlay", new Type[] {
                typeof(PlayerChoiceContext),
                typeof(CardPlay)
            });
        }

        static void Postfix(NeowsFury __instance, Task __result) {
            try {
                if (__instance?.Owner == null || __result == null) return;
                var relic = ReflectionUtil.FindRelic<NeowsTorment>(__instance.Owner);
                if (relic == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            RelicTracker.AddAmount(relic, "Neow's Fury Played", 1);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
