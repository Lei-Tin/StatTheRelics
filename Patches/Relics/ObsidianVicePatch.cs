using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ObsidianVice), nameof(ObsidianVice.BeforeSideTurnEnd))]
    public static class ObsidianVicePatch {
        [ThreadStatic] internal static ObsidianVice? Current;

        static void Prefix(ObsidianVice __instance, IEnumerable<Creature> participants) {
            if (participants.Contains(__instance.Owner.Creature)) Current = __instance;
        }

        static void Postfix() { Current = null; }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new[] {
        typeof(PlayerChoiceContext), typeof(Creature), typeof(decimal), typeof(ValueProp),
        typeof(Creature), typeof(CardModel), typeof(CardPlay)
    })]
    public static class ObsidianViceDamagePatch {
        static void Postfix(Creature dealer, Task<IEnumerable<DamageResult>> __result) {
            try {
                var relic = ObsidianVicePatch.Current;
                if (relic == null || dealer != relic.Owner.Creature || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        var total = task.Result.Sum(result => Math.Max(0, result.TotalDamage));
                        if (total > 0) RelicTracker.AddAmount(relic, "Damage Dealt", total);
                    } catch { }
                });
            } catch { }
        }
    }
}
