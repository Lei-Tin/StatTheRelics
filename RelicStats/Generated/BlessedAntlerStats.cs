using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BlessedAntlerStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BlessedAntler";
        public override IReadOnlyList<string> DefaultCounters => DefaultFlashes;

        public override string Format(IReadOnlyDictionary<string,int> counters, IReadOnlyDictionary<string,string> textStats, bool historyMode, string bannerNote) {
            var sb = new StringBuilder();

            // Every time it flashes, it gives 3 dazed into your hand
            var flashes = counters.TryGetValue("Flashes", out var e) ? e : 0;
            var energyGiven = counters.TryGetValue("Energy Given", out var g) ? g : 0;
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Energy Given: {energyGiven}");
            sb.AppendLine($"Dazed Given: {flashes * 3}");
            return sb.ToString().TrimEnd();
        }
    }
}