using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Permafrost), nameof(Permafrost.AfterCardPlayed))]
    public static class PermafrostPatch {
        static readonly object Sync = new();
        static Permafrost? activeRelic;

        internal static Permafrost? ActiveRelic {
            get {
                lock (Sync) return activeRelic;
            }
        }

        static void Prefix(Permafrost __instance, CardPlay cardPlay) {
            try {
                if (__instance == null || cardPlay?.Card == null || cardPlay.Card.Owner != __instance.Owner) return;
                if (Convert.ToInt32(cardPlay.Card.Type) != 3) return;
                if ((bool?)ReflectionUtil.GetMemberValue(__instance, "ActivatedThisCombat") == true) return;

                lock (Sync) activeRelic = __instance;
            } catch { }
        }

        static void Postfix(Permafrost __instance, Task __result) {
            try {
                if (__result == null) {
                    Clear(__instance);
                    return;
                }

                __result.ContinueWith(_ => {
                    try { Clear(__instance); } catch { }
                });
            } catch { }
        }

        static void Clear(Permafrost relic) {
            lock (Sync) {
                if (ReferenceEquals(activeRelic, relic)) activeRelic = null;
            }
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.GainBlock), new Type[] {
        typeof(Creature),
        typeof(BlockVar),
        typeof(CardPlay),
        typeof(bool)
    })]
    public static class PermafrostGainBlockPatch {
        static void Postfix(Creature creature, Task<decimal> __result) {
            try {
                var relic = PermafrostPatch.ActiveRelic;
                if (relic == null || creature != relic.Owner?.Creature || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion) return;
                        var block = Math.Max(0, Convert.ToInt32(task.Result));
                        if (block > 0) RelicTracker.AddAmount(relic, "Block Gained", block);
                    } catch { }
                });
            } catch { }
        }
    }
}
