using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class PandorasBoxStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.PandorasBox";
        public override IReadOnlyList<string> DefaultCounters => System.Array.Empty<string>();

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var obtained = textStats != null && textStats.TryGetValue("Cards Obtained", out var obtainedValue) && !string.IsNullOrWhiteSpace(obtainedValue)
                ? obtainedValue
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Cards Obtained:");
            sb.Append(obtained);
            return sb.ToString().TrimEnd();
        }
    }
}
