using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(StoneHumidifier), nameof(StoneHumidifier.AfterRestSiteHeal))]
    public static class StoneHumidifierPatch {
        class HealState {
            public int BeforeMaxHp { get; set; }
        }

        static void Prefix(StoneHumidifier __instance, Player player, bool isMimicked, ref object __state) {
            try {
                _ = isMimicked;
                if (__instance?.Owner == null || player != __instance.Owner) return;
                __state = new HealState { BeforeMaxHp = GetMaxHp(__instance.Owner.Creature) };
            } catch { }
        }

        static void Postfix(StoneHumidifier __instance, Task __result, object __state) {
            try {
                if (__state is not HealState state) return;

                if (__result == null) {
                    Count(__instance, state);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) Count(__instance, state);
                    } catch { }
                });
            } catch { }
        }

        static void Count(StoneHumidifier relic, HealState state) {
            try {
                var gained = Math.Max(0, GetMaxHp(relic.Owner?.Creature) - state.BeforeMaxHp);
                if (gained > 0) RelicTracker.AddAmount(relic, "Max HP Gained", gained);
            } catch { }
        }

        static int GetMaxHp(object? creature) {
            try {
                var raw = ReflectionUtil.GetMemberValue(creature, "MaxHp")
                    ?? ReflectionUtil.GetMemberValue(creature, "MaxHealth");
                return raw == null ? 0 : Convert.ToInt32(raw);
            } catch {
                return 0;
            }
        }
    }
}
