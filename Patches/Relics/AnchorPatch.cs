using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using StatTheRelics;

namespace StatTheRelics.Patches.Relics {
    // Track actual block granted by Anchor at the start of combat (after async gain completes).
    [HarmonyPatch(typeof(Anchor), nameof(Anchor.BeforeCombatStart))]
    public static class AnchorPatch {
        class BlockState { public object? Creature { get; set; } public int Before { get; set; } }

        static void Prefix(Anchor __instance, ref object __state) {
            try {
                var creature = __instance?.Owner?.Creature;
                var beforeBlock = GetBlock(creature);
                __state = new BlockState { Creature = creature, Before = beforeBlock };
                ModLog.Info($"AnchorPatch: Prefix creature={creature?.GetType().FullName ?? "null"}, beforeBlock={beforeBlock}");
            } catch { }
        }

        static void Postfix(Anchor __instance, object __state) {
            try {
                var creature = ( __state as BlockState )?.Creature ?? __instance?.Owner?.Creature;
                var beforeBlock = ( __state as BlockState )?.Before ?? GetBlock(creature);
                var afterBlock = GetBlock(creature);
                var gained = Math.Max(0, afterBlock - beforeBlock);
                ModLog.Info($"AnchorPatch: Postfix creature={creature?.GetType().FullName ?? "null"}, beforeBlock={beforeBlock}, afterBlock={afterBlock}, gained={gained}");
                if (gained > 0 && __instance != null) RelicTracker.AddAmount(__instance, "Block Generated", gained);
            } catch { }
        }

        static int GetBlock(object? creature) {
            try {
                if (creature == null) return 0;
                var type = creature.GetType();
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                var prop = type.GetProperty("Block", flags) ?? type.GetProperty("CurrentBlock", flags);
                ModLog.Info($"AnchorPatch: block via property -> prop={prop?.GetValue(creature)}");
                if (prop != null) return Convert.ToInt32(prop.GetValue(creature));
            } catch {
                ModLog.Info("AnchorPatch: failed to get block via property");
            }
            return 0;
        }
    }
}
