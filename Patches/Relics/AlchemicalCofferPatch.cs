using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(AlchemicalCoffer), nameof(AlchemicalCoffer.AfterObtained))]
    public static class AlchemicalCofferPatch {
        static AlchemicalCoffer? Current;

        static void Prefix(AlchemicalCoffer __instance) {
            Current = __instance;
        }

        static void Postfix(Task __result) {
            try {
                if (__result == null) {
                    Current = null;
                    return;
                }

                __result.ContinueWith(_ => Current = null);
            } catch {
                Current = null;
            }
        }

        internal static AlchemicalCoffer? Active => Current;
    }

    [HarmonyPatch(typeof(PotionCmd), nameof(PotionCmd.TryToProcure), new Type[] {
        typeof(PotionModel),
        typeof(Player),
        typeof(int)
    })]
    public static class AlchemicalCofferPotionProcurePatch {
        class AlchemicalCofferPotionState {
            public AlchemicalCoffer? Relic { get; set; }
        }

        static void Prefix(PotionModel potion, Player player, ref object __state) {
            try {
                var relic = AlchemicalCofferPatch.Active;
                if (relic == null || player != relic.Owner) return;
                __state = new AlchemicalCofferPotionState { Relic = relic };
            } catch { }
        }

        static void Postfix(Task<PotionProcureResult> __result, object __state) {
            try {
                var state = __state as AlchemicalCofferPotionState;
                if (state?.Relic == null || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
                        if (!task.Result.success) return;

                        var potion = task.Result.potion;
                        var title = PotionNameUtil.GetPotionName(potion);
                        PotionNameUtil.AppendPotionName(state.Relic, "Potions", title);
                    } catch { }
                });
            } catch { }
        }
    }

    internal static class PotionNameUtil {
        static readonly object TextLock = new();

        public static string GetPotionName(PotionModel? potion) {
            try {
                if (potion == null) return "Unknown";

                var title = potion.Title;
                var formatted = title?.GetFormattedText();
                if (!string.IsNullOrWhiteSpace(formatted)) return formatted;

                var raw = title?.GetRawText();
                if (!string.IsNullOrWhiteSpace(raw)) return raw;

                var modelTitle = ReflectionUtil.GetModelTitle(potion);
                if (!string.IsNullOrWhiteSpace(modelTitle)) return modelTitle;

                var id = ReflectionUtil.GetMemberValue(potion, "Id");
                var idText = id?.ToString();
                if (!string.IsNullOrWhiteSpace(idText)) return idText;
            } catch { }

            return potion?.GetType().Name ?? "Unknown";
        }

        public static void AppendPotionName(object relic, string key, string name) {
            try {
                lock (TextLock) {
                    var existing = RelicTracker.GetText(relic, key);
                    var names = ParsePotionList(existing);

                    names.Add(name);
                    RelicTracker.SetText(relic, key, FormatPotionList(names));
                }
            } catch { }
        }

        public static void SetPotionNames(object relic, string key, IEnumerable<string> names) {
            try {
                lock (TextLock) {
                    var list = names
                        .Where(n => !string.IsNullOrWhiteSpace(n))
                        .Select(n => n.Trim())
                        .ToList();
                    if (list.Count == 0) return;

                    RelicTracker.SetText(relic, key, FormatPotionList(list));
                }
            } catch { }
        }

        static List<string> ParsePotionList(string? existing) {
            var names = new List<string>();
            if (string.IsNullOrWhiteSpace(existing)) return names;

            names.AddRange(existing
                .Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(StripListNumber)
                .Where(n => !string.IsNullOrWhiteSpace(n)));

            return names;
        }

        static string FormatPotionList(IEnumerable<string> names) {
            return string.Join("\n", names);
        }

        static string StripListNumber(string value) {
            try {
                var text = value.Trim();
                var dot = text.IndexOf(". ", StringComparison.Ordinal);
                if (dot <= 0) return text;

                for (var i = 0; i < dot; i++) {
                    if (!char.IsDigit(text[i])) return text;
                }

                return text[(dot + 2)..].Trim();
            } catch {
                return value;
            }
        }
    }
}
