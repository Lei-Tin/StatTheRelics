using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;
using System;
using System.Linq;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BookOfFiveRingsStats : BaseRelicStats {
        static int? cachedHealPerFlash;

        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BookOfFiveRings";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();

            var flashes = counters.TryGetValue("Flashes", out var e) ? e : 0;
            var healPerFlash = ResolveHealPerFlash();
            var hpHealed = flashes * healPerFlash;

            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"HP Healed: {hpHealed}");
            return sb.ToString().TrimEnd();
        }

        static int ResolveHealPerFlash() {
            if (cachedHealPerFlash.HasValue) return cachedHealPerFlash.Value;

            try {
                var type = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Select(a => a.GetType("MegaCrit.Sts2.Core.Models.Relics.BookOfFiveRings", false))
                    .FirstOrDefault(t => t != null);

                if (type == null) {
                    cachedHealPerFlash = 0;
                    return 0;
                }

                var relic = Activator.CreateInstance(type, true);
                if (relic == null) {
                    cachedHealPerFlash = 0;
                    return 0;
                }

                var dynamicVars = ReflectionUtil.GetMemberValue(relic, "DynamicVars");
                var healVar = ReflectionUtil.GetMemberValue(dynamicVars, "Heal");
                var baseValueRaw = ReflectionUtil.GetMemberValue(healVar, "BaseValue");
                var healPerFlash = baseValueRaw == null ? 0 : Math.Max(0, Convert.ToInt32(baseValueRaw));

                cachedHealPerFlash = healPerFlash;
                return healPerFlash;
            } catch {
                cachedHealPerFlash = 0;
                return 0;
            }
        }
    }
}