using System.Collections.Generic;
using System.Text;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BookmarkStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Bookmark";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Cost Decreased" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var costDecreased = counters.TryGetValue("Cost Decreased", out var c) ? c : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Cost Decreased: {costDecreased}");
            return sb.ToString().TrimEnd();
        }
    }
}
