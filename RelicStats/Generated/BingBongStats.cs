using System.Collections.Generic;
using System.Text;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BingBongStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BingBong";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Cards Added" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var cardsAdded = counters.TryGetValue("Cards Added", out var c) ? c : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Cards Added: {cardsAdded}");
            return sb.ToString().TrimEnd();
        }
    }
}
