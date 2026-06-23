using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class GlassEyeStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.GlassEye";
        public override IReadOnlyList<string> DefaultCounters => System.Array.Empty<string>();

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Cards Obtained:");
            sb.Append(textStats != null && textStats.TryGetValue("Cards Obtained", out var cards) && !string.IsNullOrWhiteSpace(cards) ? cards : "None");
            return sb.ToString().TrimEnd();
        }
    }
}
