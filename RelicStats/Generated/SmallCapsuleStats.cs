using System.Collections.Generic;
using System.Text;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class SmallCapsuleStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.SmallCapsule";
        public override IReadOnlyList<string> DefaultCounters => System.Array.Empty<string>();

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var relics = textStats != null && textStats.TryGetValue("Relic Offered", out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Relic Offered:");
            sb.Append(relics);
            return sb.ToString().TrimEnd();
        }
    }
}
