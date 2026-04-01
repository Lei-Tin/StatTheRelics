using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class CrackedCoreStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.CrackedCore";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Lightning Orbs Channeled" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var lightningOrbsChanneled = counters.TryGetValue("Lightning Orbs Channeled", out var m) ? m : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Lightning Orbs Channeled: {lightningOrbsChanneled}");
            return sb.ToString().TrimEnd();
        }
    }
}