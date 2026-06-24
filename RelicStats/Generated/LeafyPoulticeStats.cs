using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class LeafyPoulticeStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.LeafyPoultice";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Max HP Lost" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var removed = textStats != null && textStats.TryGetValue("Cards Transformed", out var removedValue) && !string.IsNullOrWhiteSpace(removedValue)
                ? removedValue
                : "None";
            var added = textStats != null && textStats.TryGetValue("Cards Obtained", out var addedValue) && !string.IsNullOrWhiteSpace(addedValue)
                ? addedValue
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine(FormatDefault(DefaultCounters, counters, false, string.Empty));
            sb.AppendLine();
            sb.AppendLine("Cards Transformed:");
            sb.AppendLine(removed);
            sb.AppendLine();
            sb.AppendLine("Cards Obtained:");
            sb.Append(added);
            return sb.ToString().TrimEnd();
        }
    }
}
