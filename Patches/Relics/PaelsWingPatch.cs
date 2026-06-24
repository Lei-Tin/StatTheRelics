using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(PaelsWing), nameof(PaelsWing.OnSacrifice))]
    public static class PaelsWingPatch {
        class State {
            public Dictionary<string, int> BeforeRelics { get; set; } = new(StringComparer.Ordinal);
        }

        static void Prefix(PaelsWing __instance, ref object __state) {
            try {
                __state = new State {
                    BeforeRelics = CaptureRelics(__instance)
                };
            } catch { }
        }

        static void Postfix(PaelsWing __instance, Task __result, object __state) {
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

        static void Count(PaelsWing relic, State state) {
            try {
                var added = FindAdded(state.BeforeRelics, CaptureRelics(relic));
                if (added.Count <= 0) return;

                var current = RelicTracker.GetText(relic, "Relics Given");
                var value = string.Join("\n", added);
                RelicTracker.SetText(relic, "Relics Given", string.IsNullOrWhiteSpace(current) ? value : current + "\n" + value);
            } catch { }
        }

        static Dictionary<string, int> CaptureRelics(PaelsWing relic) {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            var relics = relic.Owner?.Relics;
            if (relics == null) return result;

            foreach (var ownedRelic in relics) {
                if (ownedRelic == null) continue;
                var name = ReflectionUtil.GetModelTitle(ownedRelic) ?? ownedRelic.GetType().Name;
                result[name] = result.TryGetValue(name, out var count) ? count + 1 : 1;
            }
            return result;
        }

        static List<string> FindAdded(IReadOnlyDictionary<string, int> before, IReadOnlyDictionary<string, int> after) {
            var added = new List<string>();
            foreach (var kv in after) {
                var beforeCount = before.TryGetValue(kv.Key, out var count) ? count : 0;
                for (var i = beforeCount; i < kv.Value; i++) added.Add(kv.Key);
            }

            added.Sort(StringComparer.OrdinalIgnoreCase);
            return added;
        }
    }
}
