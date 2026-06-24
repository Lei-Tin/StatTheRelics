using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(ElectricShrymp), nameof(ElectricShrymp.AfterObtained))]
    public static class ElectricShrympPatch {
        internal const string TypeName = "MegaCrit.Sts2.Core.Models.Relics.ElectricShrymp";
        static ElectricShrymp? Current;
        static readonly object CountLock = new();
        static readonly ConditionalWeakTable<ElectricShrymp, CountMarker> AutoPlayCountedRelics = new();

        sealed class CountMarker { }

        static void Prefix(ElectricShrymp __instance) {
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

        internal static ElectricShrymp? Active => Current;

        internal static bool HasAutoPlayCounted(ElectricShrymp relic) {
            try {
                lock (CountLock) {
                    return AutoPlayCountedRelics.TryGetValue(relic, out _);
                }
            } catch {
                return false;
            }
        }

        internal static bool MarkAutoPlayCounted(ElectricShrymp relic) {
            try {
                lock (CountLock) {
                    if (AutoPlayCountedRelics.TryGetValue(relic, out _)) return false;
                    AutoPlayCountedRelics.Add(relic, new CountMarker());
                    return true;
                }
            } catch {
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromDeckForEnchantment), new Type[] {
        typeof(Player),
        typeof(EnchantmentModel),
        typeof(int),
        typeof(CardSelectorPrefs)
    })]
    public static class ElectricShrympSelectionPatch {
        static void Postfix(Player player, Task<IEnumerable<CardModel>> __result) {
            try {
                var relic = ElectricShrympPatch.Active;
                if (relic == null || player == null || relic.Owner != player || __result == null) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status != TaskStatus.RanToCompletion || task.Result == null) return;
                        var names = task.Result
                            .Select(card => DeckUtil.GetCardDisplayName(card, preferBaseTitle: true))
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .ToList();
                        if (names.Count <= 0) return;

                        RelicTracker.SetText(relic, "Card Enchanted", DeckUtil.JoinCardList(names));
                    } catch { }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(Imbued), nameof(Imbued.AfterAutoPrePlayPhaseEntered))]
    public static class ElectricShrympImbuedAutoPlayPatch {
        static void Prefix(Imbued __instance, PlayerChoiceContext choiceContext, Player player, ref object __state) {
            try {
                if (__instance == null || player == null) return;
                if (!RelicTracker.HasTrackedRelicType(ElectricShrympPatch.TypeName)) return;
                var relic = ReflectionUtil.FindRelic<ElectricShrymp>(player);
                if (relic == null) return;
                if (ElectricShrympPatch.HasAutoPlayCounted(relic)) return;

                var card = ReflectionUtil.GetMemberValue(__instance, "Card") as CardModel;
                if (card == null || card.Owner != player) return;

                var tracked = RelicTracker.GetText(relic, "Card Enchanted");
                var cardName = DeckUtil.GetCardMatchName(card);
                if (string.IsNullOrWhiteSpace(tracked) || string.IsNullOrWhiteSpace(cardName)) return;
                if (!tracked.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Any(name => string.Equals(DeckUtil.NormalizeCardNameForMatching(name), cardName, StringComparison.OrdinalIgnoreCase))) return;

                __state = relic;
            } catch { }
        }

        static void Postfix(Task __result, object __state) {
            try {
                if (__state is not ElectricShrymp relic) return;
                if (__result == null) {
                    CountOnce(relic);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) CountOnce(relic);
                    } catch { }
                });
            } catch { }
        }

        static void CountOnce(ElectricShrymp relic) {
            try {
                if (!ElectricShrympPatch.MarkAutoPlayCounted(relic)) return;
                RelicTracker.AddAmount(relic, "Auto Played Card", 1);
            } catch { }
        }
    }
}
