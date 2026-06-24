using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class HeftyTabletStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.HeftyTablet";
        public override IReadOnlyList<string> DefaultCounters => System.Array.Empty<string>();

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Offered Cards:");
            sb.AppendLine(textStats != null && textStats.TryGetValue("Offered Cards", out var offered) && !string.IsNullOrWhiteSpace(offered) ? offered : "None");
            sb.AppendLine();
            sb.AppendLine("Card Added:");
            sb.Append(textStats != null && textStats.TryGetValue("Card Added", out var cards) && !string.IsNullOrWhiteSpace(cards) ? cards : "None");
            return sb.ToString().TrimEnd();
        }
    }
}
