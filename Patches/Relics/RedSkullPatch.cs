using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch]
    public static class RedSkullPatch {
        class State {
            public bool WasApplied { get; set; }
            public int Strength { get; set; }
        }

        static MethodBase TargetMethod() {
            return AccessTools.DeclaredMethod(typeof(RedSkull), "ModifyStrengthIfNecessary");
        }

        static void Prefix(RedSkull __instance, ref object __state) {
            try {
                __state = new State {
                    WasApplied = ReflectionUtil.GetMemberValue(__instance, "StrengthApplied") is bool value && value,
                    Strength = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Strength", 3))
                };
            } catch { }
        }

        static void Postfix(RedSkull __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.WasApplied || state.Strength <= 0) return;
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

        static void Count(RedSkull relic, State state) {
            try {
                var isApplied = ReflectionUtil.GetMemberValue(relic, "StrengthApplied") is bool value && value;
                if (isApplied) RelicTracker.AddAmount(relic, "Strength Gained", state.Strength);
            } catch { }
        }
    }
}
