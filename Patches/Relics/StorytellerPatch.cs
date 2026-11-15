using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(Storyteller), nameof(Storyteller.AfterRoomEntered))]
    public static class StorytellerPatch {
        static void Prefix(Storyteller __instance, AbstractRoom room, out int __state) {
            __state = room is CombatRoom ? Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "AimPower")) : 0;
        }

        static void Postfix(Storyteller __instance, Task __result, int __state) {
            try {
                if (__state <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Aim Gained", __state);
                    } catch { }
                });
            } catch { }
        }
    }

    [HarmonyPatch(typeof(Storyteller), nameof(Storyteller.BeforeSideTurnEnd))]
    public static class StorytellerTurnEndPatch {
        static void Prefix(Storyteller __instance, IEnumerable<Creature> participants, out int __state) {
            __state = 0;
            try {
                if (participants.Contains(__instance.Owner.Creature)) {
                    __state = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "EndOfTurnAim"));
                }
            } catch { }
        }

        static void Postfix(Storyteller __instance, Task __result, int __state) {
            try {
                if (__state <= 0 || __result == null) return;
                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmount(__instance, "Aim Gained", __state);
                    } catch { }
                });
            } catch { }
        }
    }
}
