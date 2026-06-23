using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch]
    public static class BookmarkPatch {
        static MethodBase TargetMethod() {
            return AccessTools.Method(typeof(Bookmark), "AfterFlush")
                ?? throw new MissingMethodException("Bookmark.AfterFlush not found");
        }

        static void Prefix(Bookmark __instance, Player player, IReadOnlyCollection<CardModel> retainedCards, ref bool __state) {
            try {
                __state = false;
                if (__instance == null || player == null || retainedCards == null) return;
                if (__instance.Owner != player) return;

                __state = retainedCards
                    .Any(card => card != null
                        && card.EnergyCost.GetWithModifiers(CostModifiers.Local) > 0
                        && !card.EnergyCost.CostsX);
            } catch { }
        }

        static void Postfix(Task __result, Bookmark __instance, bool __state) {
            try {
                if (__instance == null || !__state) return;

                if (__result == null || __result.IsCompletedSuccessfully) {
                    RelicTracker.AddAmount(__instance, "Cost Decreased", 1);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) {
                        RelicTracker.AddAmount(__instance, "Cost Decreased", 1);
                    }
                });
            } catch { }
        }
    }
}
