using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class ScrollBoxesStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.ScrollBoxes";
        public override IReadOnlyList<string> DefaultCounters => new string[0];

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var offered = textStats != null && textStats.TryGetValue("Cards Offered", out var offeredValue) && !string.IsNullOrWhiteSpace(offeredValue)
                ? offeredValue
                : "None";
            var added = textStats != null && textStats.TryGetValue("Cards Added", out var addedValue) && !string.IsNullOrWhiteSpace(addedValue)
                ? addedValue
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Cards Offered:");
            sb.AppendLine(offered);
            sb.AppendLine();
            sb.AppendLine("Cards Added:");
            sb.Append(added);
            return sb.ToString().TrimEnd();
        }
    }
}
