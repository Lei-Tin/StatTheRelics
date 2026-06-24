using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class WongosMysteryTicketStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.WongosMysteryTicket";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Combats Finished", "Relic Rewards Added" };

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine(FormatDefault(DefaultCounters, counters, false, string.Empty));
            sb.AppendLine();
            sb.AppendLine("Relics Added:");
            sb.Append(textStats != null && textStats.TryGetValue("Relics Added", out var relics) && !string.IsNullOrWhiteSpace(relics) ? relics : "None");
            return sb.ToString().TrimEnd();
        }
    }
}
