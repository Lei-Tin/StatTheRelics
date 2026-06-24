using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;

namespace StatTheRelics.Patches.Relics {
    [HarmonyPatch(typeof(TeaOfDiscourtesy), nameof(TeaOfDiscourtesy.BeforeCombatStart))]
    public static class TeaOfDiscourtesyPatch {
        const string TypeName = "MegaCrit.Sts2.Core.Models.Relics.TeaOfDiscourtesy";

        class State {
            public int DazedAdded { get; set; }
        }

        static void Prefix(TeaOfDiscourtesy __instance, ref object __state) {
            try {
                if (__instance == null || ReflectionUtil.GetIntMemberValue(__instance, "CombatsLeft") <= 0) return;
                __state = new State {
                    DazedAdded = Math.Max(0, ReflectionUtil.GetDynamicVarIntValue(__instance, "DazedCount", 2))
                };
            } catch { }
        }

        static void Postfix(TeaOfDiscourtesy __instance, Task __result, object __state) {
            try {
                if (__state is not State state || state.DazedAdded <= 0) return;

                if (__result == null) {
                    RelicTracker.AddAmountByType(TypeName, "Dazed Added", state.DazedAdded);
                    return;
                }

                __result.ContinueWith(task => {
                    try {
                        if (task.Status == TaskStatus.RanToCompletion) RelicTracker.AddAmountByType(TypeName, "Dazed Added", state.DazedAdded);
                    } catch { }
                });
            } catch { }
        }
    }
}
