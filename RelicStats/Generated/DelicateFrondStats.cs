using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class DelicateFrondStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.DelicateFrond";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Potions Given" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine(FormatDefault(DefaultCounters, counters, false, string.Empty));
            sb.AppendLine();
            sb.AppendLine("Potions:");
            sb.Append(textStats != null && textStats.TryGetValue("Potions", out var potions) && !string.IsNullOrWhiteSpace(potions) ? potions : "None");
            return sb.ToString().TrimEnd();
        }
    }
}
