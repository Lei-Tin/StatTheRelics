using System.Collections.Generic;
using System.Text;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class TouchOfOrobasStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.TouchOfOrobas";
        public override IReadOnlyList<string> DefaultCounters => new string[0];

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var starter = textStats != null && textStats.TryGetValue("Starter Relic", out var starterValue) && !string.IsNullOrWhiteSpace(starterValue)
                ? starterValue
                : "None";
            var upgraded = textStats != null && textStats.TryGetValue("Upgraded Relic", out var upgradedValue) && !string.IsNullOrWhiteSpace(upgradedValue)
                ? upgradedValue
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Starter Relic:");
            sb.AppendLine(starter);
            sb.AppendLine("Upgraded Relic:");
            sb.Append(upgraded);
            return sb.ToString().TrimEnd();
        }
    }
}
