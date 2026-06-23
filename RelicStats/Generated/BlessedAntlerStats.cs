using System.Collections.Generic;
using System.Text;
using StatTheRelics.RelicStats;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BlessedAntlerStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BlessedAntler";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Energy Given", "Dazed Given" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var energyGiven = counters.TryGetValue("Energy Given", out var e) ? e : 0;
            var dazedGiven = counters.TryGetValue("Dazed Given", out var d) ? d : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Energy Given: {energyGiven}");
            sb.AppendLine($"Dazed Given: {dazedGiven}");
            return sb.ToString().TrimEnd();
        }
    }
}
