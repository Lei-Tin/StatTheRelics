using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(UnsettlingLamp), nameof(UnsettlingLamp.BeforePowerAmountChanged))]
    public static class UnsettlingLampPatch {
        class State {
            public bool Triggered { get; set; }
        }

        static void Prefix(UnsettlingLamp __instance, PowerModel power, decimal amount, Creature target, Creature applier, CardModel cardSource, ref object __state) {
            try {
                if (__instance == null || power == null || target == null || applier == null || cardSource == null) return;
                if (ReflectionUtil.GetMemberValue(__instance, "TriggeringCard") != null) return;
                if (ReflectionUtil.GetMemberValue(__instance, "IsFinishedTriggering") is bool finished && finished) return;
                if (__instance.Owner?.Creature != applier) return;
                if (target.Side == __instance.Owner.Creature.Side) return;
                if (!power.IsVisible) return;
                if (power.GetTypeForAmount(amount).ToString() != "Debuff") return;

                __state = new State { Triggered = true };
            } catch { }
        }

        static void Postfix(UnsettlingLamp __instance, CardModel cardSource, object __state) {
            try {
                if (__state is not State state || !state.Triggered) return;
                if (!ReferenceEquals(ReflectionUtil.GetMemberValue(__instance, "TriggeringCard"), cardSource)) return;
                RelicTracker.AddAmount(__instance, "Debuffs Doubled", 1);
            } catch { }
        }
    }
}
