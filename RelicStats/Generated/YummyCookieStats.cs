using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class YummyCookieStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.YummyCookie";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Cards Upgraded" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine(FormatDefault(DefaultCounters, counters, false, string.Empty));
            sb.AppendLine();
            sb.AppendLine("Upgraded Cards:");
            sb.Append(textStats != null && textStats.TryGetValue("Cards Upgraded", out var cards) && !string.IsNullOrWhiteSpace(cards) ? cards : "None");
            return sb.ToString().TrimEnd();
        }
    }
}
