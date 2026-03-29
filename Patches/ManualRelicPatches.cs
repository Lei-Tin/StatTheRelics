using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace StatTheRelics.Patches {
    // Add explicit per-relic hooks here. These run after the broad dynamic patches.
    public static class ManualRelicPatches {
        class Hook {
            public string TypeName;
            public string MethodName;
            public string Label;
            public bool Prefix;
        }

        // Empty by default; add entries here for bespoke counting needs that the dynamic patcher misses.
        static readonly List<Hook> Hooks = new();

        public static void Apply(Harmony harmony) {
            foreach (var hook in Hooks) {
                try {
                    var type = AccessTools.TypeByName(hook.TypeName);
                    if (type == null) {
                        ModLog.Info($"ManualRelicPatches: type not found {hook.TypeName}");
                        continue;
                    }

                    var method = AccessTools.Method(type, hook.MethodName);
                    if (method == null) {
                        ModLog.Info($"ManualRelicPatches: method not found {hook.TypeName}.{hook.MethodName}");
                        continue;
                    }

                    var hm = new HarmonyMethod(typeof(ManualRelicPatches).GetMethod(hook.Prefix ? nameof(CountPrefix) : nameof(CountPostfix), BindingFlags.Static | BindingFlags.NonPublic));
                    harmony.Patch(method, prefix: hook.Prefix ? hm : null, postfix: hook.Prefix ? null : hm);
                    ModLog.Info($"ManualRelicPatches: patched {hook.TypeName}.{hook.MethodName} ({(hook.Prefix ? "prefix" : "postfix")})");
                } catch (Exception ex) {
                    ModLog.Info($"ManualRelicPatches: failed to patch {hook.TypeName}.{hook.MethodName} - {ex.Message}");
                }
            }
        }

        static void CountPrefix(object __instance, MethodBase __originalMethod) {
            try {
                RelicTracker.Increment(__instance, __originalMethod?.Name ?? "manual");
            } catch { }
        }

        static void CountPostfix(object __instance, MethodBase __originalMethod) {
            try {
                RelicTracker.Increment(__instance, __originalMethod?.Name ?? "manual");
            } catch { }
        }
    }
}
