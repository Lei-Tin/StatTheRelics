using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PaelsLegion), nameof(PaelsLegion.AfterModifyingBlockAmount))]
    public static class PaelsLegionPatch {
        class BlockState {
            public bool ShouldCount { get; set; }
            public int BonusBlock { get; set; }
        }

        static void Prefix(PaelsLegion __instance, decimal modifiedAmount, CardModel cardSource, CardPlay cardPlay, ref object __state) {
            try {
                if (__instance == null || modifiedAmount <= 0 || cardSource == null || cardPlay == null) return;
                if (cardSource.Owner != __instance.Owner) return;
                if (ReflectionUtil.GetIntMemberValue(__instance, "Cooldown", 0) > 0) return;
                var affectedCardPlay = ReflectionUtil.GetMemberValue(__instance, "AffectedCardPlay");
                if (affectedCardPlay != null && !ReferenceEquals(affectedCardPlay, cardPlay)) return;

                __state = new BlockState {
                    ShouldCount = affectedCardPlay == null,
                    BonusBlock = Math.Max(0, Convert.ToInt32(Math.Floor(modifiedAmount / 2m)))
                };
            } catch { }
        }

        static void Postfix(PaelsLegion __instance, Task __result, object __state) {
            try {
                if (__state is not BlockState state || !state.ShouldCount || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            RelicTracker.AddAmount(__instance, "Times Activated", 1);
                            if (state.BonusBlock > 0) RelicTracker.AddAmount(__instance, "Bonus Block Gained", state.BonusBlock);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
