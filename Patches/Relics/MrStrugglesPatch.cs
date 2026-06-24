using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(MrStruggles), nameof(MrStruggles.AfterPlayerTurnStart))]
    public static class MrStrugglesPatch {
        [ThreadStatic] internal static MrStruggles? Current;

        static void Prefix(MrStruggles __instance, Player player) {
            try {
                if (__instance == null || player == null || __instance.Owner != player) return;
                Current = __instance;
            } catch { }
        }

        static void Postfix() {
            Current = null;
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage), new Type[] {
        typeof(PlayerChoiceContext),
        typeof(IEnumerable<Creature>),
        typeof(decimal),
        typeof(ValueProp),
        typeof(Creature)
    })]
    public static class MrStrugglesDamagePatch {
        static void Postfix(Task<IEnumerable<DamageResult>> __result) {
            try {
                var relic = MrStrugglesPatch.Current;
                if (relic == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
                        var total = task.Result.Sum(result => result == null ? 0 : Math.Max(0, result.TotalDamage));
                        if (total > 0) RelicTracker.AddAmount(relic, "Damage Dealt", total);
                    } catch { }
                });
            } catch { }
        }
    }
}
