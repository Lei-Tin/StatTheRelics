using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(BoneFlute), nameof(BoneFlute.AfterAttack))]
    public static class BoneFlutePatch {
        class BoneFluteState {
            public int Block { get; set; }
        }

        static void Prefix(BoneFlute __instance, AttackCommand command, ref object __state) {
            try {
                if (__instance == null || command?.Attacker == null) return;
                if (command.Attacker.Monster?.GetType().FullName != "MegaCrit.Sts2.Core.Models.Monsters.Osty") return;
                if (command.Attacker.PetOwner?.Creature != __instance.Owner.Creature) return;

                __state = new BoneFluteState {
                    Block = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Block"))
                };
            } catch { }
        }

        static void Postfix(BoneFlute __instance, Task __result, object __state) {
            try {
                var state = __state as BoneFluteState;
                if (state == null || state.Block <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmount(__instance, "Block Given", state.Block);
                    return;
                }

                __result.ContinueWith(task => {
                    if (task.Status == TaskStatus.RanToCompletion) {
                        RelicTracker.AddAmount(__instance, "Block Given", state.Block);
                    }
                });
            } catch { }
        }
    }
}
