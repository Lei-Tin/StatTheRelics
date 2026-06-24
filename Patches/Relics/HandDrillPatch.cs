using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(HandDrill), nameof(HandDrill.AfterDamageGiven))]
    public static class HandDrillPatch {
        class DrillState {
            public int Vulnerable { get; set; }
        }

        static void Prefix(HandDrill __instance, PlayerChoiceContext choiceContext, Creature dealer, DamageResult result, ValueProp props, Creature target, CardModel cardSource, ref object __state) {
            try {
                var owner = __instance?.Owner;
                var ownerCreature = owner?.Creature;
                if (__instance == null || owner == null || ownerCreature == null || result == null || target == null) return;
                if (dealer != ownerCreature && dealer?.PetOwner != owner) return;
                if (target.IsPlayer || !result.WasBlockBroken) return;

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
