using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class CursedPearlStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.CursedPearl";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Gold Gained" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine(FormatDefault(DefaultCounters, counters, false, string.Empty));
            sb.AppendLine();
            sb.AppendLine("Curse:");
            sb.Append(textStats != null && textStats.TryGetValue("Curse", out var curse) && !string.IsNullOrWhiteSpace(curse) ? curse : "Unknown");
            return sb.ToString().TrimEnd();
        }
    }
}
