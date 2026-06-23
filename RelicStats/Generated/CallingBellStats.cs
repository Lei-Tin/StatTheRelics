using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class CallingBellStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.CallingBell";
        public override IReadOnlyList<string> DefaultCounters => System.Array.Empty<string>();

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var curse = textStats != null && textStats.TryGetValue("Curse", out var c) && !string.IsNullOrWhiteSpace(c)
                ? c
                : "Unknown";
            var relics = textStats != null && textStats.TryGetValue("Relics Offered", out var r) && !string.IsNullOrWhiteSpace(r)
                ? r
                : "Unknown";
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine("Curse:");
            sb.AppendLine(curse);
            sb.AppendLine();
            sb.AppendLine("Relics Offered:");
            sb.AppendLine(relics);
            return sb.ToString().TrimEnd();
        }
    }
}
