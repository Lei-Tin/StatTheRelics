using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using StatTheRelics.Patches;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch]
    [VersionOptionalPatch]
    public static class NeowsSacrificeAmbergrisPatch {
        const string PotionTypeName = "MegaCrit.Sts2.Core.Models.Potions.Ambergris";
        const string RelicTypeName = "MegaCrit.Sts2.Core.Models.Relics.NeowsSacrifice";
        static Type? PotionType => AccessTools.TypeByName(PotionTypeName);

        sealed class State {
            public object Relic { get; init; } = null!;
            public Creature Target { get; init; } = null!;
            public int HpBefore { get; init; }
        }

        static bool Prepare() => PotionType != null;

        static MethodBase TargetMethod() {
            return PatchTargetResolver.RequireAny(
                PotionType!,
                new PatchTargetCandidate("OnUse", typeof(PlayerChoiceContext), typeof(Creature))
            );
        }

        static void Prefix(object __instance, Creature target, ref object __state) {
            try {
                if (target == null) return;
                var owner = ReflectionUtil.GetMemberValue(__instance, "Owner");
                var relic = ReflectionUtil.FindRelicByTypeName(owner, RelicTypeName);
                if (relic == null) return;

                __state = new State {
                    Relic = relic,
                    Target = target,
                    HpBefore = target.CurrentHp
                };
            } catch { }
        }

        static void Postfix(Task __result, object __state) {
            try {
                if (__result == null || __state is not State state) return;

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) {
                            var healed = Math.Max(0, state.Target.CurrentHp - state.HpBefore);
                            if (healed > 0) RelicTracker.AddAmount(state.Relic, "HP Healed", healed);
                        }
                    } catch { }
                });
            } catch { }
        }
    }
}
