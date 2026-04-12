using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;
using System;
using System.Linq;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class AkabekoStats : BaseRelicStats {
        static int? cachedVigorPerFlash;

        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Akabeko";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            var flashes = counters.TryGetValue("Flashes", out var e) ? e : 0;
            var vigorPerFlash = ResolveVigorPerFlash();
            var vigorGiven = flashes * vigorPerFlash;

            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Vigor Given: {vigorGiven}");
            return sb.ToString().TrimEnd();
        }

        static int ResolveVigorPerFlash() {
            if (cachedVigorPerFlash.HasValue) return cachedVigorPerFlash.Value;

            try {
                var type = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Select(a => a.GetType("MegaCrit.Sts2.Core.Models.Relics.Akabeko", false))
                    .FirstOrDefault(t => t != null);

                if (type == null) {
                    cachedVigorPerFlash = 0;
                    return 0;
                }

                var relic = Activator.CreateInstance(type, true);
                if (relic == null) {
                    cachedVigorPerFlash = 0;
                    return 0;
                }

                var dynamicVars = ReflectionUtil.GetMemberValue(relic, "DynamicVars");
                var vigorVar = ReflectionUtil.GetMemberValue(dynamicVars, "VigorPower");
                var baseValueRaw = ReflectionUtil.GetMemberValue(vigorVar, "BaseValue");
                var vigorPerFlash = baseValueRaw == null ? 0 : Math.Max(0, Convert.ToInt32(baseValueRaw));

                cachedVigorPerFlash = vigorPerFlash;
                return vigorPerFlash;
            } catch {
                cachedVigorPerFlash = 0;
                return 0;
            }
        }
    }
}