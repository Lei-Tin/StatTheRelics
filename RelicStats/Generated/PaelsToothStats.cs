using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class PaelsToothStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.PaelsTooth";
        public override IReadOnlyList<string> DefaultCounters => System.Array.Empty<string>();

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var chosen = textStats != null && textStats.TryGetValue("Cards Chosen", out var chosenValue) && !string.IsNullOrWhiteSpace(chosenValue)
                ? chosenValue
                : "None";

            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Cards Chosen:");
            sb.Append(chosen);
            return sb.ToString().TrimEnd();
        }
    }
}
