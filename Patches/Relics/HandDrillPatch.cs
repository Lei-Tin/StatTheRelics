using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch]
    [PatchTargetAlternative(typeof(HandDrill), "AfterBlockBroken")]
    [PatchTargetAlternative(typeof(HandDrill), "AfterDamageGiven")]
    public static class HandDrillPatch {
        static MethodBase TargetMethod() => PatchTargetResolver.RequireAny(
            typeof(HandDrill),
            new PatchTargetCandidate("AfterBlockBroken"),
            new PatchTargetCandidate("AfterDamageGiven")
        );

        class DrillState {
            public int Vulnerable { get; set; }
        }

        static void Prefix(HandDrill __instance, MethodBase __originalMethod, object[] __args, ref object __state) {
            try {
                var owner = __instance?.Owner;
                var ownerCreature = owner?.Creature;
                if (__instance == null || owner == null || ownerCreature == null) return;

                Creature? target;
                Creature? breaker;
                if (__originalMethod.Name == "AfterBlockBroken") {
                    target = __args.Length > 1 ? __args[1] as Creature : null;
                    breaker = __args.Length > 2 ? __args[2] as Creature : null;
                } else {
                    breaker = __args.Length > 1 ? __args[1] as Creature : null;
                    var result = __args.Length > 2 ? __args[2] as DamageResult : null;
                    target = __args.Length > 4 ? __args[4] as Creature : null;
                    if (result == null || !result.WasBlockBroken) return;
                }

                if (target == null || breaker == null) return;
                if (breaker != ownerCreature && breaker.PetOwner != owner) return;
                if (target.IsPlayer) return;

                __state = new DrillState {
                    Vulnerable = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "Vulnerable", 2))
                };
            } catch { }
        }

        static void Postfix(HandDrill __instance, Task __result, object __state) {
            try {
                var state = __state as DrillState;
                if (state == null || state.Vulnerable <= 0) return;

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

        static void Count(HandDrill relic, DrillState state) {
            RelicTracker.AddAmount(relic, "Vulnerable Applied", state.Vulnerable);
        }
    }
}
