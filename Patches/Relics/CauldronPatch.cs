using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Cauldron), nameof(Cauldron.AfterObtained))]
    public static class CauldronPatch {
        class CauldronState {
            public int Potions { get; set; }
        }

        static void Prefix(Cauldron __instance, ref object __state) {
            try {
                __state = new CauldronState {
                    Potions = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Potions"))
                };
            } catch { }
        }

        static void Postfix(Cauldron __instance, Task __result, object __state) {
            try {
                var state = __state as CauldronState;
                if (state == null || state.Potions <= 0) return;
                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Potions Offered", state.Potions);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) {
                        RelicTracker.AddAmount(__instance, "Potions Offered", state.Potions);
                    }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(Cauldron), "GenerateRewards")]
    public static class CauldronRewardsPatch {
        static void Postfix(Cauldron __instance, List<Reward> __result) {
            try {
                if (__instance == null || __result == null) return;
                foreach (var reward in __result) {
                    if (reward is PotionReward potionReward) {
                        CauldronPotionRewardPatch.Track(potionReward, __instance);
                    }
                }
            } catch { }
        }
    }

    [HarmonyPatch(typeof(PotionReward), nameof(PotionReward.Populate))]
    public static class CauldronPotionRewardPatch {
        static readonly ConditionalWeakTable<PotionReward, CauldronRef> RewardSources = new();

        class CauldronRef {
            public Cauldron? Relic { get; set; }
            public bool OfferedRecorded { get; set; }
        }

        internal static void Track(PotionReward reward, Cauldron relic) {
            try {
                RewardSources.Remove(reward);
                RewardSources.Add(reward, new CauldronRef { Relic = relic });
            } catch { }
        }

        static void Postfix(PotionReward __instance) {
            try {
                if (__instance == null) return;
                if (!RewardSources.TryGetValue(__instance, out var source)) return;
                if (source.Relic == null || __instance.Potion == null) return;

                CauldronPotionProcurePatch.Track(__instance.Potion, source.Relic);
                if (!source.OfferedRecorded) {
                    source.OfferedRecorded = true;
                    PotionNameUtil.AppendPotionName(source.Relic, "Potions Offered", PotionNameUtil.GetPotionName(__instance.Potion));
                }
            } catch { }
        }
    }

    [HarmonyPatch(typeof(PotionCmd), nameof(PotionCmd.TryToProcure), new Type[] {
        typeof(PotionModel),
        typeof(Player),
        typeof(int)
    })]
    public static class CauldronPotionProcurePatch {
        static readonly ConditionalWeakTable<PotionModel, CauldronRef> PotionSources = new();

        class CauldronRef {
            public Cauldron? Relic { get; set; }
        }

        class CauldronPotionState {
            public Cauldron? Relic { get; set; }
        }

        internal static void Track(PotionModel potion, Cauldron relic) {
            try {
                PotionSources.Remove(potion);
                PotionSources.Add(potion, new CauldronRef { Relic = relic });
            } catch { }
        }

        static void Prefix(PotionModel potion, Player player, ref object __state) {
            try {
                if (potion == null) return;
                if (!PotionSources.TryGetValue(potion, out var source)) return;
                if (source.Relic == null || player != source.Relic.Owner) return;

                __state = new CauldronPotionState { Relic = source.Relic };
            } catch { }
        }

        static void Postfix(PotionModel potion, Task<PotionProcureResult> __result, object __state) {
            try {
                var state = __state as CauldronPotionState;
                if (potion != null) PotionSources.Remove(potion);
                if (state?.Relic == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
                        if (!task.Result.success) return;

                        var title = PotionNameUtil.GetPotionName(task.Result.potion);
                        PotionNameUtil.AppendPotionName(state.Relic, "Selected Potions", title);
                    } catch { }
                });
            } catch { }
        }
    }
}
