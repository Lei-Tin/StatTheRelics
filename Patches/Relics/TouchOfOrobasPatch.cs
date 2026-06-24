using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.AfterObtained))]
    public static class TouchOfOrobasPatch {
        class State {
            public string? Starter { get; set; }
            public string? Upgraded { get; set; }
        }

        static void Prefix(TouchOfOrobas __instance, ref object __state) {
            try {
                if (__instance?.Owner == null) return;
                var starter = Invoke(__instance, "GetStarterRelic", __instance.Owner);
                if (starter == null) return;
                var upgraded = Invoke(__instance, "GetUpgradedStarterRelic", starter);
                __state = new State {
                    Starter = ReflectionUtil.GetModelTitle(starter) ?? starter.GetType().Name,
                    Upgraded = ReflectionUtil.GetModelTitle(upgraded) ?? upgraded?.GetType().Name
                };
            } catch { }
        }

        static void Postfix(TouchOfOrobas __instance, Task __result, object __state) {
            try {
                if (__state is not State state) return;

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

        static void Count(TouchOfOrobas relic, State state) {
            try {
                if (!string.IsNullOrWhiteSpace(state.Starter)) RelicTracker.SetText(relic, "Starter Relic", state.Starter);
                if (!string.IsNullOrWhiteSpace(state.Upgraded)) RelicTracker.SetText(relic, "Upgraded Relic", state.Upgraded);
            } catch { }
        }

        static object? Invoke(TouchOfOrobas relic, string methodName, params object[] args) {
            try {
                var method = typeof(TouchOfOrobas).GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return method?.Invoke(relic, args);
            } catch {
                return null;
            }
        }
    }
}
