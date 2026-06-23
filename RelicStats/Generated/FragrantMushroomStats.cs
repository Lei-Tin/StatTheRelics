using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class FragrantMushroomStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.FragrantMushroom";
        public override IReadOnlyList<string> DefaultCounters => new [] { "HP Lost" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine(FormatDefault(DefaultCounters, counters, false, string.Empty));
            sb.AppendLine();
            sb.AppendLine("Cards Upgraded:");
            sb.Append(textStats != null && textStats.TryGetValue("Cards Upgraded", out var cards) && !string.IsNullOrWhiteSpace(cards) ? cards : "None");
            return sb.ToString().TrimEnd();
        }
    }
}
