using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BookmarkStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Bookmark";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();

            // Every time it flashes, it decreased a cost
            var flashes = counters.TryGetValue("Flashes", out var e) ? e : 0;
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Cost Decreased: {flashes}");
            return sb.ToString().TrimEnd();
        }
    }
}