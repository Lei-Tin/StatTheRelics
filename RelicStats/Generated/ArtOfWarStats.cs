using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;
using System;
using System.Linq;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class ArtOfWarStats : BaseRelicStats {
        static int? cachedEnergyPerFlash;

        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.ArtOfWar";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            var flashes = counters.TryGetValue("Flashes", out var e) ? e : 0;
            var energyPerFlash = ResolveEnergyPerFlash();
            var energy = flashes * energyPerFlash;
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Energy Given: {energy}");
            return sb.ToString().TrimEnd();
        }

        static int ResolveEnergyPerFlash() {
            if (cachedEnergyPerFlash.HasValue) return cachedEnergyPerFlash.Value;

            try {
                var type = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Select(a => a.GetType("MegaCrit.Sts2.Core.Models.Relics.ArtOfWar", false))
                    .FirstOrDefault(t => t != null);

                if (type == null) {
                    cachedEnergyPerFlash = 0;
                    return 0;
                }

                var relic = Activator.CreateInstance(type, true);
                if (relic == null) {
                    cachedEnergyPerFlash = 0;
                    return 0;
                }

                var dynamicVars = ReflectionUtil.GetMemberValue(relic, "DynamicVars");
                var energyVar = ReflectionUtil.GetMemberValue(dynamicVars, "Energy");
                var baseValueRaw = ReflectionUtil.GetMemberValue(energyVar, "BaseValue");
                var energyPerFlash = baseValueRaw == null ? 0 : Math.Max(0, Convert.ToInt32(baseValueRaw));

                cachedEnergyPerFlash = energyPerFlash;
                return energyPerFlash;
            } catch {
                cachedEnergyPerFlash = 0;
                return 0;
            }
        }
    }
}