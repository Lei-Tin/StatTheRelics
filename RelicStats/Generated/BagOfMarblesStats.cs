using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;
using System;
using System.Linq;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BagOfMarblesStats : BaseRelicStats {
        static int? cachedVulnerablePerFlash;

        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BagOfMarbles";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            var flashes = counters.TryGetValue("Flashes", out var e) ? e : 0;
            var vulnerablePerFlash = ResolveVulnerablePerFlash();
            var vulnerable = flashes * vulnerablePerFlash;
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Vulnerable Applied: {vulnerable}");
            return sb.ToString().TrimEnd();
        }

        static int ResolveVulnerablePerFlash() {
            if (cachedVulnerablePerFlash.HasValue) return cachedVulnerablePerFlash.Value;

            try {
                var type = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Select(a => a.GetType("MegaCrit.Sts2.Core.Models.Relics.BagOfMarbles", false))
                    .FirstOrDefault(t => t != null);

                if (type == null) {
                    cachedVulnerablePerFlash = 0;
                    return 0;
                }

                var relic = Activator.CreateInstance(type, true);
                if (relic == null) {
                    cachedVulnerablePerFlash = 0;
                    return 0;
                }

                var dynamicVars = ReflectionUtil.GetMemberValue(relic, "DynamicVars");
                var vulnerableVar = ReflectionUtil.GetMemberValue(dynamicVars, "Vulnerable");
                var baseValueRaw = ReflectionUtil.GetMemberValue(vulnerableVar, "BaseValue");
                var vulnerablePerFlash = baseValueRaw == null ? 0 : Math.Max(0, Convert.ToInt32(baseValueRaw));

                cachedVulnerablePerFlash = vulnerablePerFlash;
                return vulnerablePerFlash;
            } catch {
                cachedVulnerablePerFlash = 0;
                return 0;
            }
        }
    }
}