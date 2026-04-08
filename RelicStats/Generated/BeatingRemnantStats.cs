using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BeatingRemnantStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BeatingRemnant";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Damage Mitigated" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var mitigated = counters.TryGetValue("Damage Mitigated", out var m) ? m : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Damage Mitigated: {mitigated}");
            return sb.ToString().TrimEnd();
        }
    }
}