using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BreadStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Bread";
        public override IReadOnlyList<string> DefaultCounters => new [] {
            "Energy Lost",
            "Energy Gained"
        };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            var energyLost = counters.TryGetValue("Energy Lost", out var d) ? d : 0;
            var energyGained = counters.TryGetValue("Energy Gained", out var g) ? g : 0;

            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Energy Lost: {energyLost}");
            sb.AppendLine($"Energy Gained: {energyGained}");
            return sb.ToString().TrimEnd();
        }
    }
}