using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class ShovelStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Shovel";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Relics Dug" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var digs = counters != null && counters.TryGetValue("Relics Dug", out var digValue) ? digValue : 0;
            var relics = textStats != null && textStats.TryGetValue("Relics Dug Up", out var relicValue) && !string.IsNullOrWhiteSpace(relicValue)
                ? relicValue
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Relics Dug: {digs}");
            sb.AppendLine();
            sb.AppendLine("Relics Dug Up:");
            sb.Append(relics);
            return sb.ToString().TrimEnd();
        }
    }
}
