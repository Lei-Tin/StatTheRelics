using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Claws), nameof(Claws.AfterObtained))]
    public static class ClawsPatch {
        static Claws? Pending;

        static void Prefix(Claws __instance) {
            try {
                Pending = __instance;
            } catch { }
        }

        static void Postfix(Task __result) {
            try {
                if (__result == null) {
                    Pending = null;
                    return;
                }

                __result.ContinueWith(task => {
                    Pending = null;
                });
            } catch { }
        }

        internal static Claws? Current => Pending;
    }

    [HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Transform), new Type[] {
        typeof(IEnumerable<CardTransformation>),
        typeof(Rng),
        typeof(CardPreviewStyle)
    })]
    public static class ClawsTransformPatch {
        class ClawsTransformState {
            public Claws? Relic { get; set; }
        }

        static void Prefix(ref object __state) {
            try {
                var relic = ClawsPatch.Current;
                if (relic == null) return;
                __state = new ClawsTransformState { Relic = relic };
            } catch { }
        }

        static void Postfix(Task<IEnumerable<CardPileAddResult>> __result, object __state) {
            try {
                var state = __state as ClawsTransformState;
                if (state?.Relic == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;

                        var count = 0;
                        foreach (var result in task.Result) {
                            var success = ReflectionUtil.GetMemberValue(result, "success");
                            if (success is bool ok && ok) count++;
                        }

                        if (count > 0) RelicTracker.AddAmount(state.Relic, "Cards Transformed", count);
                    } catch { }
                });
            } catch { }
        }
    }
}
