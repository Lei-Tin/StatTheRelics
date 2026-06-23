using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BoomingConchStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.BoomingConch";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Cards Drawn", "Energy Gained" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var cardsDrawn = counters.TryGetValue("Cards Drawn", out var m) ? m : 0;
            var energyGained = counters.TryGetValue("Energy Gained", out var e) ? e : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Cards Drawn: {cardsDrawn}");
            sb.AppendLine($"Energy Gained: {energyGained}");
            return sb.ToString().TrimEnd();
        }
    }
}
