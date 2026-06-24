using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class PaelsWingStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.PaelsWing";
        public override IReadOnlyList<string> DefaultCounters => System.Array.Empty<string>();

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var relics = textStats != null && textStats.TryGetValue("Relics Given", out var relicsValue) && !string.IsNullOrWhiteSpace(relicsValue)
                ? relicsValue
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Relics Given:");
            sb.Append(relics);
            return sb.ToString().TrimEnd();
        }
    }
}
