using System.Collections.Generic;
using StatTheRelics.RelicStats;
using System.Text;

namespace StatTheRelics.RelicStats.Generated {
    internal sealed class BellowsStats : BaseRelicStats {
        public override string TypeName => "MegaCrit.Sts2.Core.Models.Relics.Bellows";
        public override IReadOnlyList<string> DefaultCounters => new [] { "Cards Upgraded" };

        public override string Format(IReadOnlyDictionary<string,int> counters, bool historyMode, string bannerNote) {
            var cardsUpgraded = counters.TryGetValue("Cards Upgraded", out var m) ? m : 0;
            var sb = new StringBuilder();
            if (historyMode && !string.IsNullOrEmpty(bannerNote)) sb.AppendLine(bannerNote);
            sb.AppendLine($"Cards Upgraded: {cardsUpgraded}");
            return sb.ToString().TrimEnd();
        }
    }
}