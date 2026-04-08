using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class ArchaicToothStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.ArchaicTooth";
        public override IReadOnlyList<string> DefaultCounters => System.Array.Empty<string>();

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var lost = textStats != null && textStats.TryGetValue("Cards Lost", out var l) && !string.IsNullOrWhiteSpace(l)
                ? l
                : "Unknown";

            var obtained = textStats != null && textStats.TryGetValue("Cards Obtained", out var o) && !string.IsNullOrWhiteSpace(o)
                ? o
                : "Unknown";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);

            sb.AppendLine("Cards Lost:");
            sb.AppendLine(lost);
            sb.AppendLine();
            sb.AppendLine("Cards Obtained:");
            sb.AppendLine(obtained);
            return sb.ToString().TrimEnd();
        }
    }
}