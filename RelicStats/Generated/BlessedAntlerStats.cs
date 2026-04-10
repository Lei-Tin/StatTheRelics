using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;
using System;
using System.Linq;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BlessedAntlerStats : BaseRelicStats {
        static int? cachedDazedPerFlash;
        static int? cachedEnergyPerFlash;

        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BlessedAntler";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();

            // Dazed amount comes from DynamicVars.Cards.IntValue on the relic definition.
            var flashes = counters.TryGetValue("Flashes", out var e) ? e : 0;
            var energyPerFlash = ResolveEnergyPerFlash();
            var energyGiven = flashes * energyPerFlash;
            var dazedPerFlash = ResolveDazedPerFlash();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Energy Given: {energyGiven}");
            sb.AppendLine($"Dazed Given: {flashes * dazedPerFlash}");
            return sb.ToString().TrimEnd();
        }

        static int ResolveEnergyPerFlash() {
            if (cachedEnergyPerFlash.HasValue) return cachedEnergyPerFlash.Value;

            try {
                var type = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Select(a => a.GetType("MegaCrit.Sts2.Core.Models.Relics.BlessedAntler", false))
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
                var intValueRaw = ReflectionUtil.GetMemberValue(energyVar, "IntValue");
                var energyPerFlash = intValueRaw == null ? 0 : Math.Max(0, Convert.ToInt32(intValueRaw));

                cachedEnergyPerFlash = energyPerFlash;
                return energyPerFlash;
            } catch {
                cachedEnergyPerFlash = 0;
                return 0;
            }
        }

        static int ResolveDazedPerFlash() {
            if (cachedDazedPerFlash.HasValue) return cachedDazedPerFlash.Value;

            try {
                var type = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Select(a => a.GetType("MegaCrit.Sts2.Core.Models.Relics.BlessedAntler", false))
                    .FirstOrDefault(t => t != null);

                if (type == null) {
                    cachedDazedPerFlash = 0;
                    return 0;
                }

                var relic = Activator.CreateInstance(type, true);
                if (relic == null) {
                    cachedDazedPerFlash = 0;
                    return 0;
                }

                var dynamicVars = ReflectionUtil.GetMemberValue(relic, "DynamicVars");
                var cardsVar = ReflectionUtil.GetMemberValue(dynamicVars, "Cards");
                var intValueRaw = ReflectionUtil.GetMemberValue(cardsVar, "IntValue");
                var dazedPerFlash = intValueRaw == null ? 0 : Math.Max(0, Convert.ToInt32(intValueRaw));

                cachedDazedPerFlash = dazedPerFlash;
                return dazedPerFlash;
            } catch {
                cachedDazedPerFlash = 0;
                return 0;
            }
        }
    }
}