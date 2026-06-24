using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class GnarledHammerStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.GnarledHammer";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Enchanted Cards Played" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            var body = FormatDefault(DefaultCounters, counters, historyMode, bannerNote);
            if (!string.IsNullOrWhiteSpace(body)) {
                sb.AppendLine(body);
                sb.AppendLine();
            }
            sb.AppendLine("Enchanted Cards:");
            sb.Append(textStats != null && textStats.TryGetValue("Enchanted Cards", out var cards) && !string.IsNullOrWhiteSpace(cards) ? cards : "None");
            return sb.ToString().TrimEnd();
        }
    }
}
